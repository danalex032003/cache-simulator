using CacheSimulatorWebApp.Models.Policy.Mapping;
using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models;

public class Cache
{
    public CacheLine[] CacheLines { get; set; }
    public IMappingPolicy MappingPolicy { get; set; }
    public IReplacementPolicy ReplacementPolicy { get; set; }
    public IWritePolicy WritePolicy { get; set; }
    public WriteAround WriteAround { get; set; }
    public int CacheSize { get; set; }
    public int AddressWidth { get; set; }
    public int BlockSize { get; set; }
    public Memory Memory { get; set; }

    public Cache(IMappingPolicy mappingPolicy, IReplacementPolicy replacementPolicy, IWritePolicy writePolicy, int cacheSize, int addressWidth, int blockSize, Memory memory)
    {
        MappingPolicy = mappingPolicy;
        ReplacementPolicy = replacementPolicy;
        WritePolicy = writePolicy;
        CacheSize = cacheSize;
        AddressWidth = addressWidth;
        BlockSize = blockSize;
        Memory = memory;

        CacheLines = new CacheLine[CacheSize];
        for (var i = 0; i < CacheSize; i++)
        {
            CacheLines[i] = new CacheLine();
        }
    }

    /// <summary>
    /// Read a block of data from memory to cache
    /// </summary>
    /// <param name="address">Memory address for data to be read</param>
    /// <returns>Block of memory (CacheLine) read</returns>
    public CacheLine Read(Instruction instruction)
    {
        // load
        return MappingPolicy.Read(instruction);
    }
    
    /// <summary>
    /// Write a block of data to cache
    /// </summary>
    /// <param name="address">Memory address for data to be written</param>
    /// <param name="data">Data to write to cache</param>
    public void Write(int address, byte[] data)
    {
        // store
        // MappingPolicy.Write(address, data);
    }

    /// <summary>
    /// Load a block of memory in the cache
    /// </summary>
    /// <param name="address">Memory address for data to load in the cache</param>
    /// <param name="data">Block of data from memory to load in the cache</param>
    public void Load(int address, byte[] data)
    {
        
    }
}