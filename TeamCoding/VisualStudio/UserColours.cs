using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeamCoding.IdentityManagement;

namespace TeamCoding.VisualStudio
{
    public static class UserColours
    {
        private static readonly Dictionary<int, SolidColorBrush> UserToBrush = new Dictionary<int, SolidColorBrush>();
        private static readonly Dictionary<int, Pen> UserToPen = new Dictionary<int, Pen>();
        public static Color GetUserColour(UserIdentity user)
        {
            return VisuallyDistinctColours.GetColourFromSeed(user.Id.GetHashCode());
        }
        public static SolidColorBrush GetUserBrush(IUserIdentity user)
        {
            var hash = user.Id.GetHashCode();

            SolidColorBrush brush;
            if(!UserToBrush.TryGetValue(hash, out brush))
            {
                brush = new SolidColorBrush(VisuallyDistinctColours.GetColourFromSeed(hash));
                brush.Freeze();
            }
            return brush;
        }
        public static Pen GetUserPen(IUserIdentity user)
        {
            var hash = user.Id.GetHashCode();

            Pen pen;
            if (!UserToPen.TryGetValue(hash, out pen))
            {
                pen = new Pen(GetUserBrush(user), 1);
                pen.Freeze();
            }
            return pen;
        }
    }
}
