using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.Configuration;

namespace org.kdtnet.CAAPI.Common.Utility;

public static class GenericHelper
{
    public static void AssertValidPassphrase(string passphrase, ApplicationConfigurationEnginePassphraseMandates mandates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passphrase);

        if (passphrase.Length < mandates.MinLength)
            throw new ApiBadPassphraseException($"passphrase must be at least {mandates.MinLength} characters in length");
        
        int countUpper = 0,
            countLower=0,
            countDigit=0,
            countSpecial=0;

        for (int i = 0; i < passphrase.Length;i++ )
        {
            if (char.IsUpper(passphrase[i])) countUpper++;
            else if (char.IsLower(passphrase[i])) countLower++;
            else if (char.IsDigit(passphrase[i])) countDigit++;
            else countSpecial++;
        }
        
        if(countUpper < mandates.MinUpperCase)
            throw new ApiBadPassphraseException($"passphrase must have at least {mandates.MinUpperCase} upper-case characters");
        if (countLower < mandates.MinLowerCase)
            throw new ApiBadPassphraseException($"passphrase must have at least {mandates.MinLowerCase} lower-case characters");
        if (countDigit < mandates.MinDigit)
            throw new ApiBadPassphraseException($"passphrase must have at least {mandates.MinDigit} numeric characters");
        if (countSpecial < mandates.MinSpecial)
            throw new ApiBadPassphraseException($"passphrase must have at least {mandates.MinSpecial} special characters");
    }
}