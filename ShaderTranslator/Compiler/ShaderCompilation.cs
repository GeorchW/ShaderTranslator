using System.Reflection;
using System.Text;
using System.Reflection.Metadata;
using ShaderTranslator.Syntax;

namespace ShaderTranslator
{
    public class ShaderCompilation
    {
        internal TypeManager TypeManager { get; }
        internal MethodManager MethodManager { get; }
        internal NamingScope GlobalScope { get; } = new NamingScope(new NameManager());

        public MethodCompilation EntryPoint { get; }
        internal SymbolResolver SymbolResolver { get; }
        internal UniformManager UniformManager { get; }

        public ShaderType ShaderType { get; }
        public string Code { get; private set; } = null!;

        internal ShaderCompilation(
            ILSpyManager ilSpyManager,
            SymbolResolver symbolResolver,
            MethodInfo rootMethod,
            ShaderType shaderType)
        {
            var decompiler = ilSpyManager.GetDecompiler(rootMethod.Module.Assembly);
            TypeManager = new TypeManager(decompiler.TypeSystem, symbolResolver.MathApi, GlobalScope);
            MethodManager = new MethodManager(this, symbolResolver, ilSpyManager);
            var entryPoint = decompiler.TypeSystem.MainModule.GetDefinition((MethodDefinitionHandle)rootMethod.GetEntityHandle());
            EntryPoint = MethodManager.Require(entryPoint, true);
            SymbolResolver = symbolResolver;
            ShaderType = shaderType;
            UniformManager = new UniformManager(TypeManager, SymbolResolver, GlobalScope);
        }

        internal void Compile()
        {
            while (MethodManager.CompileNextMethod()) ;
            while (TypeManager.CompileNextType()) ;
            IndentedStringBuilder result = new IndentedStringBuilder();
            result.WriteLine("#version 460");
            foreach (var attribute in EntryPoint.Method.GetAttributes())
            {
                if (attribute.AttributeType.FullName != typeof(VerbatimHeaderAttribute).FullName)
                    continue;
                string code = (string)attribute.FixedArguments[0].Value;
                result.WriteLine(code);
            }
            TypeManager.Print(result);
            UniformManager.Print(result);
            MethodManager.Print(result);
            MainMethodGenerator.GenerateMainMethod(EntryPoint, result, ShaderType);
            Code = result.ToString();
        }
    }
}
