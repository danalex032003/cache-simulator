namespace CacheSimulatorWebApp.Models;

public class LRU : IReplacementPolicy
{
    public CacheLine GetReplacementLine(CacheLine[] cacheLines)
    {
        return cacheLines.Where(line => line.IsValid).OrderBy(line => line.LastModifiedDateTime).FirstOrDefault();
    }

    public void UpdateCacheLine(CacheLine cacheLine)
    { 
        cacheLine.LastModifiedDateTime = DateTime.Now;
    }
}