using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Syntax
{
    public class Texture2D
    {
        [Glsl("texture")]
        public Vector4 Sample(Vector2 position) => throw new NotImplementedException();
        [Glsl("textureLod")]
        public Vector4 SampleLevel(Vector2 position, float level) => throw new NotImplementedException();
    }
    public class TextureCube
    {
        [Glsl("texture")]
        public Vector4 Sample(Vector3 direction) => throw new NotImplementedException();
    }
}
