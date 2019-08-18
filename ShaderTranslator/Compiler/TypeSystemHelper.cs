using System;
using ICSharpCode.Decompiler.TypeSystem;
using System.Reflection;
using System.Linq;
using System.Reflection.Metadata;

namespace ShaderTranslator
{
    static class TypeSystemHelper
    {
        public static Type ToReflectionType(this IType type) => Type.GetType(type.ReflectionName);
        public static MethodInfo ToReflectionMethod(this IMethod method)
        {
            var type = method.DeclaringType.ToReflectionType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            int handle = method.GetMetadataTokenAsInt();
            // This could use some caching.
            return methods.Where(m => m.MetadataToken == handle).Single();
        }

        public static unsafe int GetMetadataTokenAsInt(this IMember member)
        {
            var metadataToken = member.MetadataToken;
            int handle = *(int*)&metadataToken;
            return handle;
        }

        public static unsafe EntityHandle GetEntityHandle(this MemberInfo member)
        {
            // Hacky as fuck. As soon as the System.Reflection.Metadata API 
            // is married to System.Reflection, we should remove this.
            int token = member.MetadataToken;
            return *(EntityHandle*)&token;
        }
    }
}
