using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.CredentialManagement;

namespace TeamCoding.VisualStudio.Identity
{
    public class CachedGitHubIdentityProvider : IIdentityProvider
    {
        private readonly UserIdentity Identity;
        public UserIdentity GetIdentity() => Identity;
        public CachedGitHubIdentityProvider()
        {
            Credential credential = new Credential { Target = "git:https://github.com" };
            credential.Load();

            if (credential.Username == null)
            {
                credential = new Credential { Target = "https://github.com" };
                credential.Load();
            }

            Identity = new UserIdentity()
            {
                DisplayName = credential.Username,
                ImageUrl = $"http://www.gravatar.com/avatar/{CalculateMD5Hash(credential.Username).ToLower()}"
            };
        }
        private string CalculateMD5Hash(string input)
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
