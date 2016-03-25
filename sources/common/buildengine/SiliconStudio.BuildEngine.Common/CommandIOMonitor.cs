﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// This class monitors input/output access from every BuildStep execution, and display an error message if an object url is the input of a command and the output of another command running at the same time.
    /// </summary>
    internal class CommandIOMonitor
    {
        /// <summary>
        /// A dictionary containing read and write access timings (value) of a given object url (key)
        /// </summary>
        private readonly Dictionary<ObjectUrl, ObjectAccesses> objectsAccesses = new Dictionary<ObjectUrl, ObjectAccesses>();

        /// <summary>
        /// A dictionary containing execution intervals of BuildStep
        /// </summary>
        private readonly Dictionary<CommandBuildStep, TimeInterval> commandExecutionIntervals = new Dictionary<CommandBuildStep, TimeInterval>();

        private readonly Dictionary<CommandBuildStep, List<ObjectUrl>> commandInputFiles = new Dictionary<CommandBuildStep, List<ObjectUrl>>();

        private readonly ILogger logger;

        private readonly object lockObject = new object();

        private readonly Stopwatch stopWatch = new Stopwatch();

        // Store earliest start time of command still running (to clean up accesses as time goes)
        private long earliestCommandAliveStartTime;

        public CommandIOMonitor(ILogger logger)
        {
            this.logger = logger;
            stopWatch.Start();
        }

        public void CommandStarted(CommandBuildStep command)
        {
            lock (lockObject)
            {
                long startTime = stopWatch.ElapsedTicks;
                commandExecutionIntervals.Add(command, new TimeInterval(startTime));

                // Get a list of unique input files
                var inputFiles = command.Command.GetInputFiles().Distinct().ToList();
                // Store it aside, so that we're sure to remove the same entries during CommandEnded
                commandInputFiles.Add(command, inputFiles);

                // Setup start read time for each file entry
                var inputHash = new HashSet<ObjectUrl>();
                foreach (ObjectUrl inputUrl in inputFiles)
                {
                    if (inputHash.Contains(inputUrl))
                        logger.Error("The command '{0}' has several times the file '{1}' as input. Input Files must not be duplicated", command.Title, inputUrl.Path);
                    inputHash.Add(inputUrl);

                    ObjectAccesses inputAccesses;
                    if (!objectsAccesses.TryGetValue(inputUrl, out inputAccesses))
                    {
                        objectsAccesses.Add(inputUrl, inputAccesses = new ObjectAccesses());
                    }
                    inputAccesses.Reads.Add(new TimeInterval<BuildStep>(command, startTime));
                }
            }
        }

        public void CommandEnded(CommandBuildStep command)
        {
            lock (lockObject)
            {
                TimeInterval commandInterval = commandExecutionIntervals[command];

                long startTime = commandInterval.StartTime;
                long endTime = stopWatch.ElapsedTicks;
                commandInterval.End(endTime);

                commandExecutionIntervals.Remove(command);

                foreach (var outputObject in command.Result.OutputObjects)
                {
                    var outputUrl = outputObject.Key;
                    ObjectAccesses inputAccess;
                    if (objectsAccesses.TryGetValue(outputUrl, out inputAccess))
                    {
                        foreach (TimeInterval<BuildStep> input in inputAccess.Reads.Where(input => input.Object != command && input.Overlap(startTime, endTime)))
                        {
                            logger.Error("Command {0} is writing {1} while command {2} is reading it", command, outputUrl, input.Object);
                        }
                    }

                    ObjectAccesses outputAccess;
                    if (!objectsAccesses.TryGetValue(outputUrl, out outputAccess))
                    {
                        objectsAccesses.Add(outputUrl, outputAccess = new ObjectAccesses());
                    }

                    foreach (var output in outputAccess.Writes.Where(output => output.Object.Key != command && output.Overlap(startTime, endTime)))
                    {
                        if (outputObject.Value != output.Object.Value)
                            logger.Error("Commands {0} and {1} are both writing {2} at the same time, but they are different objects", command, output.Object, outputUrl);
                    }

                    outputAccess.Writes.Add(new TimeInterval<KeyValuePair<BuildStep, ObjectId>>(new KeyValuePair<BuildStep, ObjectId>(command, outputObject.Value), startTime, endTime));
                }

                foreach (ObjectUrl inputUrl in command.Result.InputDependencyVersions.Keys)
                {
                    ObjectAccesses outputAccess;
                    if (objectsAccesses.TryGetValue(inputUrl, out outputAccess))
                    {
                        foreach (TimeInterval<KeyValuePair<BuildStep, ObjectId>> output in outputAccess.Writes.Where(output => output.Object.Key != command && output.Overlap(startTime, endTime)))
                        {
                            logger.Error("Command {0} is writing {1} while command {2} is reading it", output.Object, inputUrl, command);
                        }
                    }
                }

                // Notify that we're done reading input files
                List<ObjectUrl> inputFiles;
                if (commandInputFiles.TryGetValue(command, out inputFiles))
                {
                    commandInputFiles.Remove(command);
                    foreach (ObjectUrl input in inputFiles)
                    {
                        objectsAccesses[input].Reads.Single(x => x.Object == command).End(endTime);
                    }
                }

                // "Garbage collection" of accesses
                var newEarliestCommandAliveStartTime = commandExecutionIntervals.Count > 0 ? commandExecutionIntervals.Min(x => x.Value.StartTime) : endTime;
                if (newEarliestCommandAliveStartTime > earliestCommandAliveStartTime)
                {
                    earliestCommandAliveStartTime = newEarliestCommandAliveStartTime;

                    // We can remove objects whose all R/W accesses are "completed" (EndTime is set)
                    // and happened before all the current running commands started, since they won't affect us
                    foreach (var objectAccesses in objectsAccesses.ToList())
                    {
                        if (objectAccesses.Value.Reads.All(x => x.EndTime != 0 && x.EndTime < earliestCommandAliveStartTime)
                            && objectAccesses.Value.Writes.All(x => x.EndTime != 0 && x.EndTime < earliestCommandAliveStartTime))
                            objectsAccesses.Remove(objectAccesses.Key);
                    }
                }
            }
        }

        class ObjectAccesses
        {
            public List<TimeInterval<BuildStep>> Reads { get; } = new List<TimeInterval<BuildStep>>();
            public List<TimeInterval<KeyValuePair<BuildStep, ObjectId>>> Writes { get; } = new List<TimeInterval<KeyValuePair<BuildStep, ObjectId>>>();
        }
    }
}