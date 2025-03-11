namespace CacheSimulatorWebApp.Util;

public class AddressDirectMappingTableRow
{
    public string Tag { get; set; }
    public string Index { get; set; }
    public string Offset { get; set; }
}

public class AddressFullyAssociativeMappingTableRow
{
    public string Tag { get; set; }
    public string Offset { get; set; }
}

public class CacheLineTableRow
{
    public string Index { get; set; }
    public string IsValid { get; set; }
    public string Tag { get; set; }
    public string Data { get; set; }
    public string IsDirty { get; set; }
}