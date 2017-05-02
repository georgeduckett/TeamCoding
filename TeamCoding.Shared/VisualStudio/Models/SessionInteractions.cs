using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models
{
    [Flags]
    public enum SessionInteractions { Decline = 0, Invite = 1, Accept = 2, InSession = 4 }
}
