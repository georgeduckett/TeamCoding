using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeamCoding.Extensions;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// A user's identity
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class UserIdentity
    {
        private static Color UIntToColor(uint color)
        { // http://stackoverflow.com/a/4382138
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }
        private static readonly List<Color> KellysMaxContrastSet = new List<Color>
        { // http://stackoverflow.com/a/4382138
            UIntToColor(0xFFFFB300), //Vivid Yellow
            UIntToColor(0xFF803E75), //Strong Purple
            UIntToColor(0xFFFF6800), //Vivid Orange
            UIntToColor(0xFFA6BDD7), //Very Light Blue
            UIntToColor(0xFFC10020), //Vivid Red
            UIntToColor(0xFFCEA262), //Grayish Yellow
            UIntToColor(0xFF817066), //Medium Gray

            //The following will not be good for people with defective color vision
            UIntToColor(0xFF007D34), //Vivid Green
            UIntToColor(0xFFF6768E), //Strong Purplish Pink
            UIntToColor(0xFF00538A), //Strong Blue
            UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
            UIntToColor(0xFF53377A), //Strong Violet
            UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
            UIntToColor(0xFFB32851), //Strong Purplish Red
            UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
            UIntToColor(0xFF7F180D), //Strong Reddish Brown
            UIntToColor(0xFF93AA00), //Vivid Yellowish Green
            UIntToColor(0xFF593315), //Deep Yellowish Brown
            UIntToColor(0xFFF13A13), //Vivid Reddish Orange
            UIntToColor(0xFF232C16), //Dark Olive Green
        };

        [ProtoBuf.ProtoMember(1)]
        public string Id { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public string ImageUrl { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public string DisplayName { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public byte[] ImageBytes { get; set; }
        public static string GetGravatarUrlFromEmail(string email)
        {
            return $"https://www.gravatar.com/avatar/{CalculateMD5Hash(email).ToLower()}?d=404";
        }
        public override string ToString()
        {
            return $"Id: {Id}, DisplayName: {DisplayName}, ImageUrl: {ImageUrl}";
        }
        public Color GetUserColour()
        {
            return KellysMaxContrastSet[Id.GetHashCode() % KellysMaxContrastSet.Count];
        }
        public static async Task<string> GetGravatarDisplayNameFromEmail(string email)
        {
            var result = await TeamCodingPackage.Current.HttpClient.GetAsync($"https://www.gravatar.com/{CalculateMD5Hash(email).ToLower()}.json?d=404").HandleException();
            if (!result.IsSuccessStatusCode) return null;

            var gravatarProfileJsonString = await result.Content.ReadAsStringAsync();
            var gravatarProfileJsonObject = (JObject)JsonConvert.DeserializeObject(gravatarProfileJsonString);
            return gravatarProfileJsonObject?["entry"]?[0]?["displayName"]?.Value<string>();
        }
        private static string CalculateMD5Hash(string input)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
