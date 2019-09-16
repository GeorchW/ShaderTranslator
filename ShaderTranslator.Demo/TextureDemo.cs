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
    class TextureDemo
    {
        class MyShader
        {
            public struct VsOutput
            {
                public Vector2 TexCoord;
                [SvPosition]
                public Vector4 Position;
            }
            public struct CBuffer
            {
                public Vector2 WindowSize;
                public Vector2 Position;
                public Vector2 Size;
            }
            [ShaderResource(0, 0)]
            ShaderTexture2D shaderTexture = null!;
            [ShaderResource(0, 1)]
            CBuffer cBuffer = default;
            public VsOutput VertexShader(Vector2 input)
            {
                Vector2 screenPosition = (cBuffer.Position + input * cBuffer.Size) / cBuffer.WindowSize;
                screenPosition *= 2;
                screenPosition -= new Vector2(1, 1);
                screenPosition.Y *= -1;
                return new VsOutput { Position = new Vector4(screenPosition, 0, 1), TexCoord = input };
            }
            public Vector4 PixelShader(VsOutput input)
                => shaderTexture.Sample(input.TexCoord);
        }
        public static void Run()
        {
            var form = new SimpleRenderForm("ShaderTranslator demo");

            var (texture, view) = TextureHelper.CreateTextureWithMipmaps(form.Device.ImmediateContext, "./image.jpg");

            var simpleRenderer = new SimpleRenderer();
            var myShader = new MyShader();
            simpleRenderer.Load<Vector2, MyShader.VsOutput>(form.Device, myShader.VertexShader, myShader.PixelShader);
            simpleRenderer.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            var vertexBuffer = Buffer.Create(form.Device, BindFlags.VertexBuffer, new Vector2[] {
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
            });

            var constantBuffer = BufferHelper.CreateConstantBuffer<MyShader.CBuffer>(form.Device);

            form.DoRenderLoop(context =>
            {
                using (var renderContext = simpleRenderer.Begin(context))
                {
                    long ticks = DateTime.Now.Ticks % ((long)1e7);
                    float now = ticks / 1e7f * MathF.PI * 2;
                    Vector2 offset = new Vector2(MathF.Sin(now), MathF.Cos(now)) * 0.1f;
                    var data = new MyShader.CBuffer
                    {
                        Position = new Vector2(300, 300) + offset * 100,
                        Size = new Vector2(200, 300),
                        WindowSize = new Vector2(1600, 900),
                    };
                    BufferHelper.Write(context, constantBuffer, data);

                    context.PixelShader.SetShaderResource(0, view);
                    context.VertexShader.SetConstantBuffer(1, constantBuffer);
                    renderContext.Draw(vertexBuffer);
                }
            });
        }
    }
}
