using ICSharpCode.Decompiler.Semantics;
using System;
using System.Reflection;
using ICSharpCode.Decompiler.CSharp;

namespace ShaderTranslator
{
    public class CompileEngine
    {
        ILSpyManager ilSpyManager = new ILSpyManager();
        public SymbolResolver SymbolResolver { get; }

        public CompileEngine(SymbolResolver symbolResolver) => SymbolResolver = symbolResolver;

        public string Compile(object? obj, MethodInfo method, SemanticsGenerator semanticsGenerator, out string entryPoint)
        {
            var compilation = new ShaderCompilation(ilSpyManager, SymbolResolver, method, semanticsGenerator);
            var result = compilation.Compile();
            entryPoint = compilation.EntryPoint.Name;
            return result;
        }
    }
}
