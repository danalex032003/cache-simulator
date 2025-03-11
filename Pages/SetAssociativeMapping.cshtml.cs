using System.Text;
using System.Text.Json;
using CacheSimulatorWebApp.Models;
using CacheSimulatorWebApp.Models.Policy.Mapping;
using CacheSimulatorWebApp.Models.Policy.Write;
using CacheSimulatorWebApp.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CacheSimulatorWebApp.Pages;

public class SetAssociativeMappingModel : PageModel
{
    private const string PERSISTED_CACHE_SIZE = "PersistedCacheSize";
    private const string PERSISITED_MEMORY_SIZE = "PersistedMemorySize";
    private const string PERSISTED_BLOCK_SIZE = "PersistedBlockSize";
    private const string PERSISTED_WRITE_POLICY = "PersistedWritePolicy";
    private const string PERSISTED_WRITE_ON_MISS_POLICY = "PersistedWriteOnPolicy";
    private const string PERSISTED_MAPPING_POLICY = "PersistedMappingPolicy";
    private const string PERSISTED_CACHE_LINES = "PersistedCacheLines";
    private const string PERSISTED_LIST_OF_MEMORY_BLOCKS = "PersistedListOfMemoryBlocks";
    
    [BindProperty]
    public WritePolicyEnum SelectedWritePolicyEnum { get; set; }

    [BindProperty]
    public WriteOnMissPolicyEnum SelectedWriteOnMissPolicyEnum { get; set; }
    
    [BindProperty]
    public ReplacementPolicyEnum SelectedReplacementPolicyEnum { get; set; }
    
    [BindProperty]
    public string SelectedCacheSize { get; set; }
    
    [BindProperty]
    public string SelectedMemorySize { get; set; }
    
    [BindProperty]
    public string SelectedInstruction { get; set; }
    
    [BindProperty]
    public string SelectedCurrentAddress { get; set; }
    
    [BindProperty]
    public string SelectedListOfFutureAddresses { get; set; }
    
    [BindProperty]
    public int SelectedNumberOfFutureAdresses { get; set; }

    public List<AddressDirectMappingTableRow> AddressTableRows { get; set; } = [];
    public List<CacheLineTableRow> FirstCacheLineTableRows { get; set; } = [];
    public List<CacheLineTableRow> SecondCacheLineTableRows { get; set; } = [];

    public List<string> ListOfMemoryBlocks { get; set; } = [];
    public bool IsMemoryReady { get; set; } = false;
    
    public bool IsCacheSettingsSubmitted { get; set; }
    public int CurrentMemoryBlockIndex { get; set; } = -1;
    public float HitRate { get; set; }

    private IWritePolicy _writePolicy;
    private IWritePolicy _writeOnMissPolicy;
    private IReplacementPolicy _replacementPolicy;
    
    public void OnGet()
    {
        IsCacheSettingsSubmitted = false;
        SelectedInstruction = "Read/Load";
        SelectedNumberOfFutureAdresses = 10;
    }

