using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.IL;

namespace ShaderTranslator
{
    class MethodBodyVisitor : DepthFirstAstVisitor
    {
        IndentedStringBuilder codeBuilder;
        MethodCompilation parent;
        NamingScope scope;

        TypeManager TypeManager => parent.Parent.TypeManager;
        MethodManager MethodManager => parent.Parent.MethodManager;
        SymbolResolver SymbolResolver => parent.Parent.SymbolResolver;
        ShaderResourceManager ShaderResourceManager => parent.Parent.ShaderResourceManager;

        Dictionary<int, string> parameterTranslation = new Dictionary<int, string>();
        Dictionary<int, string> variableTranslation = new Dictionary<int, string>();

        public MethodBodyVisitor(
            IndentedStringBuilder codeBuilder,
            MethodCompilation parent,
            NamingScope parentScope,
            Dictionary<int, string> parameters)
        {
            this.codeBuilder = codeBuilder;
            this.parent = parent;
            this.scope = new NamingScope(parentScope);

            parameterTranslation = parameters;
        }

        protected override sealed void VisitChildren(AstNode node)
            => throw new NotSupportedException($"'{node.GetType().Name}' nodes can't be translated yet.");
        protected void VisitChildrenInternal(AstNode node) => base.VisitChildren(node);

        public override void VisitBlockStatement(BlockStatement blockStatement) => VisitChildrenInternal(blockStatement);
        public override void VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            if (expressionStatement.Expression is InvocationExpression invocation)
            {
                var member = invocation.Annotation<InvocationResolveResult>().Member;
                if (member.FullNameIs("ShaderTranslator.Syntax.ShaderMethods", "Unroll"))
                {
                    unrollNextLoop = true;
                    return;
                }
                else if (member.FullNameIs("ShaderTranslator.Syntax.ShaderMethods", "Discard"))
                {
                    codeBuilder.WriteLine("discard;");
                    return;
                }
            }
            VisitChildrenInternal(expressionStatement);
            codeBuilder.WriteLine(";");
        }

