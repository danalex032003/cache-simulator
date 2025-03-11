using System.Text;

namespace CacheSimulatorWebApp.Models;

/// <summary>
/// Represents a simulated memory component for use in a cache simulation. 
/// Stores a fixed-size array of data and provides functionality to read and write 
/// data blocks aligned by a specified block size.
/// </summary>
/// <param name="size">
/// Represents the size of the memory, multiple of 2
/// </param>
/// <param name="blockSize">
/// Represents the size of the memory block
/// </param>
/// <example>
/// The following example demonstrates how to create a memory instance and retrieve a block:
/// <code>
/// var memory = new Memory(1024, 16);
/// var block = memory.GetBlock(32);
/// </code>
/// </example>

public class Memory
{
    public int Size { get; set;  }
    
    /// <summary>
    /// Gets the block size in bytes, which is the size of each data block within the memory.
    /// </summary>
    public int BlockSize { get; set; }
    private static readonly Random Random = new();
    public byte[] Data { get; set; }

    public Memory(int size, int blockSize)
    {
        Size = size;
        BlockSize = blockSize;
        Data = Enumerable.Range(0, size).Select(_ => (byte)Random.Next(0, 255)).ToArray();
    }
    
    public Memory(){}
    /// <summary>
    /// Retrieves a block of data from memory starting at the specified address, 
    /// aligned to the nearest lower multiple of <see cref="BlockSize"/>.
    /// </summary>
    /// <param name="address">
    /// The starting address in memory from which the block will be retrieved.
    /// This address is aligned to the nearest lower multiple of <see cref="BlockSize"/>.
    /// </param>
    /// <returns>
    /// A <see cref="byte"/> array containing the block of data retrieved from memory. 
    /// The length of this array matches <see cref="BlockSize"/>.
    /// </returns>
    public byte[] GetBlock(int address)
    {
        var start = address - address % BlockSize;
        var block = Data.Skip(start).Take(BlockSize).ToArray();
        return block;
    }

    /// <summary>
    /// Writes a block of data to memory at the specified address, aligning the address 
    /// to the start of the memory block defined by the <see cref="BlockSize"/>.
    /// </summary>
    /// <param name="address">The starting address in memory where the block will be written.
    /// This address is aligned to the nearest multiple of <see cref="BlockSize"/></param>
    /// <param name="block">The data block to be written to memory. The size of this block should match
    /// the <see cref="BlockSize"/> to ensure correct alignment and memory storage.</param>
    /// <remarks>
    /// This method aligns the provided <paramref name="address"/> to the nearest 
    /// lower multiple of <see cref="BlockSize"/> to ensure consistent block alignment 
    /// within memory. The <paramref name="block"/> data is then copied into the 
    /// <c>Data</c> array starting at this aligned address.
    /// </remarks>
    public void SetBlock(int address, byte[] block) 
    {
        var start = address - address % BlockSize;
        Array.Copy(block, 0, Data, start, BlockSize);
    }
    
    /// <summary>
    /// Returns a string that represents the current memory object, including 
    /// its size, block size, and data content.
    /// </summary>
    /// <returns>String including its size, block size, and data content.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Memory [Size={Size}, BlockSize={BlockSize}]");

        for (var i = 0; i < Data.Length; i += BlockSize)
        {
            // Take a chunk of BlockSize bytes
            var chunk = Data.Skip(i).Take(BlockSize);
            // Convert each byte to a hex string and join them with hyphens
            var chunkString = string.Join("-", chunk.Select(b => b.ToString("X2")));
            sb.AppendLine($"[{i:D4}]: {chunkString}");
        }

        return sb.ToString();
    }
}