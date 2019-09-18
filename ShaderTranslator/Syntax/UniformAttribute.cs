using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Syntax
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, AllowMultiple = false)]
    class UniformAttribute : Attribute
    {
        public int Slot { get; }
        public UniformAttribute(int slot)
        {
            Slot = slot;
        }
    }
}
