using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ShaderTranslator.Demo
{
    class SimpleRenderer
    {
        InputLayout inputLayout;
        VertexShader vs;
        PixelShader ps;
        int stride;

        public PrimitiveTopology PrimitiveTopology { get; set; } = PrimitiveTopology.TriangleList;

        public void Load(Device device, Delegate vertexShaderMethod, Delegate pixelShaderMethod)
        {
            (vs, inputLayout, stride) = ShaderCompiler.CompileVertexShader(vertexShaderMethod, device);
            ps = ShaderCompiler.CompilePixelShader(pixelShaderMethod, device);
        }

        public struct RenderContext : IDisposable
        {
            DeviceContext context;
            SimpleRenderer parent;
            internal RenderContext(DeviceContext context, SimpleRenderer parent)
            {
                this.context = context;
                this.parent = parent;
            }
            public void Draw(Buffer vertexBuffer)
            {
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, parent.stride, 0));
                context.Draw(vertexBuffer.Description.SizeInBytes / parent.stride, 0);
            }
            void IDisposable.Dispose()
            { }
        }

        public RenderContext Begin(DeviceContext context)
        {
            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology;
            context.VertexShader.Set(vs);
            context.PixelShader.Set(ps);
            return new RenderContext(context, this);
        }
    }
}
