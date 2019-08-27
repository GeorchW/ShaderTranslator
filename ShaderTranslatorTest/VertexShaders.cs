using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class VertexShaders : Base
    {
        [Test]
        public void SimpleVertexShader() => TestVertexShader(new Func<Vector2, Vector4>((Vector2 input) => new Vector4(input, 0, 1)));

        struct OutputStruct
        {
            public Vector2 TexCoord;
            [SvPosition]
            public Vector4 Position;
        }
        [Test]
        public void CustomOutputSemantics() => TestVertexShader(new Func<Vector2, OutputStruct>(
            (Vector2 input) => new OutputStruct {
                Position = new Vector4(input, 0, 1),
                TexCoord = input
            }));
    }
}
