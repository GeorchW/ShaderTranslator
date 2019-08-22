using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Resolver;
using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace ShaderTranslator
{
    class TypeManager
    {
        Dictionary<IType, TargetType> KnownTypes = new Dictionary<IType, TargetType>();
        IDecompilerTypeSystem TypeSystem;
        MathApi mathApi;

        List<StructTargetType> translatedTypes = new List<StructTargetType>();
        Queue<StructTargetType> toBeVisited = new Queue<StructTargetType>();

        NamingScope globalScope;

        public TypeManager(IDecompilerTypeSystem typeSystem, MathApi mathApi, NamingScope globalScope)
        {
            TypeSystem = typeSystem;
            this.mathApi = mathApi;
            this.globalScope = globalScope;
        }
        public string GetTypeString(AstNode node)
        {
            var type = node.Annotation<ICSharpCode.Decompiler.Semantics.TypeResolveResult>().Type;
            return GetTypeString(type);
        }

        public string GetTypeString(IType type) => GetTargetType(type).Name;

        public TargetType GetTargetType(IType type)
        {
            if (KnownTypes.TryGetValue(type, out var knownType))
                return knownType;
            else if (mathApi.TryResolve(type, out var mathType))
                return mathType;
            else if (type.Kind == TypeKind.Struct)
            {
                if (type.ToPrimitiveType() != ICSharpCode.Decompiler.IL.PrimitiveType.None)
                    throw new Exception("Primitive type was not translated.");
                string name = globalScope.GetFreeName(type.Name);
                var compilation = new StructTargetType(type, this, name);
                KnownTypes.Add(type, compilation);
                toBeVisited.Enqueue(compilation);
                translatedTypes.Add(compilation);
                return compilation;
            }
            else throw new Exception($"Type {type.Name} can't be translated.");
        }

        public bool CompileNextType()
        {
            if (!toBeVisited.TryDequeue(out var type))
                return false;
            type.Compile(this);
            return true;
        }

        public void Print(IndentedStringBuilder result)
        {
            foreach (var type in translatedTypes.Reverse<StructTargetType>())
            {
                result.WriteLine(type.Code);
            }
        }
    }
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
    public class PrimitiveTargetType : TargetType
    {
        public PrimitiveType PrimitiveType { get; }
        public PrimitiveTargetType(PrimitiveType targetType)
            : base(targetType.ToString(), true)
        {
            PrimitiveType = targetType;
        }
    }

    public class StructTargetType : TargetType
    {
        public class Field
        {
            public TargetType Type { get; }
            public string Name { get; }
            public int? ArrayLength { get; }
            public bool IsArray => ArrayLength != null;
            public string? Semantics { get; }

            public Field(TargetType type, string name, int? arrayLength, string? semantics)
            {
                Type = type;
                Name = name;
                ArrayLength = arrayLength;
                Semantics = semantics;
            }
        }
        string? code;
        internal string Code => code ?? throw new Exception("Call Compile() first!");
        IReadOnlyList<Field>? fields;
        public IReadOnlyList<Field> Fields => fields ?? throw new Exception("Call Compile() first!");
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
            return new Field(typeManager.GetTargetType(type), field.Name, arrayLength, null);
        }

        internal void Compile(TypeManager typeManager)
        {
            fields = SourceType.GetFields().Select(field => Convert(typeManager, field)).ToArray();

            var codeBuilder = new IndentedStringBuilder();
            codeBuilder.Write("struct ");
            codeBuilder.WriteLine(Name);
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();
            foreach (var field in Fields)
            {
                codeBuilder.Write(field.Type.Name);
                codeBuilder.Write(" ");
                codeBuilder.Write(field.Name);
                if (field.ArrayLength is int length)
                {
                    codeBuilder.Write("[");
                    codeBuilder.Write(length);
                    codeBuilder.Write("]");
                }
                codeBuilder.WriteLine(";");
            }
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("};");
            code = codeBuilder.ToString();
        }
    }
}
