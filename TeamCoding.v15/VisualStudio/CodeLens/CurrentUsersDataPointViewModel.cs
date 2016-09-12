using Microsoft.VisualStudio.CodeSense.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.CodeLens
{
    public class CurrentUsersDataPointV15ViewModel : GlyphDataPointViewModel
    {
        private readonly CurrentUsersDataPointV15Updater DataPointUpdater;
        public override string AdditionalInformation => "Current users in this area";
        public CurrentUsersDataPointV15ViewModel(CurrentUsersDataPointV15Updater dataPointUpdater, ICodeLensDataPoint dataPoint) : base(dataPoint)
        {
            DataPointUpdater = dataPointUpdater;
            dataPointUpdater.AddDataPointModel(this);
            HasDetails = false;
            PropertyChanged += CurrentUsersDataPointV15ViewModel_PropertyChanged;
        }
        private void CurrentUsersDataPointV15ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data))
            {
                Descriptor = (string)Data;
            }
        }
        protected override bool? HasDataCore(object dataValue) => (base.HasDataCore(dataValue) ?? false) && !string.IsNullOrEmpty(dataValue as string);
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataPointUpdater.RemoveDataPointModel(this);
                PropertyChanged -= CurrentUsersDataPointV15ViewModel_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}
