using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class Operators : Base
    {
        [Test]
        public void VectorAddition() => TestPixelShader((Vector2 a) => a + a);
        [Test]
        public void NewOperator() => TestPixelShader((float x, float y) => new Vector2(x, y));
    }
}
