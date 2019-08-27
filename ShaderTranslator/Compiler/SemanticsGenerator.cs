namespace ShaderTranslator
{
    /// <summary>
    /// Controls which input semantics should be generated.
    /// </summary>
    public enum InputSemanticsSettings
    {
        /// <summary>
        /// Does not generate any input semantics.
        /// </summary>
        None,
        /// <summary>
        /// Generates TexCoord semantics for all fields without semantics.
        /// </summary>
        TexCoord
    }
    /// <summary>
    /// Controls which output semantics should be generated.
    /// </summary>
    public enum OutputSemanticsSettings
    {
        /// <summary>
        /// Does not generate any output semantics.
        /// </summary>
        None,
        /// <summary>
        /// Generates TexCoord semantics for all fields without semantics.
        /// If the return type is primitive, it will get SV_Position semantics.
        /// </summary>
        VertexShader,
        /// <summary>
        /// Generates SV_Target semantics for all fields without semantics.
        /// </summary>
        PixelShader,
    }
    public class SemanticsGenerator
    {
        public InputSemanticsSettings InputSemanticsSettings { get; set; }
        public OutputSemanticsSettings OutputSemanticsSettings { get; set; }

        public SemanticsGenerator(
            InputSemanticsSettings inputSemanticsSettings = InputSemanticsSettings.None,
            OutputSemanticsSettings outputSemanticsSettings = OutputSemanticsSettings.None)
        {
            InputSemanticsSettings = inputSemanticsSettings;
            OutputSemanticsSettings = outputSemanticsSettings;
        }

        internal void GenerateSemantics(MethodCompilation method)
        {
            SetInputSemantics(method);
            SetOutputSemantics(method);
        }

        private void SetInputSemantics(MethodCompilation method)
        {
            if (InputSemanticsSettings == InputSemanticsSettings.TexCoord)
            {
                int index = 0;
                foreach (var parameter in method.Parameters)
                {
                    if (parameter.Type.IsPrimitive)
                    {
                        parameter.Semantics = "Texcoord" + index;
                        index++;
                    }
                    if (parameter.Type is StructTargetType str)
                    {
                        str.SetSemantics("Texcoord", ref index);
                    }
                }
            }
        }

        private void SetOutputSemantics(MethodCompilation method)
        {
            string? semanticsBase = OutputSemanticsSettings switch
            {
                OutputSemanticsSettings.PixelShader => "SV_Target",
                OutputSemanticsSettings.VertexShader => "Texcoord",
                _ => null
            };
            if (semanticsBase == null)
                return;
            if (method.ReturnType.Semantics == null)
            {
                if (OutputSemanticsSettings == OutputSemanticsSettings.VertexShader
                    && method.ReturnType.Type.IsPrimitive)
                    method.ReturnType.Semantics = "SV_Position";
                else if (method.ReturnType.Type.IsPrimitive)
                    method.ReturnType.Semantics = semanticsBase + "0";
                else if (method.ReturnType.Type is StructTargetType structTargetType)
                {
                    int index = 0;
                    structTargetType.SetSemantics(semanticsBase, ref index);
                }
            }
        }
    }
}
