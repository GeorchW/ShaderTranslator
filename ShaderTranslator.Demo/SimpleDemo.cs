using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ShaderTranslator.Syntax;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using ShaderTexture2D = ShaderTranslator.Syntax.Texture2D;

namespace ShaderTranslator.Demo
{
    class SimpleDemo
    {
        struct VsOutput
        {
            [SvPosition]
            public Vector4 Position;
            public float Color;
        }
        static VsOutput VertexShader(Vector3 vertex) => new VsOutput { Position = new Vector4(vertex.X, vertex.Y, 0, 1), Color = vertex.Z };
        static Vector4 PixelShader(VsOutput input)
            => new Vector4(input.Position.X / 1600, input.Position.Y / 900, input.Color, 10);

        static Vector4 PixelShader2(VsOutput input)
            => new Vector4(input.Color, input.Color, input.Color, input.Color);

        public static void Run()
        {
            var form = new SimpleRenderForm("ShaderTranslator demo");

            var simpleRenderer = new SimpleRenderer();
            simpleRenderer.Load<Vector3, VsOutput>(form.Device, VertexShader, PixelShader);

            var simpleRenderer2 = new SimpleRenderer();
            simpleRenderer2.Load<Vector3, VsOutput>(form.Device, VertexShader, PixelShader2);

            var vertexBuffer = Buffer.Create(form.Device, BindFlags.VertexBuffer, new[] {

                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),

                new Vector3(0, 1, 1),
                new Vector3(1, 0, 1),
                new Vector3(-1,0, 1)
            });

            var vertexBuffer2 = Buffer.Create(form.Device, BindFlags.VertexBuffer, new[] {

                new Vector3(-1, -1, 1),
                new Vector3(-1, 1, 0),
                new Vector3(1, -1, 0),
            });

            form.DoRenderLoop(context =>
            {
                using (var renderContext = simpleRenderer.Begin(context))
                {
                    renderContext.Draw(vertexBuffer);
                }
                using (var renderContext = simpleRenderer2.Begin(context))
                {
                    renderContext.Draw(vertexBuffer2);
                }
            });
        }
    }
}
