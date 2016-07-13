using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio;

namespace TeamCoding
{
    /// <summary>
    /// Responsible for managing external/remote models
    /// </summary>
    public class ExternalModelManager
    {
        private const string ModelSyncFileFormat = "OpenDocs*.bin";
        private readonly List<RemoteIDEModel> Models = new List<RemoteIDEModel>();
        public IEnumerable<RemoteIDEModel> GetExternalModels() => Models;

        public void SyncChanges()
        {
            Models.Clear();
            foreach (var modelSyncFile in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), ModelSyncFileFormat))
            {
                using (var f = File.OpenRead(modelSyncFile))
                {
                    Models.Add(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                }
            }
        }
    }
}
