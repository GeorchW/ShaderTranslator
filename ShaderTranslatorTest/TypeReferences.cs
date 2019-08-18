using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class TypeReferences:Base
    {
        struct SomeStruct
        {
            public float A;
            public int B;
        }
        [Test]
        public void Stuff() => TestPixelShader((SomeStruct input) => input.A * input.B);
    }
}
