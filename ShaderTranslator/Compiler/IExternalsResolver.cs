using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator
{
    interface IExternalsResolver
    {
        string? TryResolve(INamedElement symbol);
    }

    class MathResolver : IExternalsResolver
    {
        public string? TryResolve(INamedElement symbol)
        {
            if (symbol is IMethod method
                && method.IsStatic
                && method.Accessibility == Accessibility.Public
                && (method.DeclaringType.FullName == "System.Math"
                || method.DeclaringType.FullName == "System.MathF"))
            {
                return method.Name.ToLowerInvariant();
            }
            else return null;
        }
    }

    class SystemNumericsResolver : IExternalsResolver
    {
        Dictionary<string, string> typeTranslations = new Dictionary<string, string>();
        public SystemNumericsResolver()
        {
            void AddTranslation(Type type, string name) => typeTranslations.Add(type.FullName, name);
            AddTranslation(typeof(System.Numerics.Vector2), "float2");
            AddTranslation(typeof(System.Numerics.Vector3), "float3");
            AddTranslation(typeof(System.Numerics.Vector4), "float4");
            AddTranslation(typeof(System.Numerics.Matrix3x2), "matrix3x2");
            AddTranslation(typeof(System.Numerics.Matrix4x4), "matrix");
        }
        public string? TryResolve(INamedElement symbol)
        {
            if (symbol is IType type
                && typeTranslations.TryGetValue(type.FullName, out var value))
                return value;
            if (symbol is IField field
                && typeTranslations.ContainsKey(field.DeclaringType.FullName)
                && field.Name.Length == 1
                && field.Name[0] >= 'W' && field.Name[0] <= 'Z')
                return field.Name.ToLowerInvariant();
            return null;
        }
    }
}
