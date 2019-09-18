using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL4;

namespace ShaderTranslator.Demo
{
    using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

    class SimpleRenderer
    {
        int program;

        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

        public void Load(Delegate vertexShaderMethod, Delegate pixelShaderMethod)
        {
            int vs = ShaderCompiler.CompileVertexShader(vertexShaderMethod);
            int fs = ShaderCompiler.CompileFragmentShader(pixelShaderMethod);
            program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                throw new Exception(log);
            }
        }

        public struct RenderContext : IDisposable
        {
            SimpleRenderer parent;
            internal RenderContext(SimpleRenderer parent)
            {
                this.parent = parent;
            }
            public void Draw(int vao, int count)
            {
                GL.BindVertexArray(vao);
                GL.DrawArrays(parent.PrimitiveType, 0, count);
            }
            void IDisposable.Dispose()
            {
                GL.UseProgram(0);
            }
        }

        public RenderContext Begin()
        {
            GL.UseProgram(program);
            return new RenderContext(this);
        }
    }
}
