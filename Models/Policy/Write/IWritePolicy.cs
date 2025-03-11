namespace CacheSimulatorWebApp.Models.Policy.Write;

/// <summary>
/// Represents an interface for cache writing policies in a cache memory simulation.
/// Defines the behavior of data writing operations between the cache and main memory.
/// </summary>
/// <remarks>
/// Different writing policies, such as Write-Through, Write-Back, Write-On-Allocate and Write-Around
/// implement this interface to specify how and when data is written to the cache and main memory. 
/// These policies affect data consistency, performance, and cache management in the simulation.
/// </remarks>
public interface IWritePolicy
{
    /// <summary>
    /// Defines the method to handle writing data to the cache and memory 
    /// according to a specific write policy.
    /// </summary>
    /// <param name="instruction">
    /// The <see cref="Instruction"/> that contains the data to be written and relevant addressing information.
    /// </param>
    /// <param name="address">
    /// The memory address where the data is to be written.
    /// This address may be used to determine the location in the cache and/or memory.
    /// </param>
    /// <param name="cacheLine">
    /// The <see cref="CacheLine"/> object representing the cache line where data may be written 
    /// according to the policy.
    /// </param>
    /// <param name="memory">
    /// The <see cref="Memory"/> object representing the main memory, which may be written to 
    /// depending on the write policy (e.g., for Write-Through or Write-Back policies).
    /// </param>
    void Write(Instruction instruction, int address, CacheLine cacheLine, Memory memory, byte[] data);
}