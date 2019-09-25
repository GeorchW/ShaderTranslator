using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ShaderTranslator.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTranslator
{
    public class Parameter
    {
        public string Name { get; }
        public TargetType Type { get; }
        public string? Modifiers { get; }

        public Parameter(string name, TargetType type, string? modifiers)
        {
            Name = name;
            Type = type;
            Modifiers = modifiers;
        }

        internal void Print(IndentedStringBuilder codeBuilder)
        {
            if (Modifiers != null)
            {
                codeBuilder.Write(Modifiers);
                codeBuilder.Write(" ");
            }
            codeBuilder.Write(Type.Name);
            codeBuilder.Write(" ");
            codeBuilder.Write(Name);
        }
    }
    public enum MethodKind
    {
        Regular,
        Constructor
    }
    public class MethodCompilation
    {
        public ShaderCompilation Parent { get; }
        public IMethod Method { get; }
        public string Name { get; }
        public MethodKind Kind { get; }

        NamingScope scope;

        AstNode body;

        TargetType? returnType;
        public TargetType ReturnType => returnType ?? throw new Exception($"Call {nameof(GatherSignature)}() first!");
        Parameter[]? parameters = null;
        public IReadOnlyList<Parameter> Parameters => parameters ?? throw new Exception($"Call {nameof(GatherSignature)}() first!");


        internal MethodCompilation(ShaderCompilation parent, ILSpyManager ilSpyManager, IMethod method, string name)
        {
            this.Method = method;
            Kind = method.IsConstructor ? MethodKind.Constructor : MethodKind.Regular;
            this.Parent = parent;
            this.scope = new NamingScope(parent.GlobalScope);
            Name = name;

            T GetDeclaration<T>()
            => ilSpyManager.GetSyntaxTree(method)
                .Children
                .Where(node => node is T)
                .Cast<T>()
                .Single();

            body = Kind switch
            {
                MethodKind.Regular => GetDeclaration<MethodDeclaration>().Body,
                MethodKind.Constructor => GetDeclaration<ConstructorDeclaration>().Body,
                _ => throw new NotSupportedException()
            };
        }

        internal void GatherSignature()
        {
            returnType = Parent.TypeManager.GetTargetType(
                Kind == MethodKind.Constructor
                ? Method.DeclaringType
                : Method.ReturnType);
            List<Parameter> parameters = new List<Parameter>();
            if (!Method.IsStatic && Method.DeclaringType.Kind == TypeKind.Struct && Kind != MethodKind.Constructor)
            {
                AddParameter(Method.DeclaringType, "_this", -1, "inout");
            }
            int index = 0;
            foreach (var param in Method.Parameters)
            {
                string name = param.GetAttributes().GetName(param.Name)!;
                string? modifiers = param.ReferenceKind switch
                {
                    ReferenceKind.In => "in",
                    ReferenceKind.Out => "out",
                    ReferenceKind.Ref => "inout",
                    _ => null
                };
                AddParameter(param.Type, scope.GetFreeName(name), index,
                    modifiers);
                index++;
            }
            void AddParameter(IType type, string name, int index, string? modifiers = null)
            {
                parameters.Add(new Parameter(name, Parent.TypeManager.GetTargetType(type), modifiers));
                parameterResolve.Add(index, name);
            }
            this.parameters = parameters.ToArray();

            if (Kind == MethodKind.Constructor)
            {
                parameterResolve.Add(-1, "_this");
            }
        }

        internal void Compile()
        {
            GatherSignature();

            IndentedStringBuilder codeBuilder = new IndentedStringBuilder();
            if (Kind == MethodKind.Constructor)
            {
                string innerName = Name + "_ctor_inner";

                // Write inner method
                codeBuilder.Write("void ");
                codeBuilder.Write(innerName);
                codeBuilder.Write("(");
                codeBuilder.Write("inout ");
                codeBuilder.Write(ReturnType.Name);
                codeBuilder.Write(" _this");
                foreach (var param in Parameters)
                {
                    codeBuilder.Write(", ");
                    param.Print(codeBuilder);
                }
                codeBuilder.WriteLine(")");
                WriteMethodBody(codeBuilder);

                // Write wrapper method
                WriteMethodSignature(codeBuilder);
                codeBuilder.WriteLine("{");
                codeBuilder.IncreaseIndent();

                codeBuilder.Write(ReturnType.Name);
                codeBuilder.WriteLine(" _this;");

                codeBuilder.Write(innerName);
                codeBuilder.Write("(_this");
                foreach (var param in Parameters)
                {
                    codeBuilder.Write(", ");
                    codeBuilder.Write(param.Name);
                }
                codeBuilder.WriteLine(");");

                codeBuilder.WriteLine("return _this;");

                codeBuilder.DecreaseIndent();
                codeBuilder.WriteLine("}");
            }
            else
            {
                WriteMethodSignature(codeBuilder);
                WriteMethodBody(codeBuilder);
            }
            code = codeBuilder.ToString();
        }

        internal string GetCode() => code ?? throw new Exception($"Call {nameof(Compile)}() first!");
        string? code;

        Dictionary<int, string> parameterResolve = new Dictionary<int, string>();

        void WriteMethodSignature(IndentedStringBuilder codeBuilder)
        {
            codeBuilder.Write($"{ReturnType.Name} {Name}(");

            bool isFirst = true;
            foreach (var param in Parameters)
            {
                if (isFirst) isFirst = false;
                else codeBuilder.Write(", ");

                param.Print(codeBuilder);
            }
            codeBuilder.Write(")");
            codeBuilder.WriteLine();
        }

        void WriteMethodBody(IndentedStringBuilder codeBuilder)
        {
            if (!Method.HasBody)
                throw new Exception("Method must have a body.");

            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();

            MethodBodyVisitor visitor = new MethodBodyVisitor(codeBuilder, this, scope, parameterResolve);
            body.AcceptVisitor(visitor);

            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");
        }
    }
}
