using System;

namespace Common.Data.Linq
{
    class DebugTextWriter : System.IO.TextWriter
    {
        public override void Write(char[] buffer, int index, int count)
        {
            try
            {
                System.Diagnostics.Debug.Write(new String(buffer, index, count));
            }
            catch { }
        }

        public override void Write(string value)
        {
            try
            {
                System.Diagnostics.Debug.Write(value);
            }
            catch { }
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }
}
