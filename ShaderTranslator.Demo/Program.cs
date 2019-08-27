using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Numerics;
using System.Windows.Forms;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderTranslator.Syntax;

namespace ShaderTranslator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var form = new Form();
            form.ClientSize = new System.Drawing.Size(1600, 900);
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.Text = "ShaderTranslator demo";

            SwapChainDescription swapChainDescription;
            swapChainDescription.BufferCount = 1;
            swapChainDescription.Flags = SwapChainFlags.None;
            swapChainDescription.IsWindowed = true;
            swapChainDescription.ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
            swapChainDescription.OutputHandle = form.Handle;
            swapChainDescription.SampleDescription = new SampleDescription(1, 0);
            swapChainDescription.SwapEffect = SwapEffect.Discard;
            swapChainDescription.Usage = Usage.RenderTargetOutput;

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, swapChainDescription, out var device, out var swapChain);

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);


            Func<Vector2, Vector4> vsMethod = (Vector2 input) => new Vector4(input, 0, 1);
            var vsBytecode = Compile(vsMethod, new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.VertexShader), "vs_5_0");

            var inputLayout = new InputLayout(device,
                vsBytecode.Bytecode.Data,
                new InputElement[] {
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0)
            });

            var vs = new VertexShader(device, vsBytecode.Bytecode.Data);

            Func<Vector4> psMethod = () => new Vector4(1, 0, 0, 1);
            var psBytecode = Compile(psMethod, new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.PixelShader), "ps_5_0");
            var ps = new PixelShader(device, psBytecode.Bytecode.Data);

            var vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new Vector2[] {
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 1),
            });

            bool exit = false;
            form.FormClosed += (sender, e) => exit = true;
            form.Show();
            while (!exit)
            {
                device.ImmediateContext.ClearRenderTargetView(renderView, new SharpDX.Mathematics.Interop.RawColor4(0.1f, 0.8f, 0.2f, 1));

                var context = device.ImmediateContext;

                context.InputAssembler.InputLayout = inputLayout;
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 8, 0));
                context.VertexShader.Set(vs);
                context.PixelShader.Set(ps);
                context.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height);
                context.OutputMerger.SetTargets(renderView);

                context.Draw(3, 0);

                swapChain.Present(1, 0);
                Application.DoEvents();
            }

            Console.WriteLine("Hello World!");
        }

        static CompileEngine compileEngine = new CompileEngine(SymbolResolver.Default);
        private static SharpDX.D3DCompiler.CompilationResult Compile(Delegate shader, SemanticsGenerator semanticsGenerator, string model)
        {
            string shaderCode = compileEngine.Compile(shader.Target, shader.Method, semanticsGenerator, out string entryPoint);
            return SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderCode, entryPoint, model);
        }
    }
}
