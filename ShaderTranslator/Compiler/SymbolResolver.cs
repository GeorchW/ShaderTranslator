using ICSharpCode.Decompiler.TypeSystem;
using ShaderTranslator.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace ShaderTranslator
{
    class SymbolResolver
    {
        List<IExternalsResolver> resolvers = new List<IExternalsResolver>();
        Dictionary<INamedElement, string?> cachedResolutions = new Dictionary<INamedElement, string?>();
        HashSet<string> textureTypes = new HashSet<string>();
        public static SymbolResolver Default { get; } = new SymbolResolver()
        {
            resolvers = new List<IExternalsResolver>
            {
                new MathResolver(),
                new SystemNumericsResolver()
            },
            textureTypes = new[]
            {
                typeof(Texture2D),
                typeof(TextureCube)
            }
            .Select(t => t.FullName!)
            .ToHashSet()
        };
        public void AddExternalsResolver(IExternalsResolver resolver) => resolvers.Add(resolver);
        public string? TryResolve(INamedElement symbol)
        {
            if (cachedResolutions.TryGetValue(symbol, out string? result))
                return result;
            foreach (var resolver in resolvers)
            {
                result = resolver.TryResolve(symbol);
                if (result != null)
                    break;
            }
            cachedResolutions.Add(symbol, result);
            return result;
        }

        public bool IsTextureType(IType type) => textureTypes.Contains(type.FullName);
    }
}
