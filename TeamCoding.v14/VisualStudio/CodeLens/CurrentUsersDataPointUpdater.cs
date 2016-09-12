using Microsoft.VisualStudio.CodeSense.Roslyn;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.CodeLens
{
    [Export(typeof(CurrentUsersDataPointUpdater))]
    public class CurrentUsersDataPointUpdater : IDisposable
    {
        // Can't use an import here since this is loaded dynamically it doesn't have access to the main project's MEF exports
        private readonly ITeamCodingPackageProvider TeamCodingPackageProvider = TeamCodingProjectTypeProvider.Get<ITeamCodingPackageProvider>();
        private readonly List<CurrentUsersDataPointViewModel> DataPointModels = new List<CurrentUsersDataPointViewModel>();
        private Dictionary<int[], string> CaretMemberHashCodeToDataPointString = new Dictionary<int[], string>(new IntArrayEqualityComparer());
        private bool disposedValue = false; // To detect redundant calls
        public CurrentUsersDataPointUpdater(): base()
        {
            TeamCodingPackageProvider.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
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

            CaretMemberHashCodeToDataPointString = TeamCodingPackageProvider.RemoteModelChangeManager.GetOpenFiles()
                                              .Where(of => of.CaretPositionInfo != null)
                                              .Select(of => new
                                              {
                                                  CaretMemberHashCodes = of.CaretPositionInfo.SyntaxNodeIds,
                                                  of.IdeUserIdentity.DisplayName
                                              })
                                              .GroupBy(of => of.CaretMemberHashCodes, new IntArrayEqualityComparer())
                                              .ToDictionary(g => g.Key, g => "Current coders: " + string.Join(", ", g.Select(of => of.DisplayName).Distinct()));

            if (!oldCaretMemberHashCodeToDataPointString.DictionaryEqual(CaretMemberHashCodeToDataPointString))
            {
                foreach (var dataPointModel in DataPointModels)
                {
                    if (!dataPointModel.IsDisposed)
                    {
                        if (dataPointModel.RefreshCommand.CanExecute(null))
                        {
                            dataPointModel.RefreshCommand.Execute(null);
                        }
                    }
                }
                DataPointModels.RemoveAll(dvm => dvm.IsDisposed);
            }
        }
        public Task<string> GetTextForDataPoint(ICodeElementDescriptor codeElementDescriptor)
        { // TODO: Get code lens working in VS15
            foreach (var caret in CaretMemberHashCodeToDataPointString.Keys)
            {
                var node = codeElementDescriptor.SyntaxNode;

                // Find the first node that we start the node chain from (any node that is tracked; a class or member declaration etc)
                var syntaxNodeChain = node.AncestorsAndSelf().ToArray();
                var trackedLeafNodes = syntaxNodeChain.Where(n => n.IsTrackedLeafNode()).Reverse().ToArray();
                
                var foundMatch = false;
                for (int i = 0; i < trackedLeafNodes.Length; i++)
                {
                    var matchedLeafNode = trackedLeafNodes[i];
                    var caretMatchedHashIndex = Array.LastIndexOf(caret, matchedLeafNode.GetValueBasedHashCode());

                    if (caretMatchedHashIndex == -1)
                    {
                        foundMatch = false;
                        continue;
                    }
                    
                    // Now walk up the tree from the matching one, and up the method hashes ensuring we match all the way up
                    var nodeancestorhashes = matchedLeafNode.AncestorsAndSelf().Select(a => a.GetValueBasedHashCode());
                    if (nodeancestorhashes.SequenceEqual(caret.Take(caretMatchedHashIndex + 1).Reverse()))
                    {
                        foundMatch = true;
                    }
                    else
                    {
                        foundMatch = false;
                    }
                }

                if (foundMatch)
                {
                    return Task.FromResult(CaretMemberHashCodeToDataPointString[caret]);
                }
            }
            return Task.FromResult<string>(null);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TeamCodingPackageProvider.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
                }
                disposedValue = true;
            }
        }
        public void Dispose() { Dispose(true); }
    }
}
