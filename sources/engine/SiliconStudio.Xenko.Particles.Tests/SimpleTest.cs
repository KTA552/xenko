﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class SimpleTest
    {
        [Test]
        public void OnePlusOne()
        {
            var i = 1;
            i++;

            Assert.That(i, Is.EqualTo(2));
        }

        [Test]
        public void TestEmitter()
        {
            var dummySystem = new ParticleSystem();

            var emitter = new ParticleEmitter();
            emitter.MaxParticlesOverride = 10;
            emitter.ParticleMaxLifetime = 1;
            emitter.ParticleMinLifetime = 1;
            emitter.EmitParticles(5);

            emitter.Update(0.016f, dummySystem);

            Assert.That(emitter.pool.LivingParticles, Is.EqualTo(5));
        }
    }
}
