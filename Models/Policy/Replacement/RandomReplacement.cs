namespace CacheSimulatorWebApp.Models;

public class RandomReplacement : IReplacementPolicy
{
    private readonly Random _random = new Random();
    
    public CacheLine GetReplacementLine(CacheLine[] cacheLines)
    {
        var validLines = cacheLines.Where(line => line.IsValid).ToArray();
        return validLines[_random.Next(validLines.Length)];
    }

    public void UpdateCacheLine(CacheLine cacheLine)
    {
        //Nothing
    }
}