namespace ShaderTranslator
{
    public class PrimitiveTargetType : TargetType
    {
        public PrimitiveType PrimitiveType { get; }
        public PrimitiveTargetType(PrimitiveType targetType)
            : base(targetType.ToString(), true)
        {
            PrimitiveType = targetType;
        }
    }
}
