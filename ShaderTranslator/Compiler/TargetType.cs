namespace ShaderTranslator
{
    public abstract class TargetType
    {
        public string Name { get; }
        public bool IsPrimitive { get; }

        protected TargetType(string name, bool isPrimitive)
        {
            Name = name;
            IsPrimitive = isPrimitive;
        }
    }
}
