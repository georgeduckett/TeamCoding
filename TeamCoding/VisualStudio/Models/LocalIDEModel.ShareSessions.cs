using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models
{
    public partial class LocalIDEModel
    {
        private readonly ConcurrentDictionary<string, bool> _SharedSessionInvitedUsers = new ConcurrentDictionary<string, bool>();
        /// <summary>
        /// Gets the users currently invited to a shared session (with a bool indicating acceptance)
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, bool> SharedSessionInvitedUsers() => _SharedSessionInvitedUsers;
        /// <summary>
        /// Shares a session with a user (they have to accept) with this IDE instance as the host
        /// </summary>
        /// <param name="userId"></param>
        public void ShareSessionWithUser(string userId)
        {
            _SharedSessionInvitedUsers.TryAdd(userId, false);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Cancels sharing a session with a user
        /// </summary>
        /// <param name="userId"></param>
        public void CancelShareSessionWithUser(string userId)
        {
            _SharedSessionInvitedUsers.TryRemove(userId, out _);
            ModelChangedInternal?.Invoke(this, EventArgs.Empty);
        }
    }
}
