using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShaderTranslator
{
    class NameManager
    {
        HashSet<string> keywords = new HashSet<string>
        {
            //Source: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-appendix-keywords
            "AppendStructuredBuffer", "asm", "asm_fragment",
            "BlendState", "bool", "break", "Buffer", "ByteAddressBuffer",
            "case", "cbuffer", "centroid", "class", "column_major", "compile", "compile_fragment", "CompileShader", "const", "continue", "ComputeShader", "ConsumeStructuredBuffer",
            "default", "DepthStencilState", "DepthStencilView", "discard", "do", "double", "DomainShader", "dword",
            "else", "export", "extern",
            "false", "float", "for", "fxgroup",
            "GeometryShader", "groupshared",
            "half", "Hullshader",
            "if", "in", "inline", "inout", "InputPatch", "int", "interface",
            "line", "lineadj", "linear", "LineStream",
            "matrix", "min16float", "min10float", "min16int", "min12int", "min16uint",
            "namespace", "nointerpolation", "noperspective", "NULL",
            "out", "OutputPatch",
            "packoffset", "pass", "pixelfragment", "PixelShader", "point", "PointStream", "precise",
            "RasterizerState", "RenderTargetView", "return", "register", "row_major", "RWBuffer", "RWByteAddressBuffer", "RWStructuredBuffer", "RWTexture1D", "RWTexture1DArray", "RWTexture2D", "RWTexture2DArray", "RWTexture3D",
            "sample", "sampler", "SamplerState", "SamplerComparisonState", "shared", "snorm", "stateblock", "stateblock_state", "static", "string", "struct", "switch", "StructuredBuffer",
            "tbuffer", "technique", "technique10", "technique11", "texture", "Texture1D", "Texture1DArray", "Texture2D", "Texture2DArray", "Texture2DMS", "Texture2DMSArray", "Texture3D", "TextureCube", "TextureCubeArray", "true", "typedef", "triangle", "triangleadj", "TriangleStream",
            "uint", "uniform", "unorm", "unsigned",
            "vector", "vertexfragment", "VertexShader", "void", "volatile",
            "while"
        };

        public bool IsKeyword(string name) => keywords.Contains(name);

        Regex[] toBeRemoved = new Regex[] {
            new Regex("^_+"), // leading underscored
            new Regex("__+"), // duplicate underscores
            new Regex("[^a-zA-Z0-9]+"), // non-alphanumeric characters
            new Regex("^\\d"), // leading digits
        };
        public string Legalize(string name)
        {
            foreach (var regex in toBeRemoved)
            {
                name = regex.Replace(name, "");
            }
            return name;
        }
    }
}
