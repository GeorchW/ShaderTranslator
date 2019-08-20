using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShaderTranslator
{
    class ShaderResourceManager
    {
        Dictionary<IVariable, ShaderResourceCompilation> shaderResources = new Dictionary<IVariable, ShaderResourceCompilation>();
        HashSet<IVariable> seenFields = new HashSet<IVariable>();
        TypeManager typeManager;
        SymbolResolver symbolResolver;
        NamingScope globalScope;

        public ShaderResourceManager(TypeManager typeManager, SymbolResolver symbolResolver, NamingScope globalScope)
        {
            this.typeManager = typeManager;
            this.symbolResolver = symbolResolver;
            this.globalScope = globalScope;
        }

        public ShaderResourceCompilation? Require(IVariable variable)
        {
            if (seenFields.Add(variable))
            {
                if (shaderResources.TryGetValue(variable, out var result))
                    return result;
                IEnumerable<IAttribute> attributes;
                if (variable is IField field)
                    attributes = field.GetAttributes();
                else if (variable is IParameter parameter)
                    attributes = parameter.GetAttributes();
                else
                    return null;

                if (!attributes.TryGetAttribute(typeof(ShaderResourceAttribute), out var attr))
                    return null;

                string name = globalScope.GetFreeName(variable.Name);
                if (symbolResolver.IsTextureType(variable.Type))
                {
                    string samplerName = globalScope.GetFreeName($"{variable.Name}_Sampler");
                    result = new TextureCompilation(variable, attr, name, samplerName);
                }
                else
                {
                    result = new ConstantBufferCompilation(variable, attr, name);
                }
                shaderResources.Add(variable, result);
                return result;
            }
            return null;
        }

        public void Print(IndentedStringBuilder codeBuilder)
        {
            var shaderResources = from shaderResource in this.shaderResources.Values
                                  orderby shaderResource.Set
                                  orderby shaderResource.Slot
                                  select shaderResource;
            foreach (var shaderResource in shaderResources)
            {
                shaderResource.Print(codeBuilder, typeManager);
            }
        }
    }
    class TextureCompilation : ShaderResourceCompilation
    {
        public string SamplerName { get; }

        public TextureCompilation(IVariable variable, IAttribute attribute, string name, string samplerName) : base(variable, attribute, name)
        {
            SamplerName = samplerName;
        }

        public void InvokeSampleCall(
            IndentedStringBuilder codeBuilder,
            InvocationExpression invocationExpression,
            MethodBodyVisitor astVisitor)
        {
            codeBuilder.Write(Name);
            codeBuilder.Write(".");
            if (!(invocationExpression.Target is MemberReferenceExpression mem))
                throw new Exception();
            codeBuilder.Write(mem.MemberName);
            codeBuilder.Write("(");
            codeBuilder.Write(SamplerName);
            foreach (var arg in invocationExpression.Arguments)
            {
                codeBuilder.Write(", ");
                arg.AcceptVisitor(astVisitor);
            }
            codeBuilder.Write(")");
        }
        public override void Print(IndentedStringBuilder codeBuilder, TypeManager typeManager)
        {
            codeBuilder.Write("Texture2D ");
            codeBuilder.Write(Name);
            codeBuilder.Write(" : register(t");
            codeBuilder.Write(Slot);
            codeBuilder.Write(");");

            codeBuilder.Write("SamplerState ");
            codeBuilder.Write(SamplerName);
            codeBuilder.Write(" : register(s");
            codeBuilder.Write(Slot);
            codeBuilder.Write(");");
        }
    }
    class ConstantBufferCompilation : ShaderResourceCompilation
    {
        public ConstantBufferCompilation(IVariable variable, IAttribute attribute, string name) : base(variable, attribute, name)
        {
        }


        public override void Print(IndentedStringBuilder codeBuilder, TypeManager typeManager)
        {
            codeBuilder.Write("cbuffer ");
            codeBuilder.WriteLine(Name);
            codeBuilder.Write(" : register(b");
            codeBuilder.Write(Slot);
            codeBuilder.Write(")");
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();

            codeBuilder.Write(typeManager.GetTypeString(Variable.Type));
            codeBuilder.Write(" ");
            codeBuilder.Write(Name);
            codeBuilder.WriteLine(";");

            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");
        }
    }

    abstract class ShaderResourceCompilation
    {
        public int Set { get; }
        public int Slot { get; }
        public IVariable Variable { get; }
        public string Name { get; }

        public ShaderResourceCompilation(IVariable variable, IAttribute attribute, string name)
        {
            Variable = variable;
            Name = name;
            Set = (int)attribute.FixedArguments[0].Value;
            Slot = (int)attribute.FixedArguments[1].Value;
        }

        public abstract void Print(IndentedStringBuilder codeBuilder, TypeManager typeManager);
    }
}
