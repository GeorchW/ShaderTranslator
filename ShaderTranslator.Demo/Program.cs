using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Numerics;
using System.Windows.Forms;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderTranslator.Syntax;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ShaderTranslator.Demo
{
    class Program
    {
        struct VsOutput
        {
            public Vector2 TexCoord;
            [SvPosition]
            public Vector4 Position;
        }
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


            Func<Vector2, VsOutput> vsMethod = (Vector2 input) => new VsOutput { Position = new Vector4(input, 0, 1), TexCoord = input };
            var (vs, inputLayout) = CompileVertexShader(vsMethod, device);

            Func<Vector2, Vector4> psMethod = input => new Vector4(input * 2, 0, 1);
            var ps = CompilePixelShader(psMethod, device);

            var vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new Vector2[] {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0),
                new Vector2(0, 0.5f),
                new Vector2(0, 0),
            });

            bool exit = false;
            form.FormClosed += (sender, e) => exit = true;
            form.Show();
            while (!exit)
            {
                device.ImmediateContext.ClearRenderTargetView(renderView, new SharpDX.Mathematics.Interop.RawColor4(0.1f, 0.8f, 0.2f, 1));

                var context = device.ImmediateContext;

                context.InputAssembler.InputLayout = inputLayout;
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 8, 0));
                context.VertexShader.Set(vs);
                context.PixelShader.Set(ps);
                context.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height);
                context.OutputMerger.SetTargets(renderView);

                context.Draw(4, 0);

                swapChain.Present(1, 0);
                Application.DoEvents();
            }

            Console.WriteLine("Hello World!");
        }

        static CompileEngine compileEngine = new CompileEngine(SymbolResolver.Default);
        private static (VertexShader, InputLayout) CompileVertexShader(Delegate shaderMethod, Device device)
        {
            var semanticsGenerator = new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.VertexShader);
            var result = compileEngine.Compile(shaderMethod.Target, shaderMethod.Method, semanticsGenerator);
            List<InputElement> inputElements = new List<InputElement>();
            foreach (var parameter in result.EntryPoint.Parameters)
            {
                if (parameter.Semantics != null)
                {
                    var match = Regex.Match(parameter.Semantics, "([a-zA-Z]*)(\\d?)");
                    if (match.Success)
                    {
                        string name = match.Groups[1].Value;
                        if (!int.TryParse(match.Groups[2].Value, out int index))
                            index = 0;
                        if (!(parameter.Type is PrimitiveTargetType primitive))
                            throw new Exception();
                        PrimitiveType primitiveType = primitive.PrimitiveType;
                        inputElements.Add(new InputElement(name, index, ToDxgiFormat(primitiveType), 0));
                        continue;
                    }
                }
                throw new Exception();
            }
            var compilation = SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "vs_5_0");
            var shader = new VertexShader(device, compilation.Bytecode.Data);
            var inputLayout = new InputLayout(device, compilation.Bytecode.Data, inputElements.ToArray());
            return (shader, inputLayout);
        }

        private static PixelShader CompilePixelShader(Delegate shaderMethod, Device device)
        {
            var semanticsGenerator = new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.PixelShader);
            var result = compileEngine.Compile(shaderMethod.Target, shaderMethod.Method, semanticsGenerator);
            var compilation = SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "ps_5_0");
            var shader = new PixelShader(device, compilation.Bytecode.Data);
            return shader;
        }

        private static Format ToDxgiFormat(PrimitiveType primitiveType)
            => primitiveType.ComponentType switch
            {
                ComponentType.Float => primitiveType.Length switch
                {
                    VectorLength.Scalar => Format.R32_Float,
                    VectorLength.Vector2 => Format.R32G32_Float,
                    VectorLength.Vector3 => Format.R32G32B32_Float,
                    VectorLength.Vector4 => Format.R32G32B32A32_Float,
                    _ => throw new NotImplementedException()
                },
                ComponentType.Int => primitiveType.Length switch
                {
                    VectorLength.Scalar => Format.R32_SInt,
                    VectorLength.Vector2 => Format.R32G32_SInt,
                    VectorLength.Vector3 => Format.R32G32B32_SInt,
                    VectorLength.Vector4 => Format.R32G32B32A32_SInt,
                    _ => throw new NotImplementedException()
                },
                _ => throw new NotImplementedException()
            };
    }
}
