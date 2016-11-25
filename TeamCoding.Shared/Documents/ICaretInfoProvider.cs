using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.Interfaces.Documents
{
    public interface ICaretInfoProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapshotPoint">Must be a Microsoft.VisualStudio.Text.SnapshotPoint</param>
        /// <returns></returns>
        Task<DocumentRepoMetaData.CaretInfo> GetCaretInfoAsync(SnapshotPoint snapshotPoint);
    }
}
