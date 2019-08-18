using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class ControlFlow : Base
    {
        [Test]
        public void If() => TestPixelShader((float f) =>
        {
            if (f > 1) return 1;
            else return 0;
        });
        [Test]
        public void While() => TestPixelShader((float f) =>
        {
            while (f > 5) f -= 2;
            return f;
        });
        [Test]
        public void For() => TestPixelShader((float f) =>
        {
            for (int i = 0; i < 10; i++)
                f += f * f;
            return f;
        });
    }
}
