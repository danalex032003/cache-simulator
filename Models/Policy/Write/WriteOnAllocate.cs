namespace CacheSimulatorWebApp.Models.Policy.Write;

public class WriteOnAllocate : IWritePolicy
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
        cacheLine.IsValid = true;
        cacheLine.Tag = Convert.ToInt32(instruction.Tag, 2);
        cacheLine.Data = data;
        if (!cacheLine.IsDirty || cacheLine.IsDirty == null)
        {
            memory.SetBlock(address, data);
        }
    }
}