using System.Reflection;
using System.Text;
using System.Reflection.Metadata;

namespace ShaderTranslator
{
    class ShaderCompilation
    {
        public TypeManager TypeManager { get; }
        public MethodManager MethodManager { get; }
        public NamingScope GlobalScope { get; } = new NamingScope(new NameManager());

        public MethodCompilation EntryPoint { get; }

        public ShaderCompilation(ILSpyManager ilSpyManager, MethodInfo rootMethod)
        {
            var decompiler = ilSpyManager.GetDecompiler(rootMethod.Module.Assembly);
            TypeManager = new TypeManager(decompiler.TypeSystem, GlobalScope);
            MethodManager = new MethodManager(this, ilSpyManager);
            var entryPoint = decompiler.TypeSystem.MainModule.GetDefinition((MethodDefinitionHandle)rootMethod.GetEntityHandle());
            EntryPoint = MethodManager.Require(entryPoint, true);
        }

        public string Compile()
        {
            while (MethodManager.CompileNextMethod()) ;
            while (TypeManager.CompileNextType()) ;
            StringBuilder result = new StringBuilder();
            TypeManager.Print(result);
            MethodManager.Print(result);
            return result.ToString();
        }
    }
}
