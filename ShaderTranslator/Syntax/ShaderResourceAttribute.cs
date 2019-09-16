using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Syntax
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, AllowMultiple = false)]
    public class ShaderResourceAttribute : Attribute
    {
        public int Set { get; }
        public int Slot { get; }
        public ShaderResourceAttribute(int set, int slot)
        {
            Set = set;
            Slot = slot;
        }
    }
}
