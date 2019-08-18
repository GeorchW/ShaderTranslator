using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class Base
    {
        protected CompileEngine engine;
        [SetUp]
        public void Setup() => engine = new CompileEngine();

        protected void TestPixelShader<TIn, TOut>(Func<TIn, TOut> shaderFunction)
        {
            string code = engine.Compile(shaderFunction, out string entryPoint);
            SharpDX.D3DCompiler.ShaderBytecode.Compile(code, entryPoint, "ps_5_0");
        }
    }
}
