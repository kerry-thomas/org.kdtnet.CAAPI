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
        }

        public static class Role
        {
            
        }
    }
    
}