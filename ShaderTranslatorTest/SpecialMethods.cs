using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Test
{
    class SpecialMethods : Base
    {
        [Test]
        public void Discard() => TestPixelShader((float x) =>
             {
                 ShaderMethods.Discard();
                 return 0;
             });

        [Test]
        public void Unroll() => TestPixelShader((float x) =>
            {
                ShaderMethods.Unroll();
                for (int i = 0; i < 10; i++)
                {
                    x += i;
                }
                return x;
            });
    }
}
