using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.CodeSense.Roslyn;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.CodeLens
{
    [Export(typeof(CurrentUsersDataPointUpdater))]
    public class CurrentUsersDataPointUpdater : IDisposable
    {
        private readonly List<CurrentUsersDataPointViewModel> DataPointModels = new List<CurrentUsersDataPointViewModel>();
        private Dictionary<Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier, string> CaretMemberHashCodeToDataPointString = new Dictionary<Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier, string>();
        private readonly Dictionary<SyntaxNode, Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier> NodeToHash = new Dictionary<SyntaxNode, Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier>(); // TODO: Consolidate SyntaxNode->NodeId to a class
        private bool disposedValue = false; // To detect redundant calls
        public CurrentUsersDataPointUpdater(): base()
        {
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
        }
        public void AddDataPointModel(CurrentUsersDataPointViewModel dataPointModel)
        {
            DataPointModels.Add(dataPointModel);
        }
        public void RemoveDataPointModel(CurrentUsersDataPointViewModel dataPointModel)
        {
            DataPointModels.Remove(dataPointModel);
        }
        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            var oldCaretMemberHashCodeToDataPointString = CaretMemberHashCodeToDataPointString;

            CaretMemberHashCodeToDataPointString = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                                    .Where(of => of.CaretPositionInfo != null)
                                                                    .SelectMany(of => of.CaretPositionInfo.SyntaxNodeIds.Select(c => new { CaretMemberHashCode = c, of.IdeUserIdentity.DisplayName }))
                                                                    .GroupBy(of => of.CaretMemberHashCode)
                                                                    .ToDictionary(g => g.Key, g => "Current coders: " + string.Join(", ", g.Select(of => of.DisplayName).Distinct()));

            if (!oldCaretMemberHashCodeToDataPointString.DictionaryEqual(CaretMemberHashCodeToDataPointString))
            {
                foreach (var dataPointModel in DataPointModels)
                {
                    if (dataPointModel.IsDisposed)
                    {
                        NodeToHash.Remove(((CurrentUsersDataPoint)dataPointModel.DataPoint).CodeElementDescriptor.SyntaxNode);
                    }
                    else
                    {
                        dataPointModel.RefreshModel();
                    }
                }
                DataPointModels.RemoveAll(dvm => dvm.IsDisposed);
            }
        }
        public Task<string> GetTextForDataPoint(ICodeElementDescriptor codeElementDescriptor)
        {
            Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier hash;
            if (!NodeToHash.ContainsKey(codeElementDescriptor.SyntaxNode))
            {
                hash = codeElementDescriptor.SyntaxNode.GetTreePositionHashCode();
                NodeToHash.Add(codeElementDescriptor.SyntaxNode, hash);
            }
            else
            {
                hash = NodeToHash[codeElementDescriptor.SyntaxNode];
            }
            if (CaretMemberHashCodeToDataPointString.ContainsKey(hash))
            {
                return Task.FromResult(CaretMemberHashCodeToDataPointString[hash]);
            }
            return Task.FromResult<string>(null);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
                }
                disposedValue = true;
            }
        }
        public void Dispose() { Dispose(true); }
    }
}
