using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Interfaces.Documents
{
    public struct CaretAdornmentData
    {
        public readonly int SpanStart;
        public readonly int SpanEnd;
        public CaretAdornmentData(int spanStart, int spanEnd) { SpanStart = spanStart; SpanEnd = spanEnd; }
    }
}
