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
        Dictionary<IType, string> KnownTypes = new Dictionary<IType, string>();
        IDecompilerTypeSystem TypeSystem;
        SymbolResolver symbolResolver;

        List<TypeCompilation> translatedTypes = new List<TypeCompilation>();
        Queue<TypeCompilation> toBeVisited = new Queue<TypeCompilation>();

        NamingScope globalScope;

        public TypeManager(IDecompilerTypeSystem typeSystem, SymbolResolver symbolResolver, NamingScope globalScope)
        {
            TypeSystem = typeSystem;
            this.symbolResolver = symbolResolver;
            this.globalScope = globalScope;

            AddKnownType(typeof(int), "int");
            AddKnownType(typeof(float), "float");
        }
        public void AddKnownType(Type type, string name) => KnownTypes.Add(TypeSystem.FindType(type), name);
        public string GetTypeString(AstNode node)
        {
            var type = node.Annotation<ICSharpCode.Decompiler.Semantics.TypeResolveResult>().Type;
            return GetTypeString(type);
        }

        public string GetTypeString(IType type)
        {
            if (KnownTypes.TryGetValue(type, out var result))
                return result;
            else if (symbolResolver.TryResolve(type) is string knownSymbol)
                return knownSymbol;
            else if (type.Kind == TypeKind.Struct)
            {
                string name = globalScope.GetFreeName(type.Name);
                var compilation = new TypeCompilation(type, name);
                KnownTypes.Add(type, name);
                toBeVisited.Enqueue(compilation);
                translatedTypes.Add(compilation);
                return name;
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

        internal void Print(IndentedStringBuilder result)
        {
            foreach (var type in translatedTypes.Reverse<TypeCompilation>())
            {
                result.WriteLine(type.Code);
            }
        }
    }
    class TypeCompilation
    {
        public IType Type { get; }
        public string Name { get; }
        string? code;
        public string Code => code ?? throw new Exception("Call Compile() first!");

        public TypeCompilation(IType type, string name)
        {
            Type = type;
            Name = name;
        }

        public void Compile(TypeManager typeManager)
        {
            var codeBuilder = new IndentedStringBuilder();
            codeBuilder.Write("struct ");
            codeBuilder.WriteLine(Name);
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();
            foreach (var field in Type.GetFields())
            {
                if (field.Type is ArrayType arrayType)
                {
                    var inner = arrayType.ElementType;
                    var length = field.GetAttributes()
                        .Where(attr => attr.AttributeType.FullName == typeof(Syntax.ArrayLengthAttribute).FullName)
                        .Single()
                        .FixedArguments[0]
                        .Value;
                    codeBuilder.Write(typeManager.GetTypeString(inner));
                    codeBuilder.Write(" ");
                    codeBuilder.Write(field.Name);
                    codeBuilder.Write("[");
                    codeBuilder.Write((int)length);
                    codeBuilder.Write("]");
                }
                else
                {
                    codeBuilder.Write(typeManager.GetTypeString(field.Type));
                    codeBuilder.Write(" ");
                    codeBuilder.Write(field.Name);
                }
                codeBuilder.WriteLine(";");
            }
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("};");
            code = codeBuilder.ToString();
        }
    }
}
