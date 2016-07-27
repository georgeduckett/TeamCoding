﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.CombinedPersister
{
    public class CombinedRemoteModelPersister : IRemoteModelPersister
    {
        private readonly IRemoteModelPersister[] RemoteModelPersisters;
        public event EventHandler RemoteModelReceived
        {
            add
            {
                foreach (var remoteModelPersister in RemoteModelPersisters)
                {
                    remoteModelPersister.RemoteModelReceived += value;
                }
            }
            remove
            {
                foreach (var remoteModelPersister in RemoteModelPersisters)
                {
                    remoteModelPersister.RemoteModelReceived -= value;
                }
            }
        }

        public IEnumerable<SourceControlledDocumentData> GetOpenFiles() => RemoteModelPersisters.SelectMany(rmp => rmp.GetOpenFiles()).GroupBy(scdd => new
        {
            scdd.Repository,
            scdd.RelativePath,
            scdd.IdeUserIdentity.Id,
            scdd.BeingEdited
        }).Select(g => g.OrderByDescending(scdd => scdd.HasFocus).First()).ToArray();
        public CombinedRemoteModelPersister(params IRemoteModelPersister[] remoteModelPersisters)
        {
            RemoteModelPersisters = remoteModelPersisters;
        }
        public void Dispose()
        {
            foreach(var remoteModelPersister in RemoteModelPersisters)
            {
                remoteModelPersister.Dispose();
            }
        }
    }
}