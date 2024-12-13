namespace CacheSimulatorWebApp.Models.Policy.Mapping;

public interface IMappingPolicy
{
    public CacheLine Read(Instruction instruction);
    public bool Write(Instruction instruction);
}