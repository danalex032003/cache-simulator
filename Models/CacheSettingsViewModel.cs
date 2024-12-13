using System.ComponentModel.DataAnnotations;

namespace CacheSimulatorWebApp.Models;

public class CacheSettingsViewModel
{
    // public string ReplacementPolicy { get; set; }
    [Required]
    public string WritePolicy { get; set; }
    [Required]
    public string WriteOnMissPolicy { get; set; }
    [Required]
    public string MemorySize { get; set; }
    [Required]
    public string CacheSize { get; set; }
}
