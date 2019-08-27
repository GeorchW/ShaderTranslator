using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTranslator
{
    public class SemanticsElement
    {
        public string? Semantics { get; set; } = null;
        public TargetType Type { get; }
        public SemanticsElement(TargetType type) => Type = type;
    }
    public class Parameter : SemanticsElement
    {
        public string Name { get; }
        public Parameter(string name, TargetType type) : base(type) => Name = name;
    }
    public class MethodCompilation
    {
        public ShaderCompilation Parent { get; }
        public IMethod Method { get; }
        public string Name { get; }
        public bool IsRoot { get; }

        NamingScope scope;

        MethodDeclaration declaration;

        SemanticsElement? returnType;
        public SemanticsElement ReturnType => returnType ?? throw new Exception($"Call {nameof(GatherSignature)}() first!");
        Parameter[]? parameters = null;
        public IReadOnlyList<Parameter> Parameters => parameters ?? throw new Exception($"Call {nameof(GatherSignature)}() first!");

        string? bodyCode;
        string BodyCode => bodyCode ?? throw new Exception($"Call {nameof(Compile)}() first!");


        internal MethodCompilation(ShaderCompilation parent, ILSpyManager ilSpyManager, IMethod method, string name, bool isRoot)
        {
            this.Method = method;
            IsRoot = isRoot;
            this.Parent = parent;
            this.scope = new NamingScope(parent.GlobalScope);
            Name = name;

            declaration = ilSpyManager.GetSyntaxTree(method)
                .Children
                .Where(node => node is MethodDeclaration)
                .Cast<MethodDeclaration>()
                .Single();
        }

        internal void GatherSignature()
        {
            returnType = new SemanticsElement(Parent.TypeManager.GetTargetType(Method.ReturnType));
            List<Parameter> parameters = new List<Parameter>();
            if (!Method.IsStatic)
            {
                //AddParameter(method.DeclaringType, "this", -1);
            }
            int index = 0;
            foreach (var param in Method.Parameters)
            {
                AddParameter(param.Type, scope.GetFreeName(param.Name), index);
                index++;
            }
            void AddParameter(IType type, string name, int index)
            {
                parameters.Add(new Parameter(name, Parent.TypeManager.GetTargetType(type)));
                parameterResolve.Add(index, name);
            }
            this.parameters = parameters.ToArray();
        }

        internal void Compile()
        {
            //WriteMethodSignature(codeBuilder);
            //WriteMethodBody(codeBuilder);

            GatherSignature();
            WriteMethodBody();
        }

        internal string GetCode() => WriteMethodSignature() + BodyCode;

        Dictionary<int, string> parameterResolve = new Dictionary<int, string>();

        string WriteMethodSignature()
        {
            IndentedStringBuilder codeBuilder = new IndentedStringBuilder();
            codeBuilder.Write($"{ReturnType.Type.Name} {Name}(");

            bool isFirst = true;
            foreach (var param in Parameters)
            {
                if (isFirst) isFirst = false;
                else codeBuilder.Write(", ");

                codeBuilder.Write(param.Type.Name);
                codeBuilder.Write(" ");
                codeBuilder.Write(param.Name);

                if (param.Semantics != null)
                {
                    codeBuilder.Write(" : ");
                    codeBuilder.Write(param.Semantics);
                }
            }
            codeBuilder.Write(")");
            if (ReturnType.Semantics != null)
            {
                codeBuilder.Write(" : ");
                codeBuilder.Write(ReturnType.Semantics);
            }
            codeBuilder.WriteLine();
            return codeBuilder.ToString();
        }

        void WriteMethodBody()
        {
            if (!Method.HasBody)
                throw new Exception("Method must have a body.");
            IndentedStringBuilder codeBuilder = new IndentedStringBuilder();

            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();

            MethodBodyVisitor visitor = new MethodBodyVisitor(codeBuilder, this, scope, parameterResolve);
            declaration.Body.AcceptVisitor(visitor);

            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");

            bodyCode = codeBuilder.ToString();
        }
    }
}
