using System;
using System.Security.Permissions;

namespace TeamCoding.CredentialManagement
{
    public class Credential
    {
        public enum CredentialType : uint
        {
            None = 0,
            Generic = 1,
            DomainPassword = 2,
            DomainCertificate = 3,
            DomainVisiblePassword = 4
        }
        private static object LockObject = new object();
        private static SecurityPermission UnmanagedCodePermission;
        public string Target { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        static Credential()
        {
            lock (LockObject)
            {
                UnmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
        }
        public bool Load()
        {
            UnmanagedCodePermission.Demand();

            IntPtr credPointer;

            bool result = NativeMethods.CredRead(Target, CredentialType.Generic, 0, out credPointer);
            if (!result)
            {
                return false;
            }
            using (NativeMethods.CriticalCredentialHandle credentialHandle = new NativeMethods.CriticalCredentialHandle(credPointer))
            {
                LoadInternal(credentialHandle.GetCredential());
            }
            return true;
        }
        internal void LoadInternal(NativeMethods.CREDENTIAL credential)
        {
            Username = credential.UserName;
            Target = credential.TargetName;
            Description = credential.Comment;
        }
    }
}
