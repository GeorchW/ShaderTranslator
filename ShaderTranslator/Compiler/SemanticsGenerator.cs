namespace ShaderTranslator
{
    public enum InputSemanticsSettings
    {
        None,
        TexCoord
    }
    public enum OutputSemanticsSettings
    {
        None,
        SvTarget,
        Texcoord
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
                OutputSemanticsSettings.SvTarget => "SV_Target",
                OutputSemanticsSettings.Texcoord => "Texcoord",
                _ => null
            };
            if (semanticsBase == null)
                return;
            if (method.ReturnType.Semantics == null)
            {
                if (method.ReturnType.Type.IsPrimitive)
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
