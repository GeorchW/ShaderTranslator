using System;

namespace ShaderTranslator.Syntax
{
    class GlslAttribute : Attribute
    {
        public string Symbol { get; }
        public GlslAttribute(string symbol)
        {
            Symbol = symbol;
        }
    }
}
