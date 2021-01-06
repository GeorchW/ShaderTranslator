using ICSharpCode.Decompiler.Metadata;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace ShaderTranslator
{
    sealed class LocalAssemblyResolver : IAssemblyResolver
    {
        public LocalAssemblyResolver()
        { }

        public PEFile? Resolve(IAssemblyReference reference)
        {
            string location;
            try
            {
                location = Assembly.Load(reference.FullName).GuessLocation();
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine($"Warning: Did not find '{reference}'.");
                return null;
            }
            return new PEFile(location, PEStreamOptions.PrefetchEntireImage);
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            var result = new PEFile(moduleName);
            return result;
        }
    }
}