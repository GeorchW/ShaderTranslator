using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using ShaderTranslator.Syntax;

namespace ShaderTranslator.Test
{
    class Verbatim : Base
    {
        [VerbatimHeader("// This is a useless piece of header.")]
        Vector4 MyShader(Vector2 input) => new Vector4(1, 0, 0, 1);
        [Test]
        public void VerbatimHeader() => TestPixelShader<Vector2, Vector4>(MyShader);
    }
}
