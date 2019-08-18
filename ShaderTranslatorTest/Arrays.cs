using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class Arrays : Base
    {
        struct StructWithArray
        {
            [ArrayLength(5)]
            public float[] Floats;
        }
        [Test]
        public void FloatArrayField() => TestPixelShader((StructWithArray input) => input.Floats[3]);

        struct StructWithArrayOfStruct
        {
            [ArrayLength(3)]
            public StructWithArray[] Structs;
        }
        [Test]
        public void StructArrayField() => TestPixelShader((StructWithArrayOfStruct input) => input.Structs[1].Floats[3]);
    }
}
