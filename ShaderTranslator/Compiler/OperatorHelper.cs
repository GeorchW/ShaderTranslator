using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator
{
    class OperatorHelper
    {
        public static string GetOperatorString(BinaryOperatorType op) => op switch
        {
            BinaryOperatorType.BitwiseAnd => "&",
            BinaryOperatorType.BitwiseOr => "|",
            BinaryOperatorType.ConditionalAnd => "&&",
            BinaryOperatorType.ConditionalOr => "||",
            BinaryOperatorType.ExclusiveOr => "^",
            BinaryOperatorType.GreaterThan => ">",
            BinaryOperatorType.GreaterThanOrEqual => ">=",
            BinaryOperatorType.Equality => "==",
            BinaryOperatorType.InEquality => "!=",
            BinaryOperatorType.LessThan => "<",
            BinaryOperatorType.LessThanOrEqual => "<=",
            BinaryOperatorType.Add => "+",
            BinaryOperatorType.Subtract => "-",
            BinaryOperatorType.Multiply => "*",
            BinaryOperatorType.Divide => "/",
            BinaryOperatorType.Modulus => "%",
            BinaryOperatorType.ShiftLeft => "<<",
            BinaryOperatorType.ShiftRight => ">>",
            _ => throw new NotSupportedException(),
        };

        public static string GetOperatorString(UnaryOperatorType op) => op switch
        {
            UnaryOperatorType.Not => "!",
            UnaryOperatorType.BitNot => "~",
            UnaryOperatorType.Minus => "-",
            UnaryOperatorType.Plus => "+",
            UnaryOperatorType.Increment => "++",
            UnaryOperatorType.Decrement => "--",
            UnaryOperatorType.PostIncrement => "++",
            UnaryOperatorType.PostDecrement => "--",
            _ => throw new NotSupportedException()
        };
        public static string GetOperatorString(AssignmentOperatorType op) => op switch
        {
            AssignmentOperatorType.Assign => "=",
            AssignmentOperatorType.Add => "+=",
            AssignmentOperatorType.Subtract => "-=",
            AssignmentOperatorType.Multiply => "*=",
            AssignmentOperatorType.Divide => "/=",
            AssignmentOperatorType.Modulus => "%=",
            AssignmentOperatorType.ShiftLeft => "<<=",
            AssignmentOperatorType.ShiftRight => ">>=",
            AssignmentOperatorType.BitwiseAnd => "&=",
            AssignmentOperatorType.BitwiseOr => "|=",
            AssignmentOperatorType.ExclusiveOr => "^=",
            //AssignmentOperatorType.Any => "="
            _ => throw new NotSupportedException()
        };
    }
}
