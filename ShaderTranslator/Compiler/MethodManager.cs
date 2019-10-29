using ICSharpCode.Decompiler.TypeSystem;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

namespace ShaderTranslator
{
    class MethodManager
    {
        List<MethodCompilation> methods = new List<MethodCompilation>();
        Dictionary<IMethod, MethodCompilation> seenMethods = new Dictionary<IMethod, MethodCompilation>();
        Queue<MethodCompilation> toBeVisited = new Queue<MethodCompilation>();
        ILSpyManager ilSpyManager;
        ShaderCompilation parent;
        SymbolResolver symbolResolver;

        List<string> requiredCode = new List<string>();
        HashSet<IParameterizedMember> requiredCodeSet = new HashSet<IParameterizedMember>();

        public MethodManager(ShaderCompilation parent, SymbolResolver symbolResolver, ILSpyManager ilSpyManager)
        {
            this.parent = parent;
            this.symbolResolver = symbolResolver;
            this.ilSpyManager = ilSpyManager;
        }
        public MethodCompilation Require(IMethod method)
        {
            if (seenMethods.TryGetValue(method, out var result))
                return result;
            else if (symbolResolver.TryResolve(method) is ResolveResult resolveResult)
            {
                if (resolveResult.RequiredCode != null)
                    RegisterRequiredCode(method, resolveResult.RequiredCode);
                result = new MethodCompilation(parent, ilSpyManager, method, resolveResult.Name);
                seenMethods.Add(method, result);
            }
            else
            {
                var name = parent.GlobalScope.GetFreeName(method.DeclaringType.Name + "_" + method.Name);
                result = new MethodCompilation(parent, ilSpyManager, method, name);
                seenMethods.Add(method, result);
                toBeVisited.Enqueue(result);
            }
            return result;
        }
        public bool CompileNextMethod()
        {
            if (!toBeVisited.TryDequeue(out var method))
                return false;

            method.Compile();
            methods.Add(method);
            return true;
        }
        public void Print(IndentedStringBuilder target)
        {
            target.WriteLine("// Required code from math usage");
            foreach (var code in requiredCode)
            {
                target.WriteLine(code);
            }
            target.WriteLine("// User-defined methods");
            foreach (var str in methods.Reverse<MethodCompilation>())
            {
                target.WriteLine(str.GetCode());
            }
        }

        public void RegisterRequiredCode(IParameterizedMember member, string requiredCode)
        {
            if (requiredCodeSet.Add(member))
                this.requiredCode.Add(requiredCode);
        }
    }
}
