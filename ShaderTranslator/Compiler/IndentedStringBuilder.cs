using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTranslator
{
    class IndentedStringBuilder
    {
        int tabs = 0;
        bool tabsWritten = false;
        StringBuilder result = new StringBuilder();
        public void IncreaseIndent() => tabs++;
        public void DecreaseIndent() => tabs--;
        public void Write(string str)
        {
            if(tabs == 0)
            {
                result.Append(str);
                return;
            }
            if (!tabsWritten)
            {
                WriteTabs();
            }
            int start = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    result.Append(str.AsSpan().Slice(start, i - start + 1));
                    WriteTabs();
                    start = i + 1;
                }
            }
            result.Append(str.AsSpan().Slice(start, str.Length - start));
        }
        public void Write(IFormattable formattable) => Write(formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture));

        private void WriteTabs()
        {
            for (int i = 0; i < tabs; i++)
            {
                result.Append("    ");
            }
            tabsWritten = true;
        }

        public void WriteLine(string str)
        {
            Write(str);
            WriteLine();
        }
        public void WriteLine()
        {
            result.AppendLine();
            tabsWritten = false;
        }
        public override string ToString() => result.ToString();
    }
}
