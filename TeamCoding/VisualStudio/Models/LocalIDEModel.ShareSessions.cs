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
        public class AcceptedSessionEventArgs : EventArgs
        {
            public string UserId { get; set; }
            public AcceptedSessionEventArgs(string userId) => UserId = userId;
        }
        /// <summary>
        /// Occurs when we accept a remote user's shared session invite.
        /// </summary>
        public event EventHandler<AcceptedSessionEventArgs> AcceptedSharedSession;

        private readonly ConcurrentDictionary<string, SessionInteractions> _SharedSessionInteractedUsers = new ConcurrentDictionary<string, SessionInteractions>();
        /// <summary>
        /// Gets the users currently invited to a shared session (with an enum indicating status)
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
            AcceptedSharedSession?.Invoke(this, new AcceptedSessionEventArgs(userId));
        }
        public void DeclineSessionInvite(string userId)
        {
            _SharedSessionInteractedUsers.AddOrUpdate(userId, SessionInteractions.Decline, (s, b) => SessionInteractions.Decline);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Updates the model to indicate that we're currently in a shared session with a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isHost"></param>
        public void MarkInSession(string userId, bool isHost)
        {
            var flag = SessionInteractions.InSession;
            if (isHost)
            {
                flag |= SessionInteractions.Invite;
            }
            else
            {
                flag |= SessionInteractions.Accept;
            }

            _SharedSessionInteractedUsers.AddOrUpdate(userId, flag, (s, b) => flag);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Updates the model to indicate that we're no longer in a shared session with a user
        /// </summary>
        /// <param name="userId"></param>
        public void MarkLeftSession(string userId)
        {
            _SharedSessionInteractedUsers.TryRemove(userId, out _);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
    }
}
