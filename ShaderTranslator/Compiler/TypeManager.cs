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
            type.GatherFields(this);
            return true;
        }

        public void Print(IndentedStringBuilder result)
        {
            foreach (var type in translatedTypes.Reverse<StructTargetType>())
            {
                result.WriteLine(type.GetCode());
            }
        }
    }
}
