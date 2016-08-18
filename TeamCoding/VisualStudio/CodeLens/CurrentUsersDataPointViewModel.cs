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
    public class CurrentUsersDataPointViewModel : GlyphDataPointViewModel
    {
        public override bool IsVisible { get { return Data != null && !string.IsNullOrEmpty((string)Data); } }
        public override string AdditionalInformation { get { return "Current users in this area"; } }
        public CurrentUsersDataPointViewModel(ICodeLensDataPoint dataPoint) : base(dataPoint)
        {
            HasDetails = false;
            PropertyChanged += CurrentUsersDataPointViewModel_PropertyChanged;
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
        }
        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            Refresh(); // TODO: Refresh the CodeLens data points much more efficiently
        }
        private void CurrentUsersDataPointViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Descriptor = (string)Data;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PropertyChanged -= CurrentUsersDataPointViewModel_PropertyChanged;
                TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
            }
            base.Dispose(disposing);
        }
    }
}
