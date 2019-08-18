using NUnit.Framework;

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
            var engine = new CompileEngine();
            string result = engine.Compile<float, int>(SimpleDummyShader, out _);
        }
    }
}