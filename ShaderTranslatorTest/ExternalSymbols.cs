using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class ExternalSymbols : Base
    {
        [Test]
        public void ExternalMethod() => TestPixelShader((float f) => MathF.Sin(f));
        [Test]
        public void ExternalStruct() => TestPixelShader((Vector2 v) => v.X);
    }
}
