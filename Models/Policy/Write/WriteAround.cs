using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models;

public class WriteAround : IWritePolicy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="address"></param>
    /// <param name="cacheLine"></param>
    /// <param name="memory"></param>
    public void Write(Instruction instruction, int address, CacheLine cacheLine, Memory memory, byte[] data)
    {
        if (!cacheLine.IsDirty || cacheLine.IsDirty == null)
        {
            memory.SetBlock(address, data);
        }
        
    }
}