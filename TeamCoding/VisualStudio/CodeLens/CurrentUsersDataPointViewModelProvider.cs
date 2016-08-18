using Microsoft.VisualStudio.CodeSense.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;

namespace TeamCoding.VisualStudio.CodeLens
{
    [DataPointViewModelProvider(typeof(CurrentUsersDataPoint))]
    public class CurrentUsersDataPointViewModelProvider : DataPointViewModelProvider<CurrentUsersDataPointViewModel>
    {
        protected override CurrentUsersDataPointViewModel GetViewModel(ICodeLensDataPoint dataPoint)
        {
            return new CurrentUsersDataPointViewModel(dataPoint);
        }
    }
}
