namespace CacheSimulatorWebApp.Models;

public class FIFO : IReplacementPolicy
{
    public CacheLine GetReplacementLine(CacheLine[] cacheLines)
    {
        return cacheLines.Where(line => line.IsValid).OrderBy(line => line.CreatedDateTime).FirstOrDefault();
    }

    public void UpdateCacheLine(CacheLine cacheLine)
    {
        if (!cacheLine.IsValid)
        {
            cacheLine.CreatedDateTime = DateTime.Now;
        }
    }
}