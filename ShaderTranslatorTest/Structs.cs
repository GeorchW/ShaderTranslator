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
            public float B;

            public SomeStruct(float a)
            {
                A = a;
                B = a + 1;
            }

            public float Sum() => A + B;
            public float SumWrapper() => Sum();
        }
        [Test]
        public void InputStruct() => TestPixelShader((SomeStruct input) => input.A * input.B);
        [Test]
        public void OutputStruct() => TestPixelShader((Vector2 input) => new SomeStruct { A = input.X, B = (int)input.Y });
        [Test]
        public void StructWithCtor() => TestPixelShader((Vector2 input) => new SomeStruct(input.X));
        [Test]
        public void StructMethod() => TestPixelShader((SomeStruct input) => input.Sum());
        [Test]
        public void WrappedStructMethod() => TestPixelShader((SomeStruct input) => input.SumWrapper());
    }
}
