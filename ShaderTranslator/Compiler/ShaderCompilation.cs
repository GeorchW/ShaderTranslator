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
        public SymbolResolver SymbolResolver { get; }
        public ShaderResourceManager ShaderResourceManager { get; } 

        public ShaderCompilation(ILSpyManager ilSpyManager, SymbolResolver symbolResolver, MethodInfo rootMethod)
        {
            var decompiler = ilSpyManager.GetDecompiler(rootMethod.Module.Assembly);
            TypeManager = new TypeManager(decompiler.TypeSystem, symbolResolver, GlobalScope);
            MethodManager = new MethodManager(this, symbolResolver, ilSpyManager);
            var entryPoint = decompiler.TypeSystem.MainModule.GetDefinition((MethodDefinitionHandle)rootMethod.GetEntityHandle());
            EntryPoint = MethodManager.Require(entryPoint, true);
            SymbolResolver = symbolResolver;
            ShaderResourceManager = new ShaderResourceManager(TypeManager, SymbolResolver, GlobalScope);
        }

        public string Compile()
        {
            while (MethodManager.CompileNextMethod()) ;
            while (TypeManager.CompileNextType()) ;
            IndentedStringBuilder result = new IndentedStringBuilder();
            TypeManager.Print(result);
            ShaderResourceManager.Print(result);
            MethodManager.Print(result);
            return result.ToString();
        }
    }
}
