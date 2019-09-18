using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShaderTranslator
{
    static class MainMethodGenerator
    {
        public static string GenerateMainMethod(MethodCompilation entryPoint, IndentedStringBuilder codeBuilder)
        {
            foreach (var param in entryPoint.Parameters)
            {
                codeBuilder.WriteLine($"in {param.Type.Name} {param.Semantics};");
            }
            bool singleOutput = entryPoint.ReturnType.Semantics != null;
            string returnType = entryPoint.ReturnType.Type.Name;
            if (singleOutput)
            {
                codeBuilder.WriteLine($"out {returnType} {entryPoint.ReturnType.Semantics};");
            }
            else
            {
                foreach (var field in ((StructTargetType)entryPoint.ReturnType.Type).Fields)
                {
                    codeBuilder.WriteLine($"out {field.Type.Name} {field.Name};");
                }
            }
            codeBuilder.WriteLine("void main()");
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();
            string callEntryPoint = $"{entryPoint.Name}({string.Join(",", entryPoint.Parameters.Select(p => p.Semantics))});";
            if (singleOutput)
            {
                codeBuilder.WriteLine($"{entryPoint.ReturnType.Semantics} = {callEntryPoint}");
            }
            else
            {
                codeBuilder.WriteLine($"{returnType} result = {callEntryPoint}");
            }
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");

            return codeBuilder.ToString();
        }
    }
}
