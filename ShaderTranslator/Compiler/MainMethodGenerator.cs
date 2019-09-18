using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShaderTranslator.Syntax;

namespace ShaderTranslator
{
    static class MainMethodGenerator
    {
        public static string GenerateMainMethod(MethodCompilation entryPoint, IndentedStringBuilder codeBuilder, ShaderType shaderType)
        {
            int outputLocationIndex = 1;
            int inputLocationIndex = 1;

            if (entryPoint.Parameters.Count == 1
                && entryPoint.Parameters[0].Type is StructTargetType structInput)
            {
                foreach (var field in structInput.Fields)
                {
                    string fieldName = field.SemanticName ?? field.Name;
                    if (!fieldName.StartsWith("gl_"))
                    {
                        codeBuilder.WriteLine($"layout(location = {inputLocationIndex++}) in {field.Type.Name} {fieldName};");
                    }
                }
            }
            else
            {
                foreach (var param in entryPoint.Parameters)
                {
                    codeBuilder.WriteLine($"layout(location = {inputLocationIndex++}) in {param.Type.Name} {param.Name};");
                }
            }
            string? singleReturnName = null;
            if (entryPoint.ReturnType is StructTargetType returnStruct)
            {
                foreach (var field in returnStruct.Fields)
                {
                    string name = field.SemanticName ?? field.Name;
                    if (!name.StartsWith("gl_"))
                    {
                        codeBuilder.WriteLine($"layout(location = {outputLocationIndex++}) out {field.Type.Name} {field.Name};");
                    }
                }
            }
            else
            {
                if (entryPoint.Method.GetReturnTypeAttributes().TryGetAttribute(typeof(GlslAttribute), out var attribute))
                {
                    singleReturnName = (string)attribute.FixedArguments[0].Value;
                    if (!singleReturnName.StartsWith("gl_"))
                    {
                        codeBuilder.Write($"layout(location = {outputLocationIndex++}) out {entryPoint.ReturnType.Name} {singleReturnName};");
                    }
                }
                else
                {
                    bool generateVariable;
                    (singleReturnName, generateVariable) = shaderType switch
                    {
                        ShaderType.VertexShader => ("gl_Position", false),
                        ShaderType.FragmentShader => ("gl_FragColor", false),
                        _ => throw new NotImplementedException()
                    };
                    if (generateVariable)
                    {
                        codeBuilder.Write($"layout(location = {outputLocationIndex++}) out {entryPoint.ReturnType.Name} {singleReturnName};");
                    }
                }
            }

            codeBuilder.WriteLine("void main()");
            codeBuilder.WriteLine("{");
            codeBuilder.IncreaseIndent();


            string callEntryPoint;
            if (entryPoint.Parameters.Count == 1
                && entryPoint.Parameters[0].Type is StructTargetType structInput2)
            {
                codeBuilder.WriteLine($"{structInput2.Name} inputStruct = {structInput2.Name}(");
                codeBuilder.IncreaseIndent();

                bool isFirst = true;
                foreach (var field in structInput2.Fields)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        codeBuilder.Write(",\n");
                    string name = field.SemanticName ?? field.Name;
                    codeBuilder.Write(name);
                }
                codeBuilder.WriteLine();
                codeBuilder.DecreaseIndent();
                codeBuilder.WriteLine(");");
                callEntryPoint = $"{entryPoint.Name}(inputStruct)";
            }
            else
            {
                callEntryPoint = $"{entryPoint.Name}({string.Join(",", entryPoint.Parameters.Select(p => p.Name))})";
            }
            if (singleReturnName != null)
            {
                codeBuilder.WriteLine($"{singleReturnName} = {callEntryPoint};");
            }
            else
            {
                codeBuilder.WriteLine($"{entryPoint.ReturnType.Name} result = {callEntryPoint};");
                foreach (var field in ((StructTargetType)entryPoint.ReturnType).Fields)
                {
                    codeBuilder.Write($"{field.SemanticName ?? field.Name} = result.{field.Name};");
                }
            }
            codeBuilder.DecreaseIndent();
            codeBuilder.WriteLine("}");

            return codeBuilder.ToString();
        }
    }
}
