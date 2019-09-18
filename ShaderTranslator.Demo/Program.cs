using System;
using System.Numerics;
using ShaderTranslator.Syntax;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

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
            simpleRenderer.Load(form.Device, vsMethod, psMethod);
            simpleRenderer.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            var vertexBuffer = Buffer.Create(form.Device, BindFlags.VertexBuffer, new Vector2[] {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0),
                new Vector2(0, 0.5f),
                new Vector2(0, 0),
            });

            form.DoRenderLoop(context =>
            {
                using (var renderContext = simpleRenderer.Begin(context))
                    renderContext.Draw(vertexBuffer);
            });
        }
    }
}
