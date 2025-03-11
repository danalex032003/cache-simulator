using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public class FullyAssociativeMapping : IMappingPolicy
{
    public CacheLine[] CacheLines { get; set; }
    private IWritePolicy WritePolicy { get; set; }
    private IWritePolicy WriteOnMiss { get; set; }
    public Memory Memory { get; set; }
    private IReplacementPolicy ReplacementPolicy { get; set; }
    public Statistics.Statistics Statistics { get; set; }

    public FullyAssociativeMapping(
        CacheLine[] cacheLines,
        IWritePolicy writePolicy,
        IWritePolicy writeOnMissPolicy,
        Memory memory,
        IReplacementPolicy replacementPolicy,
        Statistics.Statistics statistics)
    {
        CacheLines = cacheLines;
        WritePolicy = writePolicy;
        WriteOnMiss = writeOnMissPolicy;
        Memory = memory;
        ReplacementPolicy = replacementPolicy;
        Statistics = statistics;
    }
    public FullyAssociativeMapping(){}
    public bool Read(Instruction instruction)
    {
        var tag = Convert.ToInt32(instruction.AddressInMemory, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);

        var hitLine = CacheLines.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (hitLine != null)
        {
            Console.WriteLine(instruction);
            ReplacementPolicy.UpdateCacheLine(hitLine);
            Statistics.IncrementHitCount();
            return true;
        }
        
        var replaceableLine = CacheLines.FirstOrDefault(line => !line.IsValid) 
                              ?? ReplacementPolicy.GetReplacementLine(CacheLines);

        if (replaceableLine.IsDirty)
        {
            Memory.SetBlock(address, replaceableLine.Data);
        }
        
        var data = Memory.GetBlock(address);
        replaceableLine.Data = data;
        replaceableLine.Tag = tag;
        replaceableLine.IsValid = true;
        replaceableLine.IsDirty = false;
        
        ReplacementPolicy.UpdateCacheLine(replaceableLine);
        Statistics.IncrementMissCount();
        return false;
    }

    public bool Write(Instruction instruction, byte[] data)
    {
        var tag = Convert.ToInt32(instruction.AddressInMemory, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);
        
        var cacheLine = CacheLines.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (cacheLine != null)
        {
            WritePolicy.Write(instruction, address, cacheLine, Memory, data);
            ReplacementPolicy.UpdateCacheLine(cacheLine);
            Statistics.IncrementHitCount();
            return true;
        }
        
        cacheLine = CacheLines.FirstOrDefault(line => !line.IsValid)
                     ?? ReplacementPolicy.GetReplacementLine(CacheLines);

        if (cacheLine.IsDirty)
        {
            //Memory.SetBlock(address, cacheLine.Data);
            cacheLine.IsDirty = false;
        }
        
        WriteOnMiss.Write(instruction, address, cacheLine, Memory, data);
        
        ReplacementPolicy.UpdateCacheLine(cacheLine);
        Statistics.IncrementMissCount();
        return false;
    }

    public void SetWritePolicy(IWritePolicy writePolicy)
    {
        WritePolicy = writePolicy;
    }

    public void SetWriteOnMissPolicy(IWritePolicy writeOnMissPolicy)
    {
        WriteOnMiss = writeOnMissPolicy;
    }

    public void SetReplacementPolicy(IReplacementPolicy replacementPolicy)
    {
        ReplacementPolicy = replacementPolicy;
    }
}