using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Device = SharpDX.Direct3D11.Device;

namespace ShaderTranslator.Demo
{
    public static class ShaderCompiler
    {
        static CompileEngine compileEngine = new CompileEngine(SymbolResolver.Default);
        public static (VertexShader, InputLayout, int stride) CompileVertexShader(Delegate shaderMethod, Device device)
        {
            throw new NotImplementedException();
            //var semanticsGenerator = new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.VertexShader);
            //var result = compileEngine.Compile(shaderMethod.Target, shaderMethod.Method, semanticsGenerator);
            //var compilation = SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "vs_5_0");
            //var shader = new VertexShader(device, compilation.Bytecode.Data);
            //InputElement[] elements = GetInputElements(result);
            //int stride = elements.Sum(ie => ie.Format.SizeOfInBytes());
            //var inputLayout = new InputLayout(device, compilation.Bytecode.Data, elements);
            //return (shader, inputLayout, stride);
        }

        private static InputElement[] GetInputElements(ShaderCompilation shader)
        {
            throw new NotImplementedException();
            //List<InputElement> inputElements = new List<InputElement>();
            //foreach (var parameter in shader.EntryPoint.Parameters)
            //{
            //    if (parameter.Semantics != null)
            //    {
            //        var match = Regex.Match(parameter.Semantics, "([a-zA-Z]*)(\\d?)");
            //        if (match.Success)
            //        {
            //            string name = match.Groups[1].Value;
            //            if (!int.TryParse(match.Groups[2].Value, out int index))
            //                index = 0;
            //            if (!(parameter.Type is PrimitiveTargetType primitive))
            //                throw new Exception();
            //            PrimitiveType primitiveType = primitive.PrimitiveType;
            //            inputElements.Add(new InputElement(name, index, ToDxgiFormat(primitiveType), 0));
            //            continue;
            //        }
            //    }
            //    throw new Exception();
            //}

            //return inputElements.ToArray();
        }

        public static PixelShader CompilePixelShader(Delegate shaderMethod, Device device)
        {
            throw new NotImplementedException();
            //var semanticsGenerator = new SemanticsGenerator(InputSemanticsSettings.TexCoord, OutputSemanticsSettings.PixelShader);
            //var result = compileEngine.Compile(shaderMethod.Target, shaderMethod.Method, semanticsGenerator);
            //var compilation = SharpDX.D3DCompiler.ShaderBytecode.Compile(result.Code, result.EntryPoint.Name, "ps_5_0");
            //var shader = new PixelShader(device, compilation.Bytecode.Data);
            //return shader;
        }

        private static Format ToDxgiFormat(PrimitiveType primitiveType)
            => primitiveType.ComponentType switch
            {
                ComponentType.Float => primitiveType.Length switch
                {
                    VectorLength.Scalar => Format.R32_Float,
                    VectorLength.Vector2 => Format.R32G32_Float,
                    VectorLength.Vector3 => Format.R32G32B32_Float,
                    VectorLength.Vector4 => Format.R32G32B32A32_Float,
                    _ => throw new NotImplementedException()
                },
                ComponentType.Int => primitiveType.Length switch
                {
                    VectorLength.Scalar => Format.R32_SInt,
                    VectorLength.Vector2 => Format.R32G32_SInt,
                    VectorLength.Vector3 => Format.R32G32B32_SInt,
                    VectorLength.Vector4 => Format.R32G32B32A32_SInt,
                    _ => throw new NotImplementedException()
                },
                _ => throw new NotImplementedException()
            };
    }
}
