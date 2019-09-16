using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ShaderTranslator.Demo
{
    static class BufferHelper
    {
        public static unsafe Buffer CreateConstantBuffer<T>(Device device)
            where T : unmanaged
        {
            BufferDescription bufferDescription;
            bufferDescription.BindFlags = BindFlags.ConstantBuffer;
            bufferDescription.CpuAccessFlags = CpuAccessFlags.Write;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.SizeInBytes = RoundTo16(sizeof(T));
            bufferDescription.Usage = ResourceUsage.Dynamic;
            bufferDescription.OptionFlags = ResourceOptionFlags.None;
            bufferDescription.StructureByteStride = 0;
            return new Buffer(device, bufferDescription);
        }

        public static unsafe void Write<T>(DeviceContext context, Buffer buffer, in T data)
            where T : unmanaged
        {
            var box = context.MapSubresource(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            unsafe
            {
                var span = new Span<byte>(box.DataPointer.ToPointer(), sizeof(T));
                var castedSpan = MemoryMarshal.Cast<byte, T>(span);
                castedSpan[0] = data;
            }
            context.UnmapSubresource(buffer, 0);
        }

        private static int RoundTo16(int size)
        {
            if (size % 16 != 0)
            {
                size /= 16;
                size++;
                size *= 16;
            }

            return size;
        }
    }
}
