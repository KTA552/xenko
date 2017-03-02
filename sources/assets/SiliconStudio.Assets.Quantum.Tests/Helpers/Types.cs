﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public static class Types
    {
        public const string FileExtension = ".xktest";

        [DataContract]
        public abstract class MyAssetBase : Asset
        {
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset1 : MyAssetBase
        {
            public string MyString { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset2 : MyAssetBase
        {
            public List<string> MyStrings { get; set; } = new List<string>();
            public StructWithList Struct = new StructWithList { MyStrings = new List<string>() };
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset3 : MyAssetBase
        {
            public Dictionary<string, string> MyDictionary { get; set; } = new Dictionary<string, string>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset4 : MyAssetBase
        {
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset5 : MyAssetBase
        {
            public List<IMyInterface> MyInterfaces { get; set; } = new List<IMyInterface>();
            public IMyInterface MyInterface { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset6 : MyAssetBase
        {
            public Dictionary<string, IMyInterface> MyDictionary { get; set; } = new Dictionary<string, IMyInterface>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset7 : MyAssetBase
        {
            public MyAsset2 MyAsset2 { get; set; }
            public MyAsset3 MyAsset3 { get; set; }
            public MyAsset4 MyAsset4 { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset8 : MyAssetBase
        {
            [NonIdentifiableCollectionItems]
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset9 : MyAssetBase
        {
            public SomeObject MyObject { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset10 : MyAssetBase
        {
            [DefaultValue(true)]
            public bool MyBool { get; set; } = true;
        }

        [DataContract]
        public class MyReferenceable : IIdentifiable
        {
            public MyReferenceable() { Id = Guid.NewGuid(); }
            public string Value { get; set; }
            [NonOverridable]
            public Guid Id { get; set; }
            public override string ToString() => $"[{Id}] {Value}";
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAssetWithRef : MyAssetBase
        {
            public MyReferenceable MyObject1 { get; set; }

            public MyReferenceable MyObject2 { get; set; }

            [DefaultValue(null)]
            public MyReferenceable MyObject3 { get; set; }

            public List<MyReferenceable> MyObjects { get; set; } = new List<MyReferenceable>();

            [NonIdentifiableCollectionItems]
            public List<MyReferenceable> MyNonIdObjects { get; set; } = new List<MyReferenceable>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAssetWithRef2 : MyAssetBase
        {
            public MyReferenceable NonReference { get; set; }

            public MyReferenceable Reference { get; set; }

            public List<MyReferenceable> References { get; set; } = new List<MyReferenceable>();

            public static int MemberCount => 4 + 3; // 4 (number of members in Asset) + 3 (number of members in Types.MyAssetWithRef2)
        }


        [DataContract]
        public struct StructWithList
        {
            public List<string> MyStrings { get; set; }
        }

        public interface IMyInterface
        {
            string Value { get; set; }
        }

        [DataContract]
        public class SomeObject : IMyInterface
        {
            public string Value { get; set; }
        }

        [DataContract]
        public class SomeObject2 : IMyInterface
        {
            public string Value { get; set; }
            public int Number { get; set; }
        }

        [AssetPropertyGraph(typeof(MyAssetWithRef))]
        public class AssetWithRefPropertyGraph : MyAssetBasePropertyGraph
        {
            public AssetWithRefPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
                : base(container, assetItem, logger)
            {
            }

            public static Func<IGraphNode, Index, bool> IsObjectReferenceFunc { get; set; }

            public override bool IsObjectReference(IGraphNode targetNode, Index index, object value)
            {
                return IsObjectReferenceFunc?.Invoke(targetNode, index) ?? base.IsObjectReference(targetNode, index, value);
            }
        }

        [AssetPropertyGraph(typeof(MyAssetWithRef2))]
        public class AssetWithRefPropertyGraph2 : MyAssetBasePropertyGraph
        {
            public AssetWithRefPropertyGraph2(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
                : base(container, assetItem, logger)
            {
            }

            public override bool IsObjectReference(IGraphNode targetNode, Index index, object value)
            {
                if ((targetNode as IMemberNode)?.Name == nameof(MyAssetWithRef2.Reference))
                    return true;
                if ((targetNode as IObjectNode)?.Retrieve() == ((MyAssetWithRef2)Asset).References)
                    return true;

                return false;
            }
        }

        // TODO: we don't want to have to do this to detect children!
        [DataContract]
        public class ChildrenList : List<MyPart> { }

        [DataContract("MyPart")]
        public class MyPart : IIdentifiable
        {
            [NonOverridable]
            public Guid Id { get; set; }
            public string Name { get; set; }
            public MyPart Parent { get; set; }
            public MyPart MyReference { get; set; }
            public List<MyPart> MyReferences { get; set; }
            public List<MyPart> Children { get; } = new List<MyPart>();
            public void AddChild([NotNull] MyPart child) { Children.Add(child); child.Parent = this; }
            public override string ToString() => $"{Name} [{Id}]";
        }

        [DataContract("MyPartDesign")]
        public class MyPartDesign : IAssetPartDesign<MyPart>
        {
            public BasePart Base { get; set; }
            // ReSharper disable once NotNullMemberIsNotInitialized
            public MyPart Part { get; set; }
            public override string ToString() => $"Design: {Part.Name} [{Part.Id}]";
        }

        public class MyAssetHierarchy : AssetCompositeHierarchy<MyPartDesign, MyPart>
        {
            public override MyPart GetParent(MyPart part) => part.Parent;
            public override int IndexOf(MyPart part) => GetParent(part)?.Children.IndexOf(part) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
            public override MyPart GetChild(MyPart part, int index) => part.Children[index];
            public override int GetChildCount(MyPart part) => part.Children.Count;
            public override IEnumerable<MyPart> EnumerateChildParts(MyPart part, bool isRecursive) => isRecursive ? part.Children.DepthFirst(t => t.Children) : part.Children;
            public AssetCompositeHierarchyData<MyPartDesign, MyPart> CreatePartInstances()
            {
                Dictionary<Guid, Guid> idRemapping;
                var instance = (MyAssetHierarchy)CreateDerivedAsset("", out idRemapping);
                var instanceId = instance.Hierarchy.Parts.FirstOrDefault()?.Base?.InstanceId ?? Guid.NewGuid();
                return instance.Hierarchy;
            }
        }

        [AssetPropertyGraph(typeof(MyAssetHierarchy))]
        // ReSharper disable once ClassNeverInstantiated.Local
        public class MyAssetHierarchyPropertyGraph : AssetCompositeHierarchyPropertyGraph<MyPartDesign, MyPart>
        {
            public MyAssetHierarchyPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger) { }
            public override bool IsChildPartReference(IGraphNode node, Index index) => node.Type == typeof(ChildrenList);
            protected override void AddChildPartToParentPart(MyPart parentPart, MyPart childPart, int index) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Add(childPart, new Index(index));
            protected override void RemoveChildPartFromParentPart(MyPart parentPart, MyPart childPart) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Remove(childPart, new Index(parentPart.Children.IndexOf(childPart)));
            protected override Guid GetIdFromChildPart(object part) => ((MyPart)part).Id;
            protected override IEnumerable<IGraphNode> RetrieveChildPartNodes(MyPart part)
            {
                yield return Container.NodeContainer.GetNode(part.Children);
            }
        }


        [AssetPropertyGraph(typeof(MyAssetBase))]
        public class MyAssetBasePropertyGraph : AssetPropertyGraph
        {
            private readonly Dictionary<IGraphNode, IGraphNode> customBases = new Dictionary<IGraphNode, IGraphNode>();

            public MyAssetBasePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
                : base(container, assetItem, logger)
            {
            }

            public void RegisterCustomBaseLink(IGraphNode node, IGraphNode baseNode)
            {
                customBases.Add(node, baseNode);
            }

            public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
            {
                IGraphNode baseNode;
                return customBases.TryGetValue(sourceNode, out baseNode) ? baseNode : base.FindTarget(sourceNode, target);
            }
        }
    }
}
