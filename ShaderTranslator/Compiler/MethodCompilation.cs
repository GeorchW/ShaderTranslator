using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTranslator
{
    class MethodCompilation
    {
        IndentedStringBuilder codeBuilder = new IndentedStringBuilder();
        NamingScope scope;
        ShaderCompilation parent;

        MethodDeclaration declaration;
        private IMethod method;

        string? code = null;
        public string Code => code ?? throw new Exception("Need to compile first!");

        public string Name { get; }
        public bool IsRoot { get; }

        public MethodCompilation(ShaderCompilation parent, ILSpyManager ilSpyManager, IMethod method, bool isRoot)
        {
            this.method = method;
            IsRoot = isRoot;
            this.parent = parent;
            this.scope = new NamingScope(parent.GlobalScope);
            Name = parent.GlobalScope.GetFreeName(method.Name);

            declaration = ilSpyManager.GetSyntaxTree(method)
                .Children
                .Where(node => node is MethodDeclaration)
                .Cast<MethodDeclaration>()
                .Single();
        }

        public void Compile()
        {
            WriteMethodSignature();
            WriteMethodBody();
            code = codeBuilder.ToString();
        }

        Dictionary<int, string> parameters = new Dictionary<int, string>();

        void WriteMethodSignature()
        {
            codeBuilder.Write($"{parent.TypeManager.GetTypeString(method.ReturnType)} {Name}(");

            bool isFirst = true;
            if (!method.IsStatic)
            {
                //AddParameter(method.DeclaringType, "this", -1);
            }
            int index = 0;
            int texCoordIndex = 0;
            foreach (var param in method.Parameters)
            {
                AddParameter(param.Type, scope.GetFreeName(param.Name), index);
                index++;
            }
            void AddParameter(IType type, string name, int index)
            {
                if (isFirst) isFirst = false;
                else codeBuilder.Write(", ");

                codeBuilder.Write(parent.TypeManager.GetTypeString(type));
                codeBuilder.Write(" ");
                codeBuilder.Write(name);

                if (IsRoot)
                {
                    codeBuilder.Write(" : TEXCOORD");
                    codeBuilder.Write(texCoordIndex);
                }

                parameters.Add(index, name);
            }
            codeBuilder.Write(")");
            if(IsRoot)
            {
                codeBuilder.Write(" : SV_Target0");
            }
            codeBuilder.WriteLine();
        }

        void WriteMethodBody()
        {
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();

            MethodBodyVisitor visitor = new MethodBodyVisitor(codeBuilder, parent.TypeManager, scope, parameters);
            declaration.Body.AcceptVisitor(visitor);

            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");
        }
    }
}
