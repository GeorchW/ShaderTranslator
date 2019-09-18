using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
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
                    result = new TextureCompilation(variable, attr, name);
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
        public TextureCompilation(IVariable variable, IAttribute attribute, string name) : base(variable, attribute, name)
        {
        }

        public void InvokeSampleCall(
            IndentedStringBuilder codeBuilder,
            InvocationExpression invocationExpression,
            MethodBodyVisitor astVisitor)
        {
            if (!(invocationExpression.Target is MemberReferenceExpression mem))
                throw new Exception();

            string methodName = mem.MemberName;
            var method = (invocationExpression.Annotation<InvocationResolveResult>()?.Member as IMethod);
            methodName = method?.GetAttributes().GetName(methodName) ?? methodName;

            codeBuilder.Write(methodName);
            codeBuilder.Write("(");
            codeBuilder.Write(Name);
            foreach (var arg in invocationExpression.Arguments)
            {
                codeBuilder.Write(", ");
                arg.AcceptVisitor(astVisitor);
            }
            codeBuilder.Write(")");
        }
        public override void Print(IndentedStringBuilder codeBuilder, TypeManager typeManager)
        {
            codeBuilder.Write("layout(location = ");
            codeBuilder.Write(Slot);
            codeBuilder.Write(")");
            codeBuilder.Write(" uniform sampler2D ");
            codeBuilder.Write(Name);
            codeBuilder.WriteLine(";");
        }
    }
    class ConstantBufferCompilation : ShaderResourceCompilation
    {
        public ConstantBufferCompilation(IVariable variable, IAttribute attribute, string name) : base(variable, attribute, name)
        {
        }


        public override void Print(IndentedStringBuilder codeBuilder, TypeManager typeManager)
        {
            codeBuilder.Write("layout(std140, binding = ");
            codeBuilder.Write(Slot);
            codeBuilder.Write(") uniform ");
            //TODO: requires a proper name
            codeBuilder.Write(Name + "_asdfqwetr_constant_buffer");
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();

            codeBuilder.Write(typeManager.GetTypeString(this.Variable.Type));
            codeBuilder.Write(" ");
            codeBuilder.Write(Name);
            codeBuilder.WriteLine(";");

            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("};");
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
