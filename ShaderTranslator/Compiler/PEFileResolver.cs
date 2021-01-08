using System.Reflection;
using ICSharpCode.Decompiler.Metadata;
using System.Reflection.PortableExecutable;
using System.IO;
using System;

namespace ShaderTranslator
{
    public abstract class PEFileResolver : IAssemblyResolver
    {
        public abstract PEFile Resolve(AssemblyName assemblyName);
        public abstract PEFile? Resolve(IAssemblyReference reference);
        public abstract PEFile? ResolveModule(PEFile mainModule, string moduleName);

        public static PEFileResolver Default { get; } = new DefaultResolver();

        class DefaultResolver : PEFileResolver
        {
            internal DefaultResolver() { }
            public override PEFile Resolve(AssemblyName assemblyName)
                => new PEFile(Assembly.Load(assemblyName).GuessLocation(), PEStreamOptions.PrefetchEntireImage);

            public override PEFile? Resolve(IAssemblyReference reference)
            {
                string location;
                try
                {
                    location = Assembly.Load(reference.FullName).GuessLocation();
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Warning: Did not find '{reference}'.");
                    return null;
                }
                return new PEFile(location, PEStreamOptions.PrefetchEntireImage);
            }

            public override PEFile? ResolveModule(PEFile mainModule, string moduleName)
            {
                var result = new PEFile(moduleName, PEStreamOptions.PrefetchEntireImage);
                return result;
            }
        }
    }
}
