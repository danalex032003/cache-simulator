using CacheSimulatorWebApp.Models.Policy.Write;

namespace CacheSimulatorWebApp.Models;

public class WriteBack : IWritePolicy
{
    public void Write(Instruction instruction, int address, CacheLine cacheLine, Memory memory, byte[] data)
    {
        cacheLine.Data = data;
        cacheLine.IsDirty = true;
        cacheLine.Tag = Convert.ToInt32(instruction.Tag, 2);
    }
}