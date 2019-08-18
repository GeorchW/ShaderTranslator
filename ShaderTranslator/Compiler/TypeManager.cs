using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Resolver;
using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;

namespace ShaderTranslator
{
    class TypeManager
    {
        Dictionary<IType, string> KnownTypes = new Dictionary<IType, string>();
        IDecompilerTypeSystem TypeSystem;
        public TypeManager(IDecompilerTypeSystem typeSystem)
        {
            TypeSystem = typeSystem;

            AddKnownType(typeof(int), "int");
            AddKnownType(typeof(float), "float");
        }
        public void AddKnownType(Type type, string name) => KnownTypes.Add(TypeSystem.FindType(type), name);
        public string GetTypeString(AstNode node)
        {
            var type = node.Annotation<ICSharpCode.Decompiler.Semantics.TypeResolveResult>().Type;
            return GetTypeString(type);
        }
        public string GetTypeString(IType type) => KnownTypes[type];
    }
}
