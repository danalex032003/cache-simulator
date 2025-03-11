namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public interface IMappingPolicy
{
    public bool Read(Instruction instruction);
    public bool Write(Instruction instruction, byte[] data);
}