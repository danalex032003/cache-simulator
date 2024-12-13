namespace CacheSimulatorWebApp.Models;

/// <summary>
/// Represents a memory instruction, parsed into its tag, index, offset, and full memory address. 
/// This class is used in memory systems, particularly in cache simulations, to break down the 
/// instruction's binary representation for cache and memory operations.
/// </summary>
public class Instruction
{
    /// <summary>
    /// Gets the tag of the instruction, calculated as the portion of the instruction 
    /// that remains after subtracting the offset and index sizes.
    /// </summary>
    public string Tag { get; } // log2(memorySize) - offset - index
    /// <summary>
    /// Gets the index of the instruction, which specifies the cache line within 
    /// the cache where the memory block might be located.
    /// </summary>
    public string Index { get; } // log2(cacheSize/blockSize)

    private readonly string _offset; // 2 bits
    /// <summary>
    /// Gets the full memory block composing of tag and index
    /// </summary>
    public string AddressInMemory { get; }
    /// <summary>
    /// Gets the size of the block (cache line)
    /// </summary>
    private int BlockSize { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Instruction"/> class, parsing 
    /// the given instruction integer to extract the tag, index, and offset based on 
    /// the provided cache and memory sizes.
    /// </summary>
    /// <param name="instruction">
    /// The integer representation of the instruction. This value is converted to a 
    /// binary string and parsed to derive the tag, index, and offset components.
    /// </param>
    /// <param name="cacheSize">
    /// The size of the cache, used to determine the number of bits needed for the 
    /// index and offset in the binary representation of the instruction.
    /// </param>
    /// <param name="memorySize">
    /// The size of the memory, used to determine the total number of bits required 
    /// for the binary representation of the instruction.
    /// </param>
    /// <param name="blockSize">
    /// The size of the block of data (cache line), used to divide the memory into
    /// blockSize blocks
    /// </param>
    public Instruction(int instruction, int cacheSize, int memorySize, int blockSize)
    {
        BlockSize = blockSize;
        var instructionSize = (int)Math.Log2(memorySize);
        var binaryInstruction = Convert.ToString(instruction, 2).PadLeft(instructionSize, '0');
        const int offsetSize = 2;
        var indexSize = (int)Math.Log2(cacheSize/BlockSize);
        var tagSize = instructionSize - offsetSize - indexSize;
        _offset = binaryInstruction.Substring(
            instructionSize - offsetSize, offsetSize);
        Index = binaryInstruction.Substring(
            instructionSize - offsetSize - indexSize, indexSize);
        Tag = binaryInstruction.Substring(
            0, tagSize);
        AddressInMemory = Tag + Index;
    }
    
    /// <summary>
    /// Returns a string that represents the current instruction object, including 
    /// its tag, index and offset.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Instruction: [{Tag} {Index} {_offset}]";
    }
}