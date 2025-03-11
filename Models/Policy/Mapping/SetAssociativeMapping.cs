using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public class SetAssociativeMapping : IMappingPolicy
{
    public CacheLine[][] CacheLines { get; set; }
    private IWritePolicy WritePolicy { get; set; }
    private IWritePolicy WriteOnMiss { get; set; }
    public Memory Memory { get; set; }
    private IReplacementPolicy ReplacementPolicy { get; set; }
    public Statistics.Statistics Statistics { get; set; }
    private int LinesPerSet { get; set; }
    private int NumSets { get; set; }
    public SetAssociativeMapping(
        CacheLine[] cacheLines, 
        IWritePolicy writePolicy,
        IWritePolicy writeOnMiss,
        Memory memory,
        IReplacementPolicy replacementPolicy,
        Statistics.Statistics statistics,
        int linesPerSet,
        int numSets)
    {
        CacheLines = new CacheLine[numSets][];
        for (int i = 0; i < numSets; i++)
        {
            CacheLines[i] = new CacheLine[linesPerSet];
            for (int j = 0; j < linesPerSet; j++)
            {
                CacheLines[i][j] = new CacheLine();
            }
        }
        
        WritePolicy = writePolicy;
        WriteOnMiss = writeOnMiss;
        Memory = memory;
        ReplacementPolicy = replacementPolicy;
        Statistics = statistics;
        LinesPerSet = linesPerSet;
        NumSets = numSets;
    }
    
    public SetAssociativeMapping(){}
    public bool Read(Instruction instruction)
    {
        // Parse fields from the instruction
        var index = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);

        // Get the set corresponding to the index
        var set = CacheLines[index]; // This is a CacheLine[] representing the set

        // Check for a hit in the set
        var hitLine = set.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (hitLine != null)
        {
            Console.WriteLine(instruction);
            ReplacementPolicy.UpdateCacheLine(hitLine); // Notify replacement policy of access
            Statistics.IncrementHitCount();
            return true; // Return the cache line on a hit
        }

        // Miss: Find a line to replace
        var replaceableLine = set.FirstOrDefault(line => !line.IsValid) 
                              ?? ReplacementPolicy.GetReplacementLine(set);

        // Write back the evicted line if it's dirty
        if (replaceableLine.IsDirty)
        {
            var writeBackAddress = (replaceableLine.Tag << instruction.Index.Length) | index;
            Memory.SetBlock(writeBackAddress, replaceableLine.Data);
        }

        // Load the new block from memory
        var data = Memory.GetBlock(address);
        replaceableLine.Data = data;
        replaceableLine.Tag = tag;
        replaceableLine.IsValid = true;
        replaceableLine.IsDirty = false; // New data is clean

        // Notify the replacement policy of the updated line
        ReplacementPolicy.UpdateCacheLine(replaceableLine);
        Statistics.IncrementMissCount();
        return false; // Return the updated cache line
    }
    
    public bool Write(Instruction instruction, byte[] data)
    {
        var index = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);

        var set = CacheLines[index];

        var cacheLine = set.FirstOrDefault(line => line.IsValid && line.Tag == tag);
        if (cacheLine != null)
        {
            WritePolicy.Write(instruction, address, cacheLine, Memory, data);
            ReplacementPolicy.UpdateCacheLine(cacheLine);
            Statistics.IncrementHitCount();
            return true;
        }

        cacheLine = set.FirstOrDefault(line => !line.IsValid) 
                    ?? ReplacementPolicy.GetReplacementLine(set);

        if (cacheLine.IsDirty)
        {
            var writeBackAddress = (cacheLine.Tag << instruction.Index.Length) | index;
            Memory.SetBlock(writeBackAddress, cacheLine.Data);
            cacheLine.IsDirty = false;
        }

        WriteOnMiss.Write(instruction, address, cacheLine, Memory, data);

        cacheLine.Tag = tag;
        cacheLine.IsValid = true;
        cacheLine.IsDirty = true;
        ReplacementPolicy.UpdateCacheLine(cacheLine);
        Statistics.IncrementMissCount();
        return false;
    }

    public void SetWritePolicy(IWritePolicy writePolicy)
    {
        WritePolicy = writePolicy;
    }

    public void SetWriteOnMissPolicy(IWritePolicy writeOnMiss)
    {
        WriteOnMiss = writeOnMiss;
    }

    public void SetReplacementPolicy(IReplacementPolicy replacementPolicy)
    {
        ReplacementPolicy = replacementPolicy;
    }
}