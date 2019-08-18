using ICSharpCode.Decompiler.Semantics;
using System;
using System.Reflection;
using ICSharpCode.Decompiler.CSharp;

namespace ShaderTranslator
{
    class CompileEngine
    {
        ILSpyManager ilSpyManager = new ILSpyManager();
        public SymbolResolver SymbolResolver { get; }

        public CompileEngine(SymbolResolver symbolResolver) => SymbolResolver = symbolResolver;

        public string Compile<TIn, TOut>(Func<TIn, TOut> shader, out string entryPoint)
            => Compile(shader.Target, shader.Method, out entryPoint);

        public string Compile(object obj, MethodInfo method, out string entryPoint)
        {
            var compilation = new ShaderCompilation(ilSpyManager, SymbolResolver, method);
            var result = compilation.Compile();
            entryPoint = compilation.EntryPoint.Name;
            return result;
        }
    }
}
