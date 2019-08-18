using ICSharpCode.Decompiler.TypeSystem;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ShaderTranslator
{
    class MethodManager
    {
        List<MethodCompilation> methods = new List<MethodCompilation>();
        Dictionary<IMethod, MethodCompilation> seenMethods = new Dictionary<IMethod, MethodCompilation>();
        Queue<MethodCompilation> toBeVisited = new Queue<MethodCompilation>();
        ILSpyManager ilSpyManager;
        ShaderCompilation parent;
        public MethodManager(ShaderCompilation parent, ILSpyManager ilSpyManager)
        {
            this.parent = parent;
            this.ilSpyManager = ilSpyManager;
        }
        public MethodCompilation Require(IMethod method, bool isRoot = false)
        {
            if (seenMethods.TryGetValue(method, out var result))
                return result;
            else
            {
                result = new MethodCompilation(parent, ilSpyManager, method, isRoot);
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
        public void Print(StringBuilder target)
        {
            foreach (var str in methods.Reverse<MethodCompilation>())
            {
                target.AppendLine(str.Code);
            }
        }
    }
}
