using ICSharpCode.Decompiler.TypeSystem;
using ShaderTranslator.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ShaderTranslator
{
    public class SymbolResolver
    {
        List<IExternalsResolver> resolvers = new List<IExternalsResolver>();
        Dictionary<ISymbol, ResolveResult?> cachedResolutions = new Dictionary<ISymbol, ResolveResult?>();
        HashSet<string> textureTypes = new HashSet<string>();
        MathApi mathApi = new MathApi();
        public static SymbolResolver Default { get; } = CreateDefault();
        public MathApi MathApi { get; } = new MathApi();

        static SymbolResolver CreateDefault()
        {
            var result = new SymbolResolver();
            result.AddExternalsResolver(new SystemMathResolver());
            result.AddExternalsResolver(new VectorComponentResolver());
            result.AddExternalsResolver(new VectorLengthResolver());
            result.AddExternalsResolver(new VectorOperatorResolver());
            result.AddTextureType(typeof(Texture2D).FullName!);
            result.AddTextureType(typeof(TextureCube).FullName!);
            result.MathApi.AddType(typeof(int), ComponentType.Int, VectorLength.Scalar);
            result.MathApi.AddType(typeof(float), ComponentType.Float, VectorLength.Scalar);
            result.MathApi.AddType(typeof(Vector2), ComponentType.Float, VectorLength.Vector2);
            result.MathApi.AddType(typeof(Vector3), ComponentType.Float, VectorLength.Vector3);
            result.MathApi.AddType(typeof(Vector4), ComponentType.Float, VectorLength.Vector4);
            return result;
        }
        public void AddExternalsResolver(IExternalsResolver resolver) => resolvers.Add(resolver);
        public void AddTextureType(string type) => textureTypes.Add(type);
        public ResolveResult? TryResolve(ISymbol symbol)
        {
            if (cachedResolutions.TryGetValue(symbol, out var result))
                return result;
            foreach (var resolver in resolvers)
            {
                result = resolver.TryResolve(symbol, MathApi);
                if (result != null)
                    break;
            }
            cachedResolutions.Add(symbol, result);
            return result;
        }

        public bool IsTextureType(IType type) => textureTypes.Contains(type.FullName);
    }
}
