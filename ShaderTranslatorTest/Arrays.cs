using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class Arrays : Base
    {
        [Uniform(0)]
        StructWithArray structWithArray;
        struct StructWithArray
        {
            [ArrayLength(5)]
            public float[] Floats;
        }
        [Test]
        public void FloatArrayField() => TestPixelShader((float input) => structWithArray.Floats[3] + input);


        [Uniform(0)]
        StructWithArrayOfStruct structWithArrayOfStruct;
        struct StructWithArrayOfStruct
        {
            [ArrayLength(3)]
            public StructWithArray[] Structs;
        }
        [Test]
        public void StructArrayField() => TestPixelShader((float input) => structWithArrayOfStruct.Structs[1].Floats[3] + input);
    }
}
