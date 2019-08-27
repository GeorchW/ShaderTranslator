using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class TypeReferences : Base
    {
        struct SomeStruct
        {
            public float A;
            public int B;
        }
        [Test]
        public void InputStruct() => TestPixelShader((SomeStruct input) => input.A * input.B);
        [Test]
        public void OutputStruct() => TestPixelShader((Vector2 input) => new SomeStruct { A = input.X, B = (int)input.Y });
    }
}
