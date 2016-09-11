using Microsoft.VisualStudio.CodeSense.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using System.ComponentModel.Composition;

namespace TeamCoding.VisualStudio.CodeLens
{
    [DataPointViewModelProvider(typeof(CurrentUsersDataPointV15))]
    public class CurrentUsersDataPointV15ViewModelProvider : DataPointViewModelProvider<CurrentUsersDataPointV15ViewModel>
    {
        [Import]
        private readonly CurrentUsersDataPointV15Updater DataPointUpdater = null;
        protected override CurrentUsersDataPointV15ViewModel GetViewModel(ICodeLensDataPoint dataPoint)
        {
            var dataPointModel = new CurrentUsersDataPointV15ViewModel(DataPointUpdater, dataPoint);
            return dataPointModel;
        }
    }
}
