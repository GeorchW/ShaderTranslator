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
            TypeManager = new TypeManager(decompiler.TypeSystem);
            MethodManager = new MethodManager(this, ilSpyManager);
            var entryPoint = decompiler.TypeSystem.MainModule.GetDefinition((MethodDefinitionHandle)rootMethod.GetEntityHandle());
            EntryPoint = MethodManager.Require(entryPoint, true);
        }

        public string Compile()
        {
            while (MethodManager.CompileNextMethod()) ;
            StringBuilder result = new StringBuilder();
            MethodManager.Print(result);
            return result.ToString();
        }
    }
}
