using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;
using ShaderTranslator.Syntax;

namespace ShaderTranslator
{
    public class StructTargetType : TargetType
    {
        public class Field
        {
            public TargetType Type { get; }
            public string Name { get; }
            public int? ArrayLength { get; }
            public bool IsArray => ArrayLength != null;
            public IField SourceField { get; set; }
            public string? SemanticName { get; }

            public Field(TargetType type, string name, int? arrayLength, IField sourceField, string? semanticName)
            {
                Type = type;
                Name = name;
                ArrayLength = arrayLength;
                SourceField = sourceField;
                SemanticName = semanticName;
            }
        }
        Field[]? fields;
        public IReadOnlyList<Field> Fields => fields ?? throw new Exception($"Call {nameof(GatherFields)}() first!");
        public IType SourceType { get; }

        internal StructTargetType(IType sourceType, TypeManager typeManager, string name)
            : base(name, false)
        {
            SourceType = sourceType;
        }

        Field Convert(TypeManager typeManager, IField field)
        {
            var type = field.Type;
            int? arrayLength = null;
            if (type is ArrayType arrayType)
            {
                if (!field.GetAttributes().TryGetAttribute(typeof(ArrayLengthAttribute), out var arrayLengthAttribute))
                    throw new Exception($"Arrays must be decorated with {nameof(ArrayLengthAttribute)}!");
                arrayLength = (int)arrayLengthAttribute.FixedArguments[0].Value!;
                type = arrayType.ElementType;
            }
            return new Field(typeManager.GetTargetType(type), field.Name, arrayLength, field, field.GetAttributes().GetName(null));
        }
        internal void GatherFields(TypeManager typeManager) => fields = SourceType.GetFields().Select(field => Convert(typeManager, field)).ToArray();

        internal string GetCode()
        {
            var codeBuilder = new IndentedStringBuilder();
            codeBuilder.Write("struct ");
            codeBuilder.WriteLine(Name);
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();
            foreach (var field in Fields)
            {
                codeBuilder.Write(field.Type.Name);
                if (field.ArrayLength is int length)
                {
                    codeBuilder.Write("[");
                    codeBuilder.Write(length);
                    codeBuilder.Write("]");
                }
                codeBuilder.Write(" ");
                codeBuilder.Write(field.Name);
                codeBuilder.WriteLine(";");
            }
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("};");
            return codeBuilder.ToString();
        }
    }
}
