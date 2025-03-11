using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models.Policy.Mapping;

/// <summary>
/// Implements a direct-mapping cache policy, where each block in memory is mapped 
/// to a single cache line. This class handles cache operations with a specific 
/// write policy and write-on-miss behavior.
/// </summary>
/// <param name="cacheLines">
/// An array of <see cref="CacheLine"/> objects representing the cache lines 
/// in the direct-mapped cache.
/// </param>
/// <param name="writePolicy">
/// The <see cref="IWritePolicy"/> used for handling writes to cache lines on cache hits.
/// </param>
/// <param name="writeOnMiss">The <see cref="IWritePolicy"/> used for handling writes to cache and memory 
/// when a cache miss occurs.
/// </param>
/// <param name="memory">
/// The <see cref="Memory"/> object representing main memory from which blocks 
/// are loaded into the cache on cache misses.
/// </param>
public class DirectMapping : IMappingPolicy
{
    public CacheLine[] CacheLines { get; set; }
    private IWritePolicy WritePolicy { get; set; }
    private IWritePolicy WriteOnMissPolicy { get; set; }
    public Memory Memory { get; set; }
    public Statistics.Statistics Statistics { get; set; }

    public int x = 10;
    
    public DirectMapping(
        CacheLine[] cacheLines,
        IWritePolicy writePolicy,
        IWritePolicy writeOnMissPolicy,
        Memory memory,
        Statistics.Statistics statistics)
    {
        CacheLines = cacheLines;
        WritePolicy = writePolicy;
        WriteOnMissPolicy = writeOnMissPolicy;
        Memory = memory;
        Statistics = statistics;
    }
    public DirectMapping() { }
    
    /// <summary>
    /// Reads data from the cache at a specified index and tag. If the data is not found 
    /// (cache miss), it loads the data from memory, updates the cache line, and handles 
    /// any necessary write-back operations.
    /// </summary>
    /// <param name="instruction">
    /// The <see cref="Instruction"/> containing the index, tag, and memory address 
    /// used to locate and read data from the cache.
    /// </param>
    /// <returns>
    /// The <see cref="CacheLine"/> containing the data retrieved from the cache or memory.
    /// </returns>
    public bool Read(Instruction instruction)
    {
        var index = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);
        var cacheLine = CacheLines[index];

        if (cacheLine.IsValid && cacheLine.Tag == tag) 
        {
            Statistics.IncrementHitCount();
            return true; // hit
        }
        
        // miss
        var data = Memory.GetBlock(address);
        cacheLine.IsValid = true;
        cacheLine.Data = data;
        cacheLine.Tag = tag;
        
        if (cacheLine.IsDirty)
        {
            //Memory.SetBlock(address, cacheLine.Data);
            cacheLine.IsDirty = false;
        }
        Statistics.IncrementMissCount();
        return false;
    }
    
    /// <summary>
    /// Attempts to write data to the cache using the specified write policy. 
    /// Handles cache hits, writes on miss, and write-back operations if the cache line is dirty.
    /// </summary>
    /// <param name="instruction">
    /// The <see cref="Instruction"/> containing the index, tag, and memory address 
    /// information for the write operation.
    /// </param>
    /// <returns>
    /// <c>true</c> if the write operation is a cache hit; otherwise, <c>false</c>.
    /// </returns>
    public bool Write(Instruction instruction, byte[] data)
    {
        var index = Convert.ToInt32(instruction.Index, 2);
        var tag = Convert.ToInt32(instruction.Tag, 2);
        var address = Convert.ToInt32(instruction.AddressInMemory, 2);
        var cacheLine = CacheLines[index];
        if (cacheLine.Tag == tag)
        {
            // hit
            WritePolicy.Write(instruction, address, cacheLine, Memory, data);
            Statistics.IncrementHitCount();
            return true;
        }
        
        if (cacheLine.IsDirty)
        {
            Memory.SetBlock(address, cacheLine.Data);
            cacheLine.IsDirty = false;
        }
        // miss
        WriteOnMissPolicy.Write(instruction, address, cacheLine, Memory, data);
        Statistics.IncrementMissCount();
        return false;
    }
    public Memory GetMemory()
    {
        return Memory;
    }

    public void SetWritePolicy(IWritePolicy writePolicy)
    {
        WritePolicy = writePolicy;
    }

    public void SetWriteOnMissPolicy(IWritePolicy writeOnMissPolicy)
    {
        WriteOnMissPolicy = writeOnMissPolicy;
    }

    public IWritePolicy GetWritePolicy()
    {
        return WritePolicy;
    }

    public IWritePolicy GetWriteOnMissPolicy()
    {
        return WriteOnMissPolicy;
    }
}