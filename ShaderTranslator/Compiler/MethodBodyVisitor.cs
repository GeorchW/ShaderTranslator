using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.IL;
using System.Globalization;

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
        UniformManager UniformManager => parent.Parent.UniformManager;

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
                    //unrollNextLoop = true;
                    return;
                }
                else if (member.FullNameIs("ShaderTranslator.Syntax.ShaderMethods", "Discard"))
                {
                    codeBuilder.WriteLine("discard;");
                    return;
                }
                else if (member.FullNameIs("ShaderTranslator.Syntax.ShaderMethods", "GlslVerbatim"))
                {
                    var code = invocation.Arguments.FirstOrNullObject()?.Annotation<ConstantResolveResult>()?.ConstantValue as string;
                    if (code == null)
                        throw new Exception("GlslVerbatim must have a constant string argument.");
                    codeBuilder.WriteLine(code);
                    return;
                }
            }
            VisitChildrenInternal(expressionStatement);
            codeBuilder.WriteLine(";");
        }

        public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            var invocation = binaryOperatorExpression.Annotation<InvocationResolveResult>();
            string op = OperatorHelper.GetOperatorString(binaryOperatorExpression.Operator);
            if (invocation != null)
            {
                if (SymbolResolver.TryResolve(invocation.Member) is ResolveResult result
                    && result.Type == ResolveType.Operator
                    && result.Name == op)
                {
                    VisitAsUsual();
                }
                else VisitInvocation(invocation, new[] { binaryOperatorExpression.Left, binaryOperatorExpression.Right });
            }
            else if (TryVisitAsDoubleComparison())
                return;
            else
                VisitAsUsual();

            // TODO: This should be a prepass in a separate visitor.
            bool TryVisitAsDoubleComparison()
            {
                if (op == "<" || op == ">" || op == "<=" || op == ">=" || op == "==" || op == "!=")
                {
                    if (IsDoubleWith(binaryOperatorExpression.Left, out ConversionResolveResult conversion)
                     && IsDoubleWith(binaryOperatorExpression.Right, out ConstantResolveResult constant))
                    {
                        WriteCast(binaryOperatorExpression.Left);
                        WriteOp();
                        WriteConstant(constant);
                        return true;
                    }
                    else if (IsDoubleWith(binaryOperatorExpression.Left, out constant)
                     && IsDoubleWith(binaryOperatorExpression.Right, out conversion))
                    {
                        WriteConstant(constant);
                        WriteOp();
                        WriteCast(binaryOperatorExpression.Right);
                        return true;
                    }

                    static bool IsDoubleWith<T>(AstNode node, out T conversionResolveResult) 
                        where T : ICSharpCode.Decompiler.Semantics.ResolveResult
                    {
                        conversionResolveResult = node.Annotation<T>();
                        return conversionResolveResult != null && conversionResolveResult.Type.IsKnownType(KnownTypeCode.Double);
                    }

                    void WriteCast(AstNode node)
                    {
                        if (binaryOperatorExpression.Left is CastExpression cast)
                            cast.Expression.AcceptVisitor(this);
                        else
                            binaryOperatorExpression.Left.AcceptVisitor(this);
                    }
                    void WriteOp()
                    {
                        codeBuilder.Write(" ");
                        codeBuilder.Write(op);
                        codeBuilder.Write(" ");
                    }
                    void WriteConstant(ConstantResolveResult constant)
                    {
                        codeBuilder.Write(((double)constant.ConstantValue).ToString("0.0", CultureInfo.InvariantCulture));
                    }
                }
                return false;
            }


            void VisitAsUsual()
            {
                binaryOperatorExpression.Left.AcceptVisitor(this);
                codeBuilder.Write(" ");
                codeBuilder.Write(op);
                codeBuilder.Write(" ");
                binaryOperatorExpression.Right.AcceptVisitor(this);
            }
        }

        void VisitInvocation(InvocationResolveResult invocation, IReadOnlyList<Expression> arguments)
        {
            if (SymbolResolver.TryResolve(invocation.Member) is ResolveResult result)
            {
                if (result.RequiredCode != null)
                {
                    MethodManager.RegisterRequiredCode(invocation.Member, result.RequiredCode);
                }
                switch (result.Type)
                {
                    case ResolveType.Field:
                        if (arguments.Count != 1)
                            throw new Exception();
                        arguments[0].AcceptVisitor(this);
                        codeBuilder.Write(".");
                        codeBuilder.Write(result.Name);
                        break;
                    case ResolveType.Method:
                        WriteCall(result.Name, arguments);
                        break;
                    case ResolveType.Operator:
                        if (arguments.Count == 1)
                        {
                            codeBuilder.Write(result.Name);
                            codeBuilder.Write("(");
                            arguments[0].AcceptVisitor(this);
                            codeBuilder.Write(")");
                        }
                        else if (arguments.Count == 2)
                        {
                            codeBuilder.Write("((");
                            arguments[0].AcceptVisitor(this);
                            codeBuilder.Write(") ");
                            codeBuilder.Write(result.Name);
                            codeBuilder.Write(" (");
                            arguments[1].AcceptVisitor(this);
                            codeBuilder.Write("))");
                        }
                        else throw new Exception();
                        break;
                    default:
                        throw new Exception();
                }
            }
            else
            {
                var method = (IMethod)invocation.Member;
                if (method.TypeParameters.Count != 0)
                    throw new NotImplementedException("Generic methods are not implemented.");

                var compilation = MethodManager.Require(method);
                WriteCall(compilation.Name, arguments);
            }
        }
        void WriteCall(string methodName, IEnumerable<Expression> arguments)
        {
            codeBuilder.Write(methodName);
            codeBuilder.Write("(");
            bool isFirst = true;
            foreach (var param in arguments)
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
            if (variableInitializer.Initializer != null
                && !(variableInitializer.Initializer is DefaultValueExpression))
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
            if (TryConstantBufferAccess(identifierExpression)) return;
            if (TryThisAccess(identifierExpression)) return;
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

        bool TryConstantBufferAccess(AstNode node)
        {
            var field = node.Annotation<MemberResolveResult>()?.Member as IField;
            if (field == null)
                return false;

            var compilation = UniformManager.Require(field);
            if (compilation == null)
                return false;

            codeBuilder.Write(compilation.Name);
            return true;
        }

        bool TryThisAccess(AstNode node)
        {
            var memberReference = node.Annotation<MemberResolveResult>();
            if (memberReference.TargetResult is ThisResolveResult)
            {
                codeBuilder.Write("_this.");
                codeBuilder.Write(memberReference.Member.Name);
                return true;
            }
            return false;
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

        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            InvocationResolveResult invocationResolveResult = invocationExpression.Annotation<InvocationResolveResult>();
            var method = (IMethod)invocationResolveResult.Member;
            if (SymbolResolver.IsTextureType(method.DeclaringType))
            {
                var target = (invocationExpression.Target as MemberReferenceExpression)?.Target?.Annotation<MemberResolveResult>()?.Member as IField;
                if (target == null)
                    throw new Exception("Textures can only be used directly."); //TODO: textures being passed around?
                var result = UniformManager.Require(target) as TextureCompilation;
                if (result == null)
                    throw new Exception("Textures must be marked as uniform.");
                result.InvokeSampleCall(codeBuilder, invocationExpression, this);
                return;
            }
            if (method.Substitution != TypeParameterSubstitution.Identity)
                throw new NotImplementedException("Generic methods are not implemented.");

            var compilation = MethodManager.Require(method);
            codeBuilder.Write(compilation.Name);
            codeBuilder.Write("(");
            bool isFirst = true;
            if (!method.IsStatic)
            {
                if (invocationExpression.Target is MemberReferenceExpression memberReference)
                    AddParameter(memberReference.Target);
                else if (invocationResolveResult.TargetResult is ILVariableResolveResult)
                {
                    isFirst = false;
                    codeBuilder.Write("_this");
                }
                else
                    throw new Exception();
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
            //if (unrollNextLoop)
            //{
            //    codeBuilder.WriteLine("[unroll]");
            //    unrollNextLoop = false;
            //}

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
            }
            else if (assignmentExpression.Left is MemberReferenceExpression memberReference)
            {
                VisitMember(memberReference);
                codeBuilder.Write(OperatorHelper.GetOperatorString(assignmentExpression.Operator));
                assignmentExpression.Right.AcceptVisitor(this);
                void VisitMember(MemberReferenceExpression memberReferenceExpression)
                {
                    if (memberReference.Target is MemberReferenceExpression childMemberReference)
                        VisitMember(childMemberReference);
                    else if (TryLocalAccess(memberReference.Target)) { }
                    else
                        throw new NotImplementedException();

                    var memberResolveResult = memberReferenceExpression.Annotation<MemberResolveResult>();
                    if (memberResolveResult.Member is IField field)
                    {
                        codeBuilder.Write(".");
                        codeBuilder.Write(SymbolResolver.TryResolve(field)?.Name ?? field.Name);
                    }
                    else throw new NotImplementedException();
                }
            }
            else if (assignmentExpression.Left is IdentifierExpression identifier
                && identifier.Annotation<MemberResolveResult>() is MemberResolveResult result
                && result != null
                && result.TargetResult is ThisResolveResult thisResolve
                && result.Member is IField field)
            {
                codeBuilder.Write("_this.");
                codeBuilder.Write(SymbolResolver.TryResolve(field)?.Name ?? field.Name);
            }
            else throw new NotImplementedException();
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
            codeBuilder.Write(TypeManager.GetTypeString(resolveResult.Type));
            codeBuilder.Write("(");
            castExpression.Expression.AcceptVisitor(this);
            codeBuilder.Write(")");
        }

        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            var memberResolveResult = memberReferenceExpression.Annotation<MemberResolveResult>();
            if (memberResolveResult.Member is IField field)
            {
                memberReferenceExpression.Target.AcceptVisitor(this);
                codeBuilder.Write(".");
                codeBuilder.Write(SymbolResolver.TryResolve(field)?.Name ?? field.Name);
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
        public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        {
            var ctor = (IMethod)objectCreateExpression.Annotation<InvocationResolveResult>().Member;
            var itype = ctor.DeclaringType;
            var type = TypeManager.GetTargetType(itype);
            if (type.IsPrimitive)
                WriteCall(type.Name, objectCreateExpression.Arguments);
            else
            {
                var member = (IMethod)objectCreateExpression.Annotation<InvocationResolveResult>().Member;
                var method = MethodManager.Require(member);
                WriteCall(method.Name, objectCreateExpression.Arguments);
            }
        }

        //public override void VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression)
        //{
        //    codeBuilder.Write("(");
        //    codeBuilder.Write(TypeManager.GetTypeString(defaultValueExpression.Annotation<ConstantResolveResult>().Type));
        //    codeBuilder.Write(")");
        //    codeBuilder.Write("0");
        //}
    }
}
