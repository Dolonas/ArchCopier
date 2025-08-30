namespace ProductComposition.Infrastructure.Services;

public class RegistryKeyParameters
{
    internal string SubKey { get; set; }
    internal string Value { get; set; }

    public RegistryKeyParameters(string subKey, string value)
    {
        SubKey = subKey;
        Value = value;  
    }
}