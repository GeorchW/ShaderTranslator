using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator.Syntax
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    class ArrayLengthAttribute : Attribute
    {
        public int Length { get; }
        public ArrayLengthAttribute(int length)
        {
            Length = length;
        }
    }
}
