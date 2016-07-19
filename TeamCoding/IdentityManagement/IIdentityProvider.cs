namespace TeamCoding.IdentityManagement
{
    /// <summary>
    /// An interface that provides a user identity
    /// </summary>
    public interface IIdentityProvider
    {
        UserIdentity GetIdentity();
    }
}
