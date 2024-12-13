using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public class FullyAssociativeMapping(
    CacheLine[] cacheLines, 
    IWritePolicy writePolicy, 
    IWritePolicy writeOnMiss, 
    Memory memory, 
    IReplacementPolicy replacementPolicy,
    Statistics.Statistics statistics ) : IMappingPolicy
{
    private CacheLine[] CacheLines { get; } = cacheLines;
    private IWritePolicy WritePolicy { get; } = writePolicy;
    private IWritePolicy WriteOnMiss { get; } = writeOnMiss;
    private Memory Memory { get; } = memory;
    private IReplacementPolicy ReplacementPolicy { get; } = replacementPolicy;
    private Statistics.Statistics Statistics { get; } = statistics;
    public CacheLine Read(Instruction instruction)
    {
        var tag = Convert.ToInt32(instruction.AddressInMemory, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);

        var hitLine = CacheLines.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (hitLine != null)
        {
            Console.WriteLine(instruction);
            ReplacementPolicy.UpdateCacheLine(hitLine);
            Statistics.IncrementHitCount();
            return hitLine;
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
        return replaceableLine;
    }

    public bool Write(Instruction instruction)
    {
        var tag = Convert.ToInt32(instruction.AddressInMemory, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);
        
        var cacheLine = CacheLines.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (cacheLine != null)
        {
            WritePolicy.Write(instruction, address, cacheLine, Memory);
            ReplacementPolicy.UpdateCacheLine(cacheLine);
            Statistics.IncrementHitCount();
            return true;
        }
        
        cacheLine = CacheLines.FirstOrDefault(line => !line.IsValid)
                     ?? ReplacementPolicy.GetReplacementLine(CacheLines);

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