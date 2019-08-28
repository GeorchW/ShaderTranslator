using NUnit.Framework;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Test
{
    class Textures : Base
    {
        Vector4 Shader(Vector2 texCoord, [ShaderResource(0, 0)] Texture2D texture) => texture.Sample(texCoord);
        [Test, Ignore("Not implemented yet, not sure whether it will ever be")]
        public void UsingParameters() => TestPixelShader<Vector2, Texture2D, Vector4>(Shader);

        [ShaderResource(0, 0)]
        Texture2D texture;
        [Test]
        public void UsingFields() => TestPixelShader((Vector2 texCoord) => texture.Sample(texCoord));
    }
}
