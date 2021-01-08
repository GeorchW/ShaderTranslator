using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

namespace ShaderTranslator
{
    class ILSpyManager
    {
        DecompilerSettings settings;

        Dictionary<AssemblyName, CSharpDecompiler> AssemblyDecompilers = new();

        public PEFileResolver PEFileResolver { get; }

        public ILSpyManager(PEFileResolver peFileResolver)
        {
            settings = new DecompilerSettings(LanguageVersion.CSharp1);
            settings.ThrowOnAssemblyResolveErrors = false;
            settings.ArrayInitializers = false;
            settings.AssumeArrayLengthFitsIntoInt32 = true;
            settings.AutomaticProperties = true;
            settings.MakeAssignmentExpressions = false;
            settings.ShowXmlDocumentation = false;
            settings.SwitchStatementOnString = false;
            settings.UsingDeclarations = false;
            settings.ForEachStatement = false;
            settings.IntroduceIncrementAndDecrement = false;
            
            PEFileResolver = peFileResolver;
        }

        public CSharpDecompiler GetDecompiler(Assembly assembly)
        {
            return GetDecompiler(assembly.GetName());
        }

        private CSharpDecompiler GetDecompiler(AssemblyName assemblyName)
        {
            if (!AssemblyDecompilers.TryGetValue(assemblyName, out var decompiler))
            {
                var peFile = PEFileResolver.Resolve(assemblyName);
                var typeSystem = new DecompilerTypeSystem(peFile, PEFileResolver);
                decompiler = AssemblyDecompilers[assemblyName] = new CSharpDecompiler(typeSystem, settings);
                decompiler.ILTransforms.Remove(decompiler.ILTransforms.Where(x => x is HighLevelLoopTransform).Single());
            }
            return decompiler;
        }

        public SyntaxTree GetSyntaxTree(IMethod method)
            => GetDecompiler(new AssemblyName(method.ParentModule.FullAssemblyName)).Decompile(method.MetadataToken);

        public (SyntaxTree, IDecompilerTypeSystem) GetSyntaxTree(MethodInfo method)
        {
            var decompiler = GetDecompiler(method.Module.Assembly);
            return (decompiler.Decompile(method.GetEntityHandle()), decompiler.TypeSystem);
        }
    }
}
