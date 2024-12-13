namespace CacheSimulatorWebApp.Models;

public class CacheLine
{
    public bool IsValid { get; set; }
    public int Tag { get; set; }
    public byte[] Data { get; set; }
    public bool IsDirty { get; set; }
    public DateTime LastModifiedDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; }
    

    public CacheLine()
    {
        IsValid = false;
        IsDirty = false;
    }

    public override string ToString()
    {
        var dataString = Data == null ? "null" : string.Join("-", Data.Select(b => b.ToString("X2")));
        return $"Tag: {Convert.ToString(Tag, 2)}, IsValid: {IsValid}, Data: {dataString}, isDirty: {IsDirty}";
    }
}