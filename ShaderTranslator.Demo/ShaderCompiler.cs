using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;

namespace ShaderTranslator.Demo
{
    public static class ShaderCompiler
    {
        static CompileEngine compileEngine = new CompileEngine(SymbolResolver.Default);
        public static int CompileVertexShader(Delegate shaderMethod) => Compile(shaderMethod, ShaderType.VertexShader, OpenTK.Graphics.OpenGL4.ShaderType.VertexShader);

        private static int Compile(Delegate shaderMethod, ShaderType shaderType, OpenTK.Graphics.OpenGL4.ShaderType glShaderType)
        {
            var result = compileEngine.Compile(shaderMethod.Target, shaderMethod.Method, shaderType);
            int shader = GL.CreateShader(glShaderType);
            GL.ShaderSource(shader, result.Code);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                Console.WriteLine(log);
                throw new Exception(log);
            }
            return shader;
        }

        public static int CompileFragmentShader(Delegate shaderMethod) => Compile(shaderMethod, ShaderType.FragmentShader, OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader);//return shader;
    }
}
