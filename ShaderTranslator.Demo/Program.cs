using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShaderTranslator.Syntax;

namespace ShaderTranslator.Demo
{
    class Program
    {
        struct VsOutput
        {
            public Vector2 TexCoord;
            [Glsl("gl_Position")]
            public Vector4 Position;
        }
        static void Main(string[] args)
        {
            var form = new SimpleRenderForm();

            Func<Vector2, VsOutput> vsMethod = (Vector2 input) => new VsOutput { Position = new Vector4(input, 0, 1), TexCoord = input };
            Func<Vector2, Vector4> psMethod = input => new Vector4(input * 2, 0, 1);
            var simpleRenderer = new SimpleRenderer();
            simpleRenderer.Load(vsMethod, psMethod);
            simpleRenderer.PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip;

            var data = new Vector2[] {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0),
                new Vector2(0, 0.5f),
                new Vector2(0, 0),
            };

            int vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            unsafe
            {
                var bytes = MemoryMarshal.AsBytes(data.AsSpan());
                fixed (void* ptr = bytes)
                {
                    GL.BufferStorage(BufferTarget.ArrayBuffer, bytes.Length, new IntPtr(ptr), BufferStorageFlags.None);
                }
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.EnableVertexArrayAttrib(vao, 1);
            GL.VertexArrayAttribBinding(vao, 1, vertexBuffer);
            GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 0);
            GL.VertexArrayVertexBuffer(vao, 1, vertexBuffer, IntPtr.Zero, 8);
            GL.BindVertexArray(0);

            form.DoRenderLoop(() =>
            {
                using (var renderContext = simpleRenderer.Begin())
                    renderContext.Draw(vao, 4);
            });
        }
    }
}
