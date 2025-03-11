namespace CacheSimulatorWebApp.Util;

public enum WritePolicyEnum
{
    WRITE_BACK,
    WRITE_THROUGH
    
}

public enum WriteOnMissPolicyEnum
{
    WRITE_ON_ALLOCATE,
    WRITE_AROUND
}