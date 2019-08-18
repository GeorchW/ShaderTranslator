using ICSharpCode.Decompiler.Semantics;
using System;
using System.Reflection;
using ICSharpCode.Decompiler.CSharp;

namespace ShaderTranslator
{
    class CompileEngine
    {
        ILSpyManager ilSpyManager = new ILSpyManager();

        public string Compile<TIn, TOut>(Func<TIn, TOut> shader, out string entryPoint)
            => Compile(shader.Target, shader.Method, out entryPoint);

        public string Compile(object obj, MethodInfo method, out string entryPoint)
        {
            //TODO: do we need the object, actually?
            var compilation = new ShaderCompilation(ilSpyManager, method);
            var result = compilation.Compile();
            entryPoint = compilation.EntryPoint.Name;
            return result;
        }
    }
}
