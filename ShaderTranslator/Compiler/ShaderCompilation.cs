using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Reflection.Metadata;
using ShaderTranslator.Syntax;
using System;

namespace ShaderTranslator
{
    public enum UniformType
    {
        UBO,
        Texture,
    }
    public class Uniform
    {
        public UniformType UniformType {get;}
        public Type Type { get; }
        public int Slot { get; }
        public string Name { get; }

        internal Uniform(UniformType uniformType, Type type, int slot, string name)
        {
            UniformType = uniformType;
            Type = type;
            Slot = slot;
            Name = name;
        }
    }
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
        public IReadOnlyCollection<Uniform> Uniforms { get; private set; } = null!;

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
            EntryPoint = MethodManager.Require(entryPoint);
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
                string code = (string)(attribute.FixedArguments[0].Value ?? throw new Exception("Verbatim headers may not be null."));
                result.WriteLine(code);
            }
            TypeManager.Print(result);
            UniformManager.Print(result);
            MethodManager.Print(result);
            MainMethodGenerator.GenerateMainMethod(EntryPoint, result, ShaderType);
            Code = result.ToString();

            var uniforms = new List<Uniform>();
            foreach (var uniform in UniformManager.Uniforms)
            {
                var uniformType = uniform is TextureCompilation ? UniformType.Texture : UniformType.UBO;
                uniforms.Add(new Uniform(uniformType, uniform.Variable.Type.ToReflectionType() ?? throw new NullReferenceException(), uniform.Slot, uniform.Name));
            }
            Uniforms = uniforms.AsReadOnly();
        }
    }
}