    public void OnPostSubmitCacheSettings()
    {
        HttpContext.Session.SetString(PERSISTED_CACHE_SIZE, SelectedCacheSize);
        HttpContext.Session.SetString(PERSISITED_MEMORY_SIZE, SelectedMemorySize);
        HttpContext.Session.SetString(PERSISTED_BLOCK_SIZE, "4");
        const int blockSize = 4;
        const int linesPerSet = 2;
        var numSets = (int.Parse(SelectedCacheSize) / blockSize) / linesPerSet;
        HttpContext.Session.SetInt32("NUM_SETS", numSets);
        var memory = new Memory(int.Parse(SelectedMemorySize), blockSize);
        var cacheLines = new CacheLine[int.Parse(SelectedCacheSize) / blockSize];
        for (var i = 0; i < Convert.ToInt32(SelectedCacheSize) / blockSize; i++)
        {
            cacheLines[i] = new CacheLine();
        }
        if (!GetPolicies()) return;

        var statistics = new Statistics.Statistics();
        var mappingPolicy = new SetAssociativeMapping(
            cacheLines,
            _writePolicy,
            _writeOnMissPolicy,
            memory,
            _replacementPolicy,
            statistics,
            linesPerSet,
            numSets);
        
        HttpContext.Session.SetString(PERSISTED_CACHE_LINES, JsonSerializer.Serialize(
            mappingPolicy.CacheLines,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            }
            ));
        HttpContext.Session.SetString(PERSISTED_MAPPING_POLICY, JsonSerializer.Serialize(
            mappingPolicy,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            }));
        var x = 0;
        for (var i = 0; i < numSets; i++)
        {
            FirstCacheLineTableRows.Add(new CacheLineTableRow
            {
                Index = i.ToString(),
                IsValid = "-",
                Data = "-",
                IsDirty = "-",
                Tag = "-"
            });
            
            SecondCacheLineTableRows.Add(new CacheLineTableRow
            {
                Index = i.ToString(),
                IsValid = "-",
                Data = "-",
                IsDirty = "-",
                Tag = "-"
            });
        }
        // foreach (var cacheLine in mappingPolicy.CacheLines)
        // {
        //     FirstCacheLineTableRows.Add(new CacheLineTableRow
        //     {
        //         Index = x.ToString(),
        //         IsValid = "-",
        //         Data = "-",
        //         IsDirty = "-",
        //         Tag = "-"
        //     });
        //     x++;
        // }
        
        SetListOfMemoryBlocks(memory);
        
    }

    public void SetListOfMemoryBlocks(Memory memory)
    {
        IsMemoryReady = false;
        for (var i = 0; i < memory.Data.Length; i += memory.BlockSize)
        {
            var sb = new StringBuilder();
            // Take a chunk of BlockSize bytes
            var chunk = memory.Data.Skip(i).Take(memory.BlockSize);
            // Convert each byte to a hex string and join them with hyphens
            var chunkString = string.Join("-", chunk.Select(b => b.ToString("X2")));
            sb.AppendLine($"[{i:D4}]: {chunkString}");
            ListOfMemoryBlocks.Add(sb.ToString());
        }
        if (ListOfMemoryBlocks.Count == 0) return;
        IsMemoryReady = true;
    }
    
    public void OnPostGenerateAddresses()
    {
        var mappingPolicyJson = HttpContext.Session.GetString(PERSISTED_MAPPING_POLICY);
        if (mappingPolicyJson == null) return;
        var mappingPolicy = JsonSerializer.Deserialize<SetAssociativeMapping>(
            mappingPolicyJson,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
        Console.WriteLine("TAG: " + mappingPolicy);
    }

    public void OnPostSubmitInstruction()
    {
        var address = Convert.ToInt32(SelectedCurrentAddress, 16);
        var cacheSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_CACHE_SIZE));
        var memorySize = Convert.ToInt32(HttpContext.Session.GetString(PERSISITED_MEMORY_SIZE));
        var blockSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_BLOCK_SIZE));
        
        var instruction = new Instruction(address, cacheSize, memorySize, blockSize);
        AddressTableRows.Add(
            new AddressDirectMappingTableRow()
            {
                Tag = instruction.Tag,
                Index = instruction.Index,
                Offset = instruction.Offset
            });
        var mappingPolicyJson = HttpContext.Session.GetString(PERSISTED_MAPPING_POLICY);
        if (mappingPolicyJson == null) return;
        var mappingPolicy = JsonSerializer.Deserialize<SetAssociativeMapping>(
            mappingPolicyJson,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
        if (mappingPolicy == null) return;
        GetPolicies();
        mappingPolicy.SetWritePolicy(_writePolicy);
        mappingPolicy.SetWriteOnMissPolicy(_writeOnMissPolicy);
        mappingPolicy.SetReplacementPolicy(_replacementPolicy);
        byte[] data = null;
        switch (SelectedInstruction)
        {
            case "Read/Load":
                mappingPolicy.Read(instruction);
                break;
            case "Write/Store":
                mappingPolicy.Write(instruction, data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var addressInMemory = Convert.ToInt32(instruction.AddressInMemory, 2);
        CurrentMemoryBlockIndex = addressInMemory - addressInMemory % mappingPolicy.Memory.BlockSize;
        CurrentMemoryBlockIndex /= mappingPolicy.Memory.BlockSize;
        UpdateCacheLineTableRows(mappingPolicy);
        HttpContext.Session.SetString(PERSISTED_MAPPING_POLICY, JsonSerializer.Serialize(
            mappingPolicy,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            }));
        
        SetListOfMemoryBlocks(mappingPolicy.Memory);
        
        HitRate = mappingPolicy.Statistics.GetHitRate();
    }

    private void UpdateCacheLineTableRows(SetAssociativeMapping mappingModelPolicy)
    {
        var x = 0;
        FirstCacheLineTableRows = [];
        SecondCacheLineTableRows = [];
        // foreach (var lines in mappingModelPolicy.CacheLines)
        // {
        //     foreach (var cacheLine in lines)
        //     {
        //         if (cacheLine.Data == null)
        //         {
        //             FirstCacheLineTableRows.Add(new CacheLineTableRow
        //             {
        //                 Index = x.ToString(),
        //                 IsValid = "-",
        //                 Data = "-",
        //                 IsDirty = "-",
        //                 Tag = "-"
        //             });
        //         }
        //         else
        //         {
        //             FirstCacheLineTableRows.Add(new CacheLineTableRow
        //             {
        //                 Index = x.ToString(),
        //                 IsValid = cacheLine.IsValid.ToString(),
        //                 Data = string.Join("-", cacheLine.Data.Select(b => b.ToString("X2"))),
        //                 IsDirty = cacheLine.IsDirty.ToString(),
        //                 Tag = cacheLine.Tag.ToString()
        //             });
        //         }
        //         x++;
        //     }
        //     
        // }
        var numSets = HttpContext.Session.GetInt32("NUM_SETS");
        for (var i = 0; i < numSets; i++)
        {
            if (mappingModelPolicy.CacheLines[i][0].Data == null)
            {
                FirstCacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = i.ToString(),
                    IsValid = "-",
                    Data = "-",
                    IsDirty = "-",
                    Tag = "-"
                });
            }
            else
            {
                FirstCacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = x.ToString(),
                    IsValid = mappingModelPolicy.CacheLines[i][0].IsValid.ToString(),
                    Data = string.Join("-", mappingModelPolicy.CacheLines[i][0].Data.Select(b => b.ToString("X2"))),
                    IsDirty = mappingModelPolicy.CacheLines[i][0].IsDirty.ToString(),
                    Tag = mappingModelPolicy.CacheLines[i][0].Tag.ToString()
                });
            }
            
            if (mappingModelPolicy.CacheLines[i][1].Data == null)
            {
                SecondCacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = i.ToString(),
                    IsValid = "-",
                    Data = "-",
                    IsDirty = "-",
                    Tag = "-"
                });
            }
            else
            {
                SecondCacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = x.ToString(),
                    IsValid = mappingModelPolicy.CacheLines[i][1].IsValid.ToString(),
                    Data = string.Join("-", mappingModelPolicy.CacheLines[i][1].Data.Select(b => b.ToString("X2"))),
                    IsDirty = mappingModelPolicy.CacheLines[i][1].IsDirty.ToString(),
                    Tag = mappingModelPolicy.CacheLines[i][1].Tag.ToString()
                });
            }
            
            // SecondCacheLineTableRows.Add(new CacheLineTableRow
            // {
            //     Index = i.ToString(),
            //     IsValid = "-",
            //     Data = "-",
            //     IsDirty = "-",
            //     Tag = "-"
            // });
        }
    }

    private bool GetPolicies()
    {
        
        switch (SelectedWritePolicyEnum)
        {
            case WritePolicyEnum.WRITE_BACK:
                _writePolicy = new WriteBack();
                break;
            case WritePolicyEnum.WRITE_THROUGH:
                _writePolicy = new WriteThrough();
                break;
            default:
                return false;
        }

        switch (SelectedWriteOnMissPolicyEnum)
        {
            case WriteOnMissPolicyEnum.WRITE_ON_ALLOCATE:
                _writeOnMissPolicy = new WriteOnAllocate();
                break;
            case WriteOnMissPolicyEnum.WRITE_AROUND:
                _writeOnMissPolicy = new WriteAround();
                break;
            default:
                return false;
        }

        switch (SelectedReplacementPolicyEnum)
        {
            case ReplacementPolicyEnum.LRU:
                _replacementPolicy = new LRU();
                break;
            case ReplacementPolicyEnum.FIFO:
                _replacementPolicy = new FIFO();
                break;
            case ReplacementPolicyEnum.RANDOM:
                _replacementPolicy = new RandomReplacement();
                break;
            default:
                return false;
        }
        return true;
    }
    private static bool IsPowerOfTwo(int x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }
}