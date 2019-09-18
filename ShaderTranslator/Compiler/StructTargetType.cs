﻿using System;
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
            public string? Semantics { get; set; }

            public Field(TargetType type, string name, int? arrayLength, string? semantics)
            {
                Type = type;
                Name = name;
                ArrayLength = arrayLength;
                Semantics = semantics;
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
                arrayLength = (int)field.GetAttributes()
                    .Where(attr => attr.AttributeType.FullName == typeof(Syntax.ArrayLengthAttribute).FullName)
                    .Single()
                    .FixedArguments[0]
                    .Value;
                type = arrayType.ElementType;
            }
            string? semantics = null;
            var semanticsAttribute = field.GetAttributes()
                .Where(attr =>
                    attr.AttributeType.GetAllBaseTypes()
                    .Any(type => type.FullName == typeof(GlslAttribute).FullName))
                .SingleOrDefault();
            if(semanticsAttribute != null)
            {
                //TODO: It would be easier to have the actual object here.
                semantics = (string)semanticsAttribute.FixedArguments[0].Value;
            }
            return new Field(typeManager.GetTargetType(type), field.Name, arrayLength, semantics);
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

        internal void SetSemantics(string semanticsBase, ref int index)
        {
            foreach (var field in Fields)
            {
                if (field.Semantics != null)
                    continue;
                if (field.Type.IsPrimitive)
                {
                    field.Semantics = semanticsBase + index;
                    index++;
                }
                if (field.Type is StructTargetType str)
                {
                    str.SetSemantics(semanticsBase, ref index);
                }
            }
        }
    }
}
