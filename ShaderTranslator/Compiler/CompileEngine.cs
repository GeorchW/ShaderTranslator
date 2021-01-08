using ICSharpCode.Decompiler.Semantics;
using System;
using System.Reflection;
using ICSharpCode.Decompiler.CSharp;

namespace ShaderTranslator
{
    public class CompileEngine
    {
        ILSpyManager ilSpyManager;
        public SymbolResolver SymbolResolver { get; }

        public CompileEngine(SymbolResolver symbolResolver, PEFileResolver peFileResolver)
        {
            SymbolResolver = symbolResolver;
            ilSpyManager = new ILSpyManager(peFileResolver);
        }

        public ShaderCompilation Compile(object? obj, MethodInfo method, ShaderType shaderType)
        {
            var compilation = new ShaderCompilation(ilSpyManager, SymbolResolver, method, shaderType);
            compilation.Compile();
            return compilation;
        }
    }
}
