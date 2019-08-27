using System;

namespace ShaderTranslator.Syntax
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
    public abstract class SemanticsAttribute : Attribute
    {
        internal string Semantics { get; }
        public SemanticsAttribute(string semantics)
        {
            Semantics = semantics;
        }
    }
    public class SvTargetAttribute : SemanticsAttribute
    {
        public int TargetIndex { get; }
        public SvTargetAttribute(int targetIndex) : base("SV_Target" + targetIndex) => TargetIndex = targetIndex;
    }
    public class SvPositionAttribute : SemanticsAttribute
    {
        public SvPositionAttribute() : base("SV_Position") { }
    }
}