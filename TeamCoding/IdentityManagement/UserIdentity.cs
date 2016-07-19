using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// A user's identity
    /// </summary>
    [ProtoBuf.ProtoContract]
    public class UserIdentity
    {
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
        public static async Task<string> GetGravatarDisplayNameFromEmail(string email)
        {
            var result = await TeamCodingPackage.Current.HttpClient.GetAsync($"https://www.gravatar.com/{CalculateMD5Hash(email).ToLower()}.json?d=404");
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
