using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TeamCoding.IdentityManagement
{
    public interface IUserIdentity
    {
        string Id { get; set; }
        string ImageUrl { get; set; }
        string DisplayName { get; set; }
        byte[] ImageBytes { get; set; }
        Color GetUserColour();
    }
}
