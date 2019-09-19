using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Syntax
{
    public class VerbatimHeaderAttribute : Attribute
    {
        public string Header { get; }
        public VerbatimHeaderAttribute(string header)
        {
            Header = header;
        }
    }
}
