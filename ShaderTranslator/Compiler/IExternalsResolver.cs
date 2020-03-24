using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator
{
    public enum ResolveType
    {
        Field,
        Method,
        Operator
    }
    public struct ResolveResult
    {
        public ResolveType Type;
        public string Name;
        public string? RequiredCode;

        public ResolveResult(ResolveType type, string name, string? requiredCode = null)
        {
            Type = type;
            Name = name;
            RequiredCode = requiredCode;
        }

        public static ResolveResult Field(string name) => new ResolveResult(ResolveType.Field, name);
        public static ResolveResult Method(string name, string? requiredCode = null) => new ResolveResult(ResolveType.Method, name, requiredCode);
        public static ResolveResult Operator(string name) => new ResolveResult(ResolveType.Operator, name);
    }
    public interface IExternalsResolver
    {
        ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi);
    }

    class SystemMathResolver : IExternalsResolver
    {
        public ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi)
        {
            if (symbol is IMethod method
                && method.IsStatic
                && method.Accessibility == Accessibility.Public
                && (method.DeclaringType.FullName == "System.Math"
                || method.DeclaringType.FullName == "System.MathF"))
            {
                return ResolveResult.Method(method.Name.ToLowerInvariant());
            }
            else return null;
        }
    }

    class VectorComponentResolver : IExternalsResolver
    {
        public ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi)
        {
            if (symbol is IField field
                && mathApi.TryResolve(field.DeclaringType, out var primitiveType)
                && field.Name.Length == 1
                && field.Name[0] >= 'W' && field.Name[0] <= 'Z')
                return ResolveResult.Field(field.Name.ToLowerInvariant());
            return null;
        }
    }

    class VectorLengthResolver : IExternalsResolver
    {
        public ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi)
        {
            if (symbol is IMethod method
                && mathApi.TryResolve(method.DeclaringType, out var thisType)
                && thisType.PrimitiveType.IsVector)
            {
                if (method.Name == "Length")
                    return ResolveResult.Method("length");
                else if (method.Name == "LengthSquared")
                    return ResolveResult.Method("lengthSquared", $"float lengthSquared({thisType.Name} value) {{return dot(value, value);}}");
            }
            return null;
        }
    }

    class VectorOperatorResolver : IExternalsResolver
    {
        Dictionary<string, string> ops = new Dictionary<string, string> {
            { "op_Addition", "+" },
            { "op_Subtraction", "-" },
            { "op_Multiply", "*" },
            { "op_Division", "/" },

            { "op_UnaryNegation", "-" },
        };

        public ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi)
        {
            if (symbol is IMethod method
                && method.IsOperator
                && mathApi.TryResolve(method.DeclaringType, out _))
            {
                if (!ops.TryGetValue(method.Name, out var op))
                    return null;

                if (method.Parameters.Count == 2)
                {
                    var (left, right) = (method.Parameters[0], method.Parameters[1]);

                    if (!mathApi.TryResolve(left.Type, out var leftType))
                        return null;
                    if (!mathApi.TryResolve(right.Type, out var rightType))
                        return null;
                    if (leftType.PrimitiveType.IsVector && rightType.PrimitiveType.IsVector
                        && leftType.PrimitiveType == rightType.PrimitiveType)
                        return ResolveResult.Operator(op);
                    else if (leftType.PrimitiveType.IsScalar || rightType.PrimitiveType.IsScalar)
                        return ResolveResult.Operator(op);
                    else
                        return null;
                }
            }
            return null;
        }
    }

    class VectorMethodResolver : IExternalsResolver
    {
        HashSet<string> simpleMethods = new HashSet<string>() {
            "Normalize",
            "Cross",
            "Dot",
        };
        public ResolveResult? TryResolve(ISymbol symbol, MathApi mathApi)
        {
            if (symbol is IMethod method
                && mathApi.TryResolve(method.DeclaringType, out _))
            {
                if (simpleMethods.Contains(method.Name))
                    return ResolveResult.Method(method.Name.ToLowerInvariant());

                if (!method.Parameters.All(p => mathApi.TryResolve(p.Type, out _)))
                    return null;

                if (method.Name == "Transform")
                {
                    if (method.Parameters.Count == 2 && method.Parameters[1].Type.Name == "Matrix4x4")
                    {
                        if (method.Parameters[0].Type.Name == "Vector3")
                        {
                            if (method.DeclaringType.Name == "Vector3")
                                return ResolveResult.Method("transform", "vec3 transform(vec3 v, mat4x4 m) { return (m * vec4(v, 1)).xyz; }");
                            else if (method.DeclaringType.Name == "Vector4")
                                return ResolveResult.Method("transform", "vec4 transform(vec3 v, mat4x4 m) { return m * vec4(v, 1); }");
                        }
                        else if (method.Parameters[0].Type.Name == "Vector4" && method.DeclaringType.Name == "Vector4")
                            return ResolveResult.Method("transform", "vec4 transform(vec4 v, mat4x4 m) { return m * v; }");
                    }

                    if (method.Parameters.Count == 2 && method.Parameters[0].Type.Name == "Vector4" && method.Parameters[1].Type.Name == "Matrix4x4")
                        return ResolveResult.Method("transform", "vec4 transform(vec4 v, mat4x4 m) { return m * v; }");
                }
                else if (method.Name == "TransformNormal")
                {
                    if (method.Parameters.Count == 2 && method.Parameters[0].Type.Name == "Vector3" && method.Parameters[1].Name == "Matrix4x4")
                        return ResolveResult.Method("transformNormal", "vec3 transformNormal(vec3 v, mat4x4 m) { return m * vec4(v, 0); }");
                }
            }
            return null;
        }
    }
}
