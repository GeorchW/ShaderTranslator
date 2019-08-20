using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class ConstantBuffers : Base
    {
        [ShaderResource(0, 0)]
        float offset;

        [Test]
        public void SimpleConstantBuffer() => TestPixelShader((float input) => input + offset);
    }
}
