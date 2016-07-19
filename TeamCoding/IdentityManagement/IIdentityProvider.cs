namespace TeamCoding.IdentityManagement
{
    public interface IIdentityProvider
    {
        UserIdentity GetIdentity();
    }
}
