using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace ShaderTranslator.Test
{
    class Base
    {
        protected CompileEngine engine;
        [SetUp]
        public void Setup() => engine = new CompileEngine(SymbolResolver.Default);

        protected void TestPixelShader<TIn, TOut>(Func<TIn, TOut> func) => TestPixelShader((Delegate)func);
        protected void TestPixelShader<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func) => TestPixelShader((Delegate)func);


        protected void TestPixelShader(Delegate shaderFunction) => TestShader(shaderFunction, ShaderType.FragmentShader);
        protected void TestVertexShader(Delegate shaderFunction) => TestShader(shaderFunction, ShaderType.VertexShader);
        protected void TestShader(Delegate shaderFunction, ShaderType shaderType)
        {
            var result = engine.Compile(shaderFunction.Target, shaderFunction.Method, shaderType);

            var startInfo = new ProcessStartInfo("glslangValidator",
                "--stdin " +
                shaderType switch
                {
                    ShaderType.FragmentShader => "-S frag ",
                    ShaderType.VertexShader => "-S vert ",
                    _ => throw new NotImplementedException()
                } +
                "-G100 " +
                "-d " +
                "--auto-map-locations ");
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            var process = Process.Start(startInfo);
            process.StandardInput.Write(result.Code);
            process.StandardInput.Close();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                string output = process.StandardOutput.ReadToEnd();
                Assert.Fail(error + output);
            }
        }
    }
}
