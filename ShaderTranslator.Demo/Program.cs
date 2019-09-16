using System;
using System.Numerics;
using ShaderTranslator.Syntax;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using ShaderTexture2D = ShaderTranslator.Syntax.Texture2D;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;

namespace ShaderTranslator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleDemo.Run();
            //TextureDemo.Run();
        }
    }
}
