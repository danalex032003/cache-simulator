namespace CacheSimulatorWebApp.Models;

public interface IReplacementPolicy
{
    CacheLine GetReplacementLine(CacheLine[] cacheLines);
    void UpdateCacheLine(CacheLine cacheLine);
}