﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;


#line 3 "D:\Code\Paradox\sources\engine\SiliconStudio.Paradox.Shaders.Tests\GameAssets\Mixins\test_mixin_simple_clone.pdxfx"
namespace Test5
{

    #line 5
    internal static partial class ShaderMixins
    {
        internal partial class ChildClone  : IShaderMixinBuilderExtended
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {

                #line 7
                context.CloneProperties();

                #line 7
                mixin.Mixin.CloneFrom(mixin.Parent.Mixin);

                #line 8
                context.Mixin(mixin, "C1");

                #line 9
                context.Mixin(mixin, "C2");
            }
            private readonly ParameterKey[] __keys__ = new ParameterKey[]
            {
            };
            public ParameterKey[] Keys
            {
                get
                {
                    return __keys__;
                }
            }
            private readonly string[] __mixins__ = new []
            {
                "C1",
                "C2",
            };
            public string[] Mixins
            {
                get
                {
                    return __mixins__;
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ChildClone", new ChildClone());
            }
        }
    }

    #line 12
    internal static partial class ShaderMixins
    {
        internal partial class DefaultSimpleClone  : IShaderMixinBuilderExtended
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {

                #line 14
                context.Mixin(mixin, "A");

                #line 15
                context.Mixin(mixin, "B");

                #line 16
                context.Mixin(mixin, "C");

                {

                    #line 18
                    var __subMixin = new ShaderMixinSourceTree() { Name = "Test", Parent = mixin };
                    mixin.Children.Add(__subMixin);

                    #line 18
                    context.BeginChild(__subMixin);

                    #line 18
                    context.Mixin(__subMixin, "ChildClone");

                    #line 18
                    context.EndChild();
                }
            }
            private readonly ParameterKey[] __keys__ = new ParameterKey[]
            {
            };
            public ParameterKey[] Keys
            {
                get
                {
                    return __keys__;
                }
            }
            private readonly string[] __mixins__ = new []
            {
                "A",
                "B",
                "C",
                "ChildClone",
                "Test",
            };
            public string[] Mixins
            {
                get
                {
                    return __mixins__;
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("DefaultSimpleClone", new DefaultSimpleClone());
            }
        }
    }
}
