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
            var result = engine.Compile(shaderFunction.Target, shaderFunction.Method,
                new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.PixelShader));
            SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "ps_5_0");
        }

        protected void TestVertexShader(Delegate shaderFunction)
        {
            var result = engine.Compile(shaderFunction.Target, shaderFunction.Method,
                new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.VertexShader));
            SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "vs_5_0");
        }
    }
}
