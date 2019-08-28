using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Device = SharpDX.Direct3D11.Device;

namespace ShaderTranslator.Demo
{
    class SimpleRenderForm
    {
        Form form;
        SwapChain swapChain;
        RenderTargetView backBufferView;

        public Device Device { get; }
        public RawColor4 ClearColor { get; } = new RawColor4(0.1f, 0.8f, 0.2f, 1);

        public SimpleRenderForm()
        {
            form = new Form();
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

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, swapChainDescription, out var device, out swapChain);
            Device = device;

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            backBufferView = new RenderTargetView(Device, backBuffer);
        }

        public void DoRenderLoop(Action<DeviceContext> loopBody)
        {
            bool exit = false;
            form.FormClosed += (sender, e) => exit = true;
            form.Show();
            while (!exit)
            {
                var context = Device.ImmediateContext;

                context.ClearRenderTargetView(backBufferView, ClearColor);
                context.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height);
                context.OutputMerger.SetTargets(backBufferView);

                loopBody(context);

                swapChain.Present(1, 0);
                Application.DoEvents();
            }
        }
    }
}
