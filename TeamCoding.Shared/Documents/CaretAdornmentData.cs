using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Interfaces.Documents
{
    public struct CaretAdornmentData
    {
        public readonly int NonWhiteSpaceStart;
        public readonly int SpanStart;
        public readonly int SpanEnd;
        public CaretAdornmentData(int nonWhiteSpaceStart, int spanStart, int spanEnd) { NonWhiteSpaceStart = nonWhiteSpaceStart; SpanStart = spanStart; SpanEnd = spanEnd; }
    }
}
