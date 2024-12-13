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
    public void Write(Instruction instruction, int address, CacheLine cacheLine, Memory memory)
    {
        cacheLine.IsValid = true;
        cacheLine.Tag = Convert.ToInt32(instruction.Tag, 2);
        if (cacheLine.Data == null || cacheLine.Data.Length != memory.BlockSize)
        {
            cacheLine.Data = memory.GetBlock(address);
        }

        memory.SetBlock(address, cacheLine.Data);
    }
}