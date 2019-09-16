using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ShaderTranslator.Syntax
{
    public class Texture2D
    {
        public Vector4 Sample(Vector2 position) => throw new NotImplementedException();
        public Vector4 SampleLevel(Vector2 position, float level) => throw new NotImplementedException();
    }
    public class TextureCube
    {
        public Vector4 Sample(Vector3 direction) => throw new NotImplementedException();
    }
}
