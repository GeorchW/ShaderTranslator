using System;
using ICSharpCode.Decompiler.TypeSystem;
using System.Reflection;
using System.Linq;
using System.Reflection.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ShaderTranslator.Syntax;

namespace ShaderTranslator
{
    static class TypeSystemHelper
    {
        public static Type? ToReflectionType(this IType type)
        {
            var result = Type.GetType(type.ReflectionName);
            if (result != null) 
                return result;
            if (type is IEntity ie)
            {
                var asmName = new AssemblyName(ie.ParentModule.FullAssemblyName);
                var asm = Assembly.Load(asmName);
                return asm.GetType(type.FullName);
            }
            return null;
        }
        public static MethodInfo? ToReflectionMethod(this IMethod method)
        {
            var type = method.DeclaringType.ToReflectionType();
            var methods = type?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            int handle = method.GetMetadataTokenAsInt();
            // This could use some caching.
            return methods?.Where(m => m.MetadataToken == handle)?.Single();
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

        public static bool TryGetAttribute(this IEnumerable<IAttribute> attributes, Type attributeType, [NotNullWhen(true)] out IAttribute? attribute)
        {
            string attrName = attributeType.FullName!; //isn't null for attributes
            foreach (var attr in attributes)
            {
                if (attr.AttributeType.FullName == attrName)
                {
                    attribute = attr;
                    return true;
                }
            }
            attribute = null;
            return false;
        }
        [return: NotNullIfNotNull("defaultName")]
        public static string? GetName(this IEnumerable<IAttribute> attributes, string? defaultName)
        {
            if (attributes.TryGetAttribute(typeof(GlslAttribute), out var attribute))
            {
                defaultName = (string)(attribute.FixedArguments[0].Value ?? throw new Exception("GLSL attribute parameters may not be null."));
            }
            return defaultName;
        }

        public static string GuessLocation(this Assembly assembly)
        {
            string location = assembly.Location;
            if (string.IsNullOrWhiteSpace(location))
            {
                location = assembly.ManifestModule.ScopeName;
            }

            return location;
        }
    }
}
