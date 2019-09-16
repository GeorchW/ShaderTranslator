using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Device = SharpDX.Direct3D11.Device;

namespace ShaderTranslator.Demo
{
    class TextureHelper
    {
        public static Texture2D CreateTexture(Device device, string path)
        {
            var image = LoadImage(path);
            var data = MemoryMarshal.AsBytes(image.GetPixelSpan());
            return CreateTexture(device, image.Width, image.Height, data);
        }
        public static (Texture2D, ShaderResourceView) CreateTextureWithMipmaps(DeviceContext deviceContext, string path)
        {
            var image = LoadImage(path);
            var data = MemoryMarshal.AsBytes(image.GetPixelSpan());
            return CreateTextureWithMipmaps(deviceContext, image.Width, image.Height, data);
        }

        private static Image<Rgba32> LoadImage(string path)
        {
            using var file = File.OpenRead(path);
            return Image.Load(file);
        }

        public static unsafe Texture2D CreateTexture(Device device, int width, int height, ReadOnlySpan<byte> data)
        {
            Texture2DDescription desc;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = Format.R8G8B8A8_UNorm;
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription = new SampleDescription(1, 0);
            desc.Usage = ResourceUsage.Immutable;
            fixed (void* ptr = data)
            {
                return new Texture2D(device, desc, new SharpDX.DataRectangle(new IntPtr(ptr), width * 4));
            }
        }
        public static unsafe (Texture2D, ShaderResourceView) CreateTextureWithMipmaps(DeviceContext context, int width, int height, ReadOnlySpan<byte> data)
        {
            Texture2DDescription desc;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = Format.R8G8B8A8_UNorm;
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 0;
            desc.OptionFlags = ResourceOptionFlags.GenerateMipMaps;
            desc.SampleDescription = new SampleDescription(1, 0);
            desc.Usage = ResourceUsage.Default;
            Texture2D texture;
            fixed (void* ptr = data)
            {
                DataRectangle[] rectangles = new DataRectangle[20];
                for (int i = 0; i < rectangles.Length; i++)
                {
                    rectangles[i] = new DataRectangle(new IntPtr(ptr), width * 4);
                }
                texture = new Texture2D(context.Device, desc, rectangles);
            }
            var view = new ShaderResourceView(context.Device, texture);
            context.GenerateMips(view);
            return (texture, view);
        }
    }
}
