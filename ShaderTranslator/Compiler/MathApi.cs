using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ShaderTranslator
{
    public class MathApi
    {
        Dictionary<string, PrimitiveTargetType> types = new Dictionary<string, PrimitiveTargetType>();
        public void AddType(Type type, ComponentType componentType, VectorLength length)
            => types.Add(type.FullName ?? throw new ArgumentException("Type must have a name.", nameof(type)),
                new PrimitiveTargetType(new PrimitiveType(componentType, length)));
        internal bool TryResolve(IType type, [NotNullWhen(true)] out PrimitiveTargetType? result)
            => types.TryGetValue(type.FullName, out result);
    }
    public enum ComponentType
    {
        Float,
        Int
    }
    public enum VectorLength
    {
        Scalar,
        Vector2, Vector3, Vector4,
        Matrix
    }
    public static class VectorEnumExtensions
    {
        public static bool IsVector(this VectorLength length) => length switch
        {
            VectorLength.Vector2 => true,
            VectorLength.Vector3 => true,
            VectorLength.Vector4 => true,
            _ => false
        };
        public static bool IsScalar(this VectorLength length)
            => length == VectorLength.Scalar;
    }
    public readonly struct PrimitiveType : IEquatable<PrimitiveType>
    {
        public readonly ComponentType ComponentType;
        public readonly VectorLength Length;

        public bool IsVector => Length.IsVector();
        public bool IsScalar => Length.IsScalar();

        public PrimitiveType(ComponentType componentType, VectorLength length)
        {
            ComponentType = componentType;
            Length = length;
        }

        public override string ToString()
        {
            string prefix = ComponentType switch
            {
                ComponentType.Float => "float",
                ComponentType.Int => "int",
                _ => throw new InvalidOperationException()
            };
            string postfix = Length switch
            {
                VectorLength.Scalar => "",
                VectorLength.Vector2 => "2",
                VectorLength.Vector3 => "3",
                VectorLength.Vector4 => "4",
                VectorLength.Matrix => "4x4",
                _ => throw new InvalidOperationException()
            };
            return prefix + postfix;
        }

        public override bool Equals(object? obj) => obj is PrimitiveType type && Equals(type);
        public bool Equals([AllowNull] PrimitiveType other) => ComponentType == other.ComponentType && Length == other.Length;
        public override int GetHashCode() => HashCode.Combine(ComponentType, Length);

        public static bool operator ==(PrimitiveType left, PrimitiveType right) => left.Equals(right);
        public static bool operator !=(PrimitiveType left, PrimitiveType right) => !(left == right);
    }
}
