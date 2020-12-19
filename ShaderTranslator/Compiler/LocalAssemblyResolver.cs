using System.Runtime.Loader;
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
            if (File.Exists(reference.Name + ".dll"))
                return new PEFile(reference.Name + ".dll", PEStreamOptions.PrefetchEntireImage);
            if (reference.FullName.StartsWith("System.") || reference.FullName.StartsWith("netstandard,"))
            {
                Assembly asm = Assembly.Load(reference.FullName);
                string fileName = asm.GuessLocation();
                return new PEFile(fileName, PEStreamOptions.PrefetchEntireImage);
            }
            Console.WriteLine($"Warning: Did not find '{reference}'.");
            return null;

            // string location;
            // try
            // {
            //     // var alc = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            //     // var asm = alc?.LoadFromAssemblyName(new AssemblyName(reference.FullName)) 
            //     //     ?? Assembly.Load(reference.FullName);
            //     // location = asm.GuessLocation();
            //     // location = Assembly.ReflectionOnlyLoad(reference.FullName).GuessLocation();
            // }
            // catch(FileNotFoundException)
            // {
            //     if(File.Exists(reference.Name + ".dll"))
            //         return new PEFile(reference.Name + ".dll");
            //     Console.WriteLine($"Warning: Did not find '{reference}'.");
            //     return null;
            // }
            // return new PEFile(location);
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            var result = new PEFile(moduleName, PEStreamOptions.PrefetchEntireImage);
            return result;
        }
    }
}