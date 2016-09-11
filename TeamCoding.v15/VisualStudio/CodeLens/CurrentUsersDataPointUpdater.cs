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
    [Export(typeof(CurrentUsersDataPointV15Updater))]
    public class CurrentUsersDataPointV15Updater : IDisposable
    {
        private readonly List<CurrentUsersDataPointV15ViewModel> DataPointModels = new List<CurrentUsersDataPointV15ViewModel>();
        private Dictionary<int[], string> CaretMemberHashCodeToDataPointString = new Dictionary<int[], string>(new IntArrayEqualityComparer());
        private bool disposedValue = false; // To detect redundant calls
        public CurrentUsersDataPointV15Updater(): base()
        {
            //TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
        }
        public void AddDataPointModel(CurrentUsersDataPointV15ViewModel dataPointModel)
        {
            DataPointModels.Add(dataPointModel);
        }
        public void RemoveDataPointModel(CurrentUsersDataPointV15ViewModel dataPointModel)
        {
            DataPointModels.Remove(dataPointModel);
        }
        private void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            /*var oldCaretMemberHashCodeToDataPointString = CaretMemberHashCodeToDataPointString;

            CaretMemberHashCodeToDataPointString = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
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
                        dataPointModel.RefreshModel();
                    }
                }
                DataPointModels.RemoveAll(dvm => dvm.IsDisposed);
            }*/
        }
        public Task<string> GetTextForDataPoint(ICodeElementDescriptor codeElementDescriptor)
        {
            // TODO: Get CodeLens working again
            /*foreach (var caret in CaretMemberHashCodeToDataPointString.Keys)
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
            }*/
            return Task.FromResult<string>(null);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
                }
                disposedValue = true;
            }
        }
        public void Dispose() { Dispose(true); }
    }
}
