using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.Configuration;

public class ApplicationConfiguration : IValidateable
{
    public required ApplicationConfigurationEngine Engine { get; init; }
    public required ApplicationConfigurationLogging Logging { get; init; }
    public required ApplicationConfigurationDataStore DataStore { get; init; }
    
    public void Validate()
    {
        ValidationHelper.AssertObjectNotNull(Engine);
        ValidationHelper.AssertObjectNotNull(Logging);
        ValidationHelper.AssertObjectNotNull(DataStore);
        
        Engine.Validate();
        Logging.Validate();
        DataStore.Validate();
    }
}

public class ApplicationConfigurationEngine : IValidateable
{
    public required ApplicationConfigurationEnginePassphraseMandates PassphraseMandates { get; init; }
    public void Validate()
    {
        ValidationHelper.AssertObjectNotNull(PassphraseMandates);
        
        PassphraseMandates.Validate();
    }
}
public class ApplicationConfigurationEnginePassphraseMandates : IValidateable
{
    public required int MinLength { get; set; }
    public required int MinUpperCase { get; set; }
    public required int MinLowerCase { get; set; }
    public required int MinDigit { get; set; }
    public required int MinSpecial { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertCondition(() => MinLength > 0, "must be greater than zero", nameof(MinLength));
        ValidationHelper.AssertCondition(() => MinUpperCase >= 0, "must be greater than or equal to zero", nameof(MinUpperCase));
        ValidationHelper.AssertCondition(() => MinLowerCase >= 0, "must be greater than or equal to zero", nameof(MinLowerCase));
        ValidationHelper.AssertCondition(() => MinDigit >= 0, "must be greater than or equal to zero", nameof(MinDigit));
        ValidationHelper.AssertCondition(() => MinSpecial >= 0, "must be greater than or equal to zero", nameof(MinSpecial));
    }
}