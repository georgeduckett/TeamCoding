using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models
{
    public enum SessionInteractions { Decline = 0, Invite = 1, Accept = 2, InviteAndAccept = 3 }
    public static class SessionInteractionsExtensions
    {
        public static bool ContainsInvite(this SessionInteractions interaction) => (interaction & SessionInteractions.Invite) == SessionInteractions.Invite;
        public static bool ContainsAccept(this SessionInteractions interaction) => (interaction & SessionInteractions.Accept) == SessionInteractions.Accept;
    }
}
