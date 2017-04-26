using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.VisualStudio.Models;

namespace TeamCoding.VisualStudio.Models
{
    public partial class LocalIDEModel
    {
        private readonly ConcurrentDictionary<string, SessionInteractions> _SharedSessionInteractedUsers = new ConcurrentDictionary<string, SessionInteractions>();
        /// <summary>
        /// Gets the users currently invited to a shared session (with a bool indicating acceptance)
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, SessionInteractions> SharedSessionInteractedUsers() => _SharedSessionInteractedUsers;
        /// <summary>
        /// Shares a session with a user (they have to accept) with this IDE instance as the host
        /// </summary>
        /// <param name="userId"></param>
        public void ShareSessionWithUser(string userId)
        {
            _SharedSessionInteractedUsers.AddOrUpdate(userId, SessionInteractions.Invite, (s, b) => b | SessionInteractions.Invite);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Cancels sharing a session with a user
        /// </summary>
        /// <param name="userId"></param>
        public void CancelShareSessionWithUser(string userId)
        {
            _SharedSessionInteractedUsers.TryRemove(userId, out _);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Accepts a session invite from a user
        /// </summary>
        /// <param name="userId"></param>
        public void AcceptSessionInvite(string userId)
        {
            _SharedSessionInteractedUsers.AddOrUpdate(userId, SessionInteractions.Accept, (s, b) => b | SessionInteractions.Accept);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        public void DeclineSessionInvite(string userId)
        {
            _SharedSessionInteractedUsers.AddOrUpdate(userId, SessionInteractions.Decline, (s, b) => SessionInteractions.Decline);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        public void LeaveSession()
        {
            // TODO: Do something to leave the session
        }
    }
}
