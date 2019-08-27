using NUnit.Framework;
using System;

namespace ShaderTranslator.Test
{
    public class Basic
    {
        public int SimpleDummyShader(float input)
        {
            int x = 2;
            int y = x + 1;
            return y;
        }
        [Test]
        public void ILSpyDecompilationWorks()
        {
            var result = new ILSpyManager().GetSyntaxTree(typeof(Basic).GetMethod(nameof(SimpleDummyShader)));
            Assert.Pass();
        }

        [Test]
        public void BasicShaderCompilationWorks()
        {
            var engine = new CompileEngine(SymbolResolver.Default);
            Func<float, int> func = SimpleDummyShader;
            string result = engine.Compile(func.Target, func.Method, new SemanticsGenerator(), out _);
        }
    }
}