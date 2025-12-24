using System.Diagnostics.CodeAnalysis;

namespace org.kdtnet.CAAPI.Engine;

[ExcludeFromCodeCoverage]
public static class ApplicationLocus
{
    public static class Administration
    {
        public static class User
        {
            public static readonly string Create =$"{nameof(Administration)}.{nameof(User)}.{nameof(Create)}";
            public static readonly string Update =$"{nameof(Administration)}.{nameof(User)}.{nameof(Update)}";
            public static readonly string Delete =$"{nameof(Administration)}.{nameof(User)}.{nameof(Delete)}";
            public static readonly string Fetch =$"{nameof(Administration)}.{nameof(User)}.{nameof(Fetch)}";
            public static readonly string CheckPrivilege =$"{nameof(Administration)}.{nameof(User)}.{nameof(CheckPrivilege)}";
        }

        public static class Role
        {
            public static readonly string Create =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Create)}";
            public static readonly string Update =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Update)}";
            public static readonly string Delete =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Delete)}";
            public static readonly string Fetch =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Fetch)}";
            public static readonly string Grant =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Grant)}";
            public static readonly string Revoke =$"{nameof(Administration)}.{nameof(Role)}.{nameof(Revoke)}";
        }
        
        public static class UserRole
        {
            public static readonly string Create =$"{nameof(Administration)}.{nameof(UserRole)}.{nameof(Create)}";
            public static readonly string Update =$"{nameof(Administration)}.{nameof(UserRole)}.{nameof(Update)}";
            public static readonly string Delete =$"{nameof(Administration)}.{nameof(UserRole)}.{nameof(Delete)}";
            public static readonly string Fetch =$"{nameof(Administration)}.{nameof(UserRole)}.{nameof(Fetch)}";
        }
    }

    public static class Certificates
    {
        //ApplicationLocus.Certificates.Certificate.Create
        public static class Certificate
        {
            public static readonly string Create =$"{nameof(Administration)}.{nameof(Certificate)}.{nameof(Create)}";
            public static readonly string Fetch =$"{nameof(Administration)}.{nameof(Certificate)}.{nameof(Fetch)}";
        }
    }
    
}