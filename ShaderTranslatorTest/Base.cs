using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
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

        protected void TestPixelShader(Delegate shaderFunction)
        {
            var result = engine.Compile(shaderFunction.Target, shaderFunction.Method,
                new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.PixelShader));

            StringBuilder shader = new StringBuilder();
            shader.AppendLine("#version 450");
            shader.AppendLine(result.Code);
            foreach (var param in result.EntryPoint.Parameters)
            {
                shader.AppendLine($"in {param.Type.Name} {param.Semantics};");
            }
            bool singleOutput = result.EntryPoint.ReturnType.Semantics != null;
            string returnType = result.EntryPoint.ReturnType.Type.Name;
            if (singleOutput)
            {
                shader.AppendLine($"out {returnType} {result.EntryPoint.ReturnType.Semantics};");
            }
            else
            {
                foreach (var field in ((StructTargetType)result.EntryPoint.ReturnType.Type).Fields)
                {
                    shader.AppendLine($"out {field.Type.Name} {field.Name};");
                }
            }
            shader.AppendLine("void main()");
            string callEntryPoint = $"{result.EntryPoint.Name}({string.Join(",", result.EntryPoint.Parameters.Select(p => p.Semantics))});";
            shader.AppendLine("{");
            if (singleOutput)
            {
                shader.AppendLine($"    {result.EntryPoint.ReturnType.Semantics} = {callEntryPoint}");
            }
            else
            {
                shader.AppendLine($"    {returnType} result = {callEntryPoint}");
            }
            shader.AppendLine("}");

            var startInfo = new ProcessStartInfo(@"E:\Programmieren\glslang\build\StandAlone\Debug\glslangValidator.exe",
                "--stdin " +
                "-S frag " +
                "-G100 " +
                "-d " +
                "--auto-map-locations ");
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            var process = Process.Start(startInfo);
            string code = shader.ToString();
            process.StandardInput.Write(code);
            process.StandardInput.Close();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                string output = process.StandardOutput.ReadToEnd();
                Assert.Fail(error + output);
            }
        }

        protected void TestVertexShader(Delegate shaderFunction)
        {
            var result = engine.Compile(shaderFunction.Target, shaderFunction.Method,
                new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.VertexShader));
            SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "vs_5_0");
        }
    }
}
