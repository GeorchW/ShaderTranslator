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
using System.Text;

namespace ShaderTranslator
{
    class ILSpyManager
    {
        DecompilerSettings settings;

        Dictionary<string, CSharpDecompiler> AssemblyDecompilers = new Dictionary<string, CSharpDecompiler>();

        public ILSpyManager()
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
        }

        public CSharpDecompiler GetDecompiler(Assembly assembly)
        {
            return GetDecompiler(assembly.GuessLocation());
        }

        private CSharpDecompiler GetDecompiler(string location)
        {
            if (!AssemblyDecompilers.TryGetValue(location, out var decompiler))
            {
                var peFile = new PEFile(location);
                var typeSystem = new DecompilerTypeSystem(peFile, new LocalAssemblyResolver());
                decompiler = AssemblyDecompilers[location] = new CSharpDecompiler(typeSystem, settings);
                decompiler.ILTransforms.Remove(decompiler.ILTransforms.Where(x => x is HighLevelLoopTransform).Single());
            }
            return decompiler;
        }

        public SyntaxTree GetSyntaxTree(IMethod method)
            => GetDecompiler(method.ParentModule.PEFile.FileName).Decompile(method.MetadataToken);

        public (SyntaxTree, IDecompilerTypeSystem) GetSyntaxTree(MethodInfo method)
        {
            var decompiler = GetDecompiler(method.Module.Assembly);
            return (decompiler.Decompile(method.GetEntityHandle()), decompiler.TypeSystem);
        }
    }
}
