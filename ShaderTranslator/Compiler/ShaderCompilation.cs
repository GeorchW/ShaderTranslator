using System.Reflection;
using System.Text;
using System.Reflection.Metadata;

namespace ShaderTranslator
{
    public class ShaderCompilation
    {
        internal TypeManager TypeManager { get; }
        internal MethodManager MethodManager { get; }
        internal NamingScope GlobalScope { get; } = new NamingScope(new NameManager());

        public MethodCompilation EntryPoint { get; }
        internal SymbolResolver SymbolResolver { get; }
        internal ShaderResourceManager ShaderResourceManager { get; }

        public SemanticsGenerator SemanticsGenerator { get; }

        public string Code { get; private set; } = null!;

        internal ShaderCompilation(
            ILSpyManager ilSpyManager,
            SymbolResolver symbolResolver,
            MethodInfo rootMethod,
            SemanticsGenerator semanticsGenerator)
        {
            var decompiler = ilSpyManager.GetDecompiler(rootMethod.Module.Assembly);
            TypeManager = new TypeManager(decompiler.TypeSystem, symbolResolver.MathApi, GlobalScope);
            MethodManager = new MethodManager(this, symbolResolver, ilSpyManager);
            var entryPoint = decompiler.TypeSystem.MainModule.GetDefinition((MethodDefinitionHandle)rootMethod.GetEntityHandle());
            EntryPoint = MethodManager.Require(entryPoint, true);
            SymbolResolver = symbolResolver;
            SemanticsGenerator = semanticsGenerator;
            ShaderResourceManager = new ShaderResourceManager(TypeManager, SymbolResolver, GlobalScope);
        }

        internal void Compile()
        {
            while (MethodManager.CompileNextMethod()) ;
            while (TypeManager.CompileNextType()) ;
            SemanticsGenerator.GenerateSemantics(EntryPoint);
            IndentedStringBuilder result = new IndentedStringBuilder();
            TypeManager.Print(result);
            ShaderResourceManager.Print(result);
            MethodManager.Print(result);
            Code = result.ToString();
        }
    }
}