        public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            var invocation = binaryOperatorExpression.Annotation<InvocationResolveResult>();
            if (invocation != null)
            {
                throw new Exception("Custom binary operators are not supported yet");
            }
            binaryOperatorExpression.Left.AcceptVisitor(this);
            codeBuilder.Write(" ");
            codeBuilder.Write(OperatorHelper.GetOperatorString(binaryOperatorExpression.Operator));
            codeBuilder.Write(" ");
            binaryOperatorExpression.Right.AcceptVisitor(this);
        }

        public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
        {
            foreach (var variable in variableDeclarationStatement.Variables)
            {
                codeBuilder.Write(TypeManager.GetTypeString(variableDeclarationStatement.Type));
                codeBuilder.Write(" ");
                variable.AcceptVisitor(this);
                codeBuilder.WriteLine(";");
            }
        }

        public override void VisitVariableInitializer(VariableInitializer variableInitializer)
        {
            string variableName = scope.GetFreeName(variableInitializer.Name);
            variableTranslation.Add(
                variableInitializer.Annotation<ILVariableResolveResult>().Variable.IndexInFunction,
                variableName);
            codeBuilder.Write(variableName);
            if (variableInitializer.Initializer != null)
            {
                codeBuilder.Write(" = ");
                variableInitializer.Initializer.AcceptVisitor(this);
            }
        }

        public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        {
            var constant = primitiveExpression.Annotation<ConstantResolveResult>();
            if (constant != null)
                PrintConstant(constant.ConstantValue);
            else
                PrintConstant(primitiveExpression.Value);

            void PrintConstant(object constant)
            {
                if (constant is bool b)
                    codeBuilder.Write(b ? "true" : "false");
                else if (constant is IFormattable f)
                    codeBuilder.Write(f);
                else
                    throw new NotSupportedException();
            }
        }

        public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
        {
            if (TryLocalAccess(identifierExpression)) return;
            throw new NotImplementedException();
        }

        bool TryLocalAccess(AstNode node)
        {
            var local = node.Annotation<ILVariableResolveResult>();
            if (local != null)
            {
                if (local.Variable.Kind == VariableKind.Parameter)
                    codeBuilder.Write(parameterTranslation[local.Variable.Index!.Value]);
                else
                    codeBuilder.Write(variableTranslation[local.Variable.IndexInFunction]);
                return true;
            }
            else return false;
        }

        public override void VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Expression == null)
                codeBuilder.WriteLine("return;");
            else
            {
                codeBuilder.Write("return ");
                returnStatement.Expression.AcceptVisitor(this);
                codeBuilder.WriteLine(";");
            }
        }

        bool unrollNextLoop = false;

        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            var method = (IMethod) invocationExpression.Annotation<InvocationResolveResult>().Member;
            if (SymbolResolver.IsTextureType(method.DeclaringType))
            {
                var target = (invocationExpression.Target as MemberReferenceExpression)?.Target?.Annotation<MemberResolveResult>()?.Member as IField;
                if (target == null)
                    throw new Exception("Textures can only be used directly."); //TODO: textures being passed around?
                var result = ShaderResourceManager.Require(target) as TextureCompilation;
                if (result == null)
                    throw new Exception("Textures must be marked as ShaderResource.");
                result.InvokeSampleCall(codeBuilder, invocationExpression, this);
                return;
            }
            if (method.Substitution != TypeParameterSubstitution.Identity)
                throw new NotImplementedException("Generic methods are not implemented.");

            var compilation = MethodManager.Require(method);
            if(!method.IsStatic)
            {
                throw new NotImplementedException("Instance methods are not supported yet");
            }
            codeBuilder.Write(compilation.Name);
            codeBuilder.Write("(");
            bool isFirst = true;
            if (!method.IsStatic)
            {
                AddParameter(invocationExpression.Target);
            }
            foreach (var param in invocationExpression.Arguments)
            {
                AddParameter(param);
            }
            void AddParameter(AstNode param)
            {
                if (isFirst) isFirst = false;
                else codeBuilder.Write(", ");
                param.AcceptVisitor(this);
            }
            codeBuilder.Write(")");
        }

        public override void VisitWhileStatement(WhileStatement whileStatement)
        {
            if (unrollNextLoop)
            {
                codeBuilder.WriteLine("[unroll]");
                unrollNextLoop = false;
            }

            codeBuilder.Write("while (");
            whileStatement.Condition.AcceptVisitor(this);
            codeBuilder.WriteLine(")\n{");
            codeBuilder.IncreaseIndent();
            whileStatement.EmbeddedStatement.AcceptVisitor(this);
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");
        }

        public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
        {
            codeBuilder.Write("if (");
            ifElseStatement.Condition.AcceptVisitor(this);
            codeBuilder.WriteLine(")\n{");
            codeBuilder.IncreaseIndent();
            ifElseStatement.TrueStatement.AcceptVisitor(this);
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");
            if (!ifElseStatement.FalseStatement.IsNull)
            {
                codeBuilder.WriteLine("else\n{");
                codeBuilder.IncreaseIndent();
                ifElseStatement.FalseStatement.AcceptVisitor(this);
                codeBuilder.DecreaseIndent();
                codeBuilder.WriteLine("}");
            }
        }

        public override void VisitBreakStatement(BreakStatement breakStatement) => codeBuilder.WriteLine("break;");
        public override void VisitContinueStatement(ContinueStatement continueStatement) => codeBuilder.WriteLine("continue;");

        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            var ilVar = assignmentExpression.Left.Annotation<ILVariableResolveResult>();
            if (ilVar != null)
            {
                if (!TryLocalAccess(assignmentExpression.Left))
                    throw new NotImplementedException();
                codeBuilder.Write(OperatorHelper.GetOperatorString(assignmentExpression.Operator));
                assignmentExpression.Right.AcceptVisitor(this);
                return;
            }
            throw new NotImplementedException();
        }

        public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            codeBuilder.Write(OperatorHelper.GetOperatorString(unaryOperatorExpression.Operator));
            unaryOperatorExpression.Expression.AcceptVisitor(this);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
        {
            codeBuilder.Write("(");
            parenthesizedExpression.Expression.AcceptVisitor(this);
            codeBuilder.Write(")");
        }

        public override void VisitCastExpression(CastExpression castExpression)
        {
            var resolveResult = castExpression.Annotation<ConversionResolveResult>();
            if (resolveResult.Conversion.IsUserDefined)
                throw new Exception("Custom cast operators are not supported yet");
            codeBuilder.Write("(");
            codeBuilder.Write(TypeManager.GetTypeString(resolveResult.Type));
            codeBuilder.Write(")");
            castExpression.Expression.AcceptVisitor(this);
        }

        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            var memberResolveResult = memberReferenceExpression.Annotation<MemberResolveResult>();
            if (memberResolveResult.Member is IField field)
            {
                memberReferenceExpression.Target.AcceptVisitor(this);
                codeBuilder.Write(".");
                codeBuilder.Write(SymbolResolver.TryResolve(field) ?? field.Name);
            }
            else throw new NotImplementedException();
        }

        public override void VisitIndexerExpression(IndexerExpression indexerExpression)
        {
            indexerExpression.Target.AcceptVisitor(this);
            codeBuilder.Write("[");
            bool isFirst = true;
            foreach (var index in indexerExpression.Arguments)
            {
                if (isFirst) isFirst = false;
                else codeBuilder.Write(", ");
                index.AcceptVisitor(this);
            }
            codeBuilder.Write("]");
        }
    }
}
