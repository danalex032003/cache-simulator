namespace CacheSimulatorWebApp.Statistics;

public class Statistics
{
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public int OperationCount { get; set; }

    public Statistics()
    {
        HitCount = 0;
        MissCount = 0;
        OperationCount = 0;
    }

    public void IncrementHitCount()
    {
        HitCount++;
        OperationCount++;
    }

    public void IncrementMissCount()
    {
        MissCount++;
        OperationCount++;
    }

    public float GetHitRate()
    {
        return (float) HitCount / OperationCount;
    }
}