using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Interfaces.Documents
{
    public interface ICaretAdornmentDataProvider
    {
        Task<IEnumerable<CaretAdornmentData>> GetCaretAdornmentData(ITextSnapshot textSnapshot, int[] caretMemberHashCodes);
    }
}
