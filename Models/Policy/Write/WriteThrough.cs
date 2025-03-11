using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models;

public class WriteThrough : IWritePolicy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="instruction"></param>
    /// <param name="cacheLine"></param>
    /// <param name="memory"></param>
    public void Write(Instruction instruction, int address, CacheLine cacheLine, Memory memory, byte[] data)
    {
        cacheLine.Data = data;
        cacheLine.Tag = Convert.ToInt32(instruction.Tag, 2);
        memory.SetBlock(address, data);
    }
}