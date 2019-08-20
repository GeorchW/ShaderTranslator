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
        public void Setup() => engine = new CompileEngine(SymbolResolver.Default);

        protected void TestPixelShader<TIn, TOut>(Func<TIn, TOut> func) => TestPixelShader((Delegate)func);
        protected void TestPixelShader<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func) => TestPixelShader((Delegate)func);

        protected void TestPixelShader(Delegate shaderFunction)
        {
            string code = engine.Compile(shaderFunction.Target, shaderFunction.Method, out string entryPoint);
            SharpDX.D3DCompiler.ShaderBytecode.Compile(code, entryPoint, "ps_5_0");
        }
    }
}
