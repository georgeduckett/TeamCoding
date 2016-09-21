using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.WindowsServicePersister
{
    public class WinServiceClient : IDisposable
    {
        public class MessageReceivedEventArgs : EventArgs
        {
            public byte[] Message;
        }
        public event EventHandler<MessageReceivedEventArgs> MessageReceived; // TODO: Raise the MessageReceived even when we receive bytes
        public void SendModel(RemoteIDEModel model)
        {
            // TODO: Send the model in the WinServiceClient
        }
        public void Dispose()
        {
            // TODO: Close the connection
        }
    }
}
