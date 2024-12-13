using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public class SetAssociativeMapping : IMappingPolicy
{
    private CacheLine[] CacheLines { get; }
    private CacheLine[][] Sets;
    private IWritePolicy WritePolicy { get; }
    private IWritePolicy WriteOnMiss { get; }
    private Memory Memory { get; }
    private IReplacementPolicy ReplacementPolicy { get; }
    private Statistics.Statistics Statistics { get; }
    private int NumSets { get; }

    public SetAssociativeMapping(
        CacheLine[] cacheLines, 
        IWritePolicy writePolicy,
        IWritePolicy writeOnMiss,
        Memory memory,
        IReplacementPolicy replacementPolicy,
        Statistics.Statistics statistics,
        int numSets,
        int nWays)
    {
        CacheLines = cacheLines;
        WritePolicy = writePolicy;
        WriteOnMiss = writeOnMiss;
        Memory = memory;
        ReplacementPolicy = replacementPolicy;
        Statistics = statistics;
        NumSets = numSets;
        
        Sets = new CacheLine[NumSets][];
        for (var i = 0; i < NumSets; i++)
        {
            Sets[i] = new CacheLine[nWays];
            for (var j = 0; j < nWays; j++)
            {
                Sets[i][j] = CacheLines[i * nWays + j];
            }
        }
    }
    public CacheLine Read(Instruction instruction)
    {
        var setIndex = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);

        var set = Sets[setIndex];

        foreach (var cacheLine in set)
        {
            if (cacheLine.IsValid && cacheLine.Tag == tag)
            {
                ReplacementPolicy.UpdateCacheLine(cacheLine);
                Statistics.IncrementHitCount();
                return cacheLine;
            }
        }
        
        Statistics.IncrementMissCount();
        var replaceableLine = CacheLines.FirstOrDefault(line => !line.IsValid) 
                              ?? ReplacementPolicy.GetReplacementLine(set);
        if (replaceableLine.IsDirty)
        {
            Memory.SetBlock(address, replaceableLine.Data);
        }

        replaceableLine.Tag = tag;
        replaceableLine.IsValid = true;
        replaceableLine.Data = Memory.GetBlock(address);
        replaceableLine.IsDirty = false;

        ReplacementPolicy.UpdateCacheLine(replaceableLine);

        return replaceableLine;
    }

    public bool Write(Instruction instruction)
    {
        var setIndex = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);
        
        var set = Sets[setIndex];
        var cacheLine = set.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (cacheLine != null)
        {
            WritePolicy.Write(instruction, address, cacheLine, Memory);
            ReplacementPolicy.UpdateCacheLine(cacheLine);
            Statistics.IncrementHitCount();
            return true;
        }
        
        cacheLine = set.FirstOrDefault(line => !line.IsValid)
                    ?? ReplacementPolicy.GetReplacementLine(set);

        if (cacheLine.IsDirty)
        {
            Memory.SetBlock(address, cacheLine.Data);
            cacheLine.IsDirty = false;
        }
        
        WriteOnMiss.Write(instruction, address, cacheLine, Memory);
        
        cacheLine.Tag = tag;
        cacheLine.IsValid = true;
        cacheLine.IsDirty = true;
        ReplacementPolicy.UpdateCacheLine(cacheLine);
        Statistics.IncrementMissCount();
        return false;
    }
}