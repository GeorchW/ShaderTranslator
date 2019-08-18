using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class MethodReferences : Base
    {
        static float Increment(float x) => x + 1;
        [Test]
        public void StaticMethodReference() => TestPixelShader((float f) => Increment(f));
        [Test]
        public void MultiStaticMethodReference() => TestPixelShader((float f) => Increment(f) + Increment(f));
    }
}
