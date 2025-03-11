using System.Text;
using System.Text.Json;
using CacheSimulatorWebApp.Models;
using CacheSimulatorWebApp.Models.Policy.Mapping;
using CacheSimulatorWebApp.Models.Policy.Write;
using CacheSimulatorWebApp.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReplacementPolicyEnum = CacheSimulatorWebApp.Util.ReplacementPolicyEnum;

namespace CacheSimulatorWebApp.Pages;

public class FullyAssociativeMappingModel : PageModel
{
    
    private readonly ILogger<FullyAssociativeMappingModel> _logger;
    
    private const string PERSISTED_CACHE_SIZE = "PersistedCacheSize";
    private const string PERSISITED_MEMORY_SIZE = "PersistedMemorySize";
    private const string PERSISTED_BLOCK_SIZE = "PersistedBlockSize";
    private const string PERSISTED_INSTRUCTION = "PersistedInstruction";
    private const string PERSISTED_WRITE_POLICY = "PersistedWritePolicy";
    private const string PERSISTED_WRITE_ON_MISS_POLICY = "PersistedWriteOnPolicy";
    private const string PERSISTED_REPLACEMENT_POLICY = "PersistedReplacementPolicy";
    private const string PERSISTED_MAPPING_POLICY = "PersistedMappingPolicy";
    private const string PERSISTED_CACHE_LINES = "PersistedCacheLines";
    private const string PERSISTED_LIST_OF_MEMORY_BLOCKS = "PersistedListOfMemoryBlocks";
    private const string PERSISTED_COUNT_FOR_STEP = "PersistedCountForStep";
    private const string PERSISTED_TYPE_OF_INSTRUCTION = "PersistedTypeOfInstruction";
    private const string PERSISTED_WRITE_DATA = "PersistedWriteData";
    private const string PERSISTED_CURRENT_INDEX = "PersistedCurrentIndex";
    private const string PERSISTED_IS_HIT = "PersistedIsHit";
    private const string PERSISTED_CURRENT_MEMORY_BLOCK_INDEX = "PersistedCurrentMemoryBlockIndex";
    
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
    public string SelectedWriteData { get; set; }
    
    [BindProperty]
    public string SelectedCurrentAddress { get; set; }
    
    [BindProperty]
    public string SelectedListOfFutureAddresses { get; set; }
    
    [BindProperty]
    public int SelectedNumberOfFutureAdresses { get; set; }

    public List<AddressFullyAssociativeMappingTableRow> AddressTableRows { get; set; } = [];
    public List<CacheLineTableRow> CacheLineTableRows { get; set; } = [];

    public List<string> ListOfMemoryBlocks { get; set; } = [];
    public bool IsMemoryReady { get; set; } = false;
    
    public bool IsCacheSettingsSubmitted { get; set; }
    public int CurrentMemoryBlockIndex { get; set; } = -1;
    public float HitRate { get; set; }
    
    public string ExplanationMessage { get; set; }
    public string ExplanationColor { get; set; }
    public string ExplanationDivColor { get; set; } = "yellow";
    public bool IsInstructionFinished { get; set; } = false;

    public int StepNo { get; set; } = 0;
    public string CurrentCacheLineIndex { get; set; }
    
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
        var memory = new Memory(int.Parse(SelectedMemorySize), blockSize);
        var cacheLines = new CacheLine[int.Parse(SelectedCacheSize) / blockSize];
        for (var i = 0; i < Convert.ToInt32(SelectedCacheSize) / blockSize; i++)
        {
            cacheLines[i] = new CacheLine();
        }
        if (!GetPolicies()) return;

        var statistics = new Statistics.Statistics();
        var mappingPolicy = new FullyAssociativeMapping(
            cacheLines,
            _writePolicy,
            _writeOnMissPolicy,
            memory,
            _replacementPolicy,
            statistics);
        
        HttpContext.Session.SetString(PERSISTED_CACHE_LINES, JsonSerializer.Serialize(
            cacheLines,
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
        foreach (var cacheLine in cacheLines)
        {
            CacheLineTableRows.Add(new CacheLineTableRow
            {
                Index = x.ToString(),
                IsValid = "-",
                Data = "-",
                IsDirty = "-",
                Tag = "-"
            });
            x++;
        }
        
        SetListOfMemoryBlocks(memory);
     
        var sb = new StringBuilder();
        sb.AppendLine($"Address: log2({SelectedMemorySize}) = {Math.Log2(int.Parse(SelectedMemorySize))} bits");
        sb.AppendLine("Offset: 2 bits");
        sb.AppendLine(
            $"Tag: {Math.Log2(int.Parse(SelectedMemorySize)) - Math.Log2(int.Parse(SelectedCacheSize) / blockSize) + Math.Log2(int.Parse(SelectedCacheSize) / blockSize) - 2}" +
            $" bits");
        
        
        ExplanationMessage = sb.ToString();
        ExplanationColor = "black";
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
        var mappingPolicy = JsonSerializer.Deserialize<FullyAssociativeMapping>(
            mappingPolicyJson,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
    }

    public void OnPostSubmitInstruction()
    {
        
        var address = Convert.ToInt32(SelectedCurrentAddress, 16);
        var cacheSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_CACHE_SIZE));
        var memorySize = Convert.ToInt32(HttpContext.Session.GetString(PERSISITED_MEMORY_SIZE));
        var blockSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_BLOCK_SIZE));
        
        HttpContext.Session.SetInt32(PERSISTED_COUNT_FOR_STEP, 0);
        
        var instruction = new Instruction(address, cacheSize, memorySize, blockSize);
        HttpContext.Session.SetString(PERSISTED_INSTRUCTION, JsonSerializer.Serialize(
            instruction,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            }));
        AddressTableRows.Add(
            new AddressFullyAssociativeMappingTableRow
            {
                Tag = instruction.AddressInMemory,
                Offset = instruction.Offset
            });
        var mappingPolicyJson = HttpContext.Session.GetString(PERSISTED_MAPPING_POLICY);
        if (mappingPolicyJson == null) return;
        var mappingPolicy = JsonSerializer.Deserialize<FullyAssociativeMapping>(
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
        switch (SelectedInstruction)
        {
            case "Read/Load":
                HttpContext.Session.SetString(PERSISTED_TYPE_OF_INSTRUCTION, "read");
                break;
            case "Write/Store":
                HttpContext.Session.SetString(PERSISTED_TYPE_OF_INSTRUCTION, "write");
                HttpContext.Session.SetString(PERSISTED_WRITE_DATA, SelectedWriteData);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var addressInMemory = Convert.ToInt32(instruction.AddressInMemory, 2);
        CurrentMemoryBlockIndex = addressInMemory - addressInMemory % mappingPolicy.Memory.BlockSize;
        CurrentMemoryBlockIndex /= mappingPolicy.Memory.BlockSize;
        HttpContext.Session.SetInt32(PERSISTED_CURRENT_MEMORY_BLOCK_INDEX, CurrentMemoryBlockIndex);
        UpdateCacheLineTableRows(mappingPolicy.CacheLines);
        HttpContext.Session.SetString(PERSISTED_MAPPING_POLICY, JsonSerializer.Serialize(
            mappingPolicy,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            }));
        
        SetListOfMemoryBlocks(mappingPolicy.Memory);
        
        HitRate = mappingPolicy.Statistics.GetHitRate();
        
        var sb = new StringBuilder();
        sb.AppendLine("The instruction is converted");
        sb.AppendLine("from hex to binary.");
        
        ExplanationMessage = sb.ToString();
        ExplanationColor = "black";
        ExplanationDivColor = "yellow";
    }

    public static byte[] StringToByteArray(string s, string dataString, int index)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("Hex string cannot be null or empty", nameof(s));
        if (s.Length > 2)
            throw new ArgumentException("Hex string cannot be longer than 2 character", nameof(s));
        // Split the string by the dash delimiter
        string[] hexValues = dataString.Substring(8, 11).Split('-');
        hexValues[index] = s;
        // Convert each hex pair into a byte
        
        byte[] byteArray = new byte[hexValues.Length];
        for (int i = 0; i < hexValues.Length; i++)
        {
            byteArray[i] = Convert.ToByte(hexValues[i], 16);
        }
        return byteArray;
    }
    
    public void OnPostFastForwardInstruction()
    {
        var mappingPolicyJson = HttpContext.Session.GetString(PERSISTED_MAPPING_POLICY);
        if (mappingPolicyJson == null) return;
        var mappingPolicy = JsonSerializer.Deserialize<FullyAssociativeMapping>(
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
        
        StepNo = 5;
        var instructionType = HttpContext.Session.GetString(PERSISTED_TYPE_OF_INSTRUCTION);
        var instructionJson = HttpContext.Session.GetString(PERSISTED_INSTRUCTION);
        if (instructionJson == null) return;
        var instruction = JsonSerializer.Deserialize<Instruction>(
            instructionJson,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
        if (instruction == null) return;
        
        bool isHit;
        switch (instructionType)
        {
            case "read":
                isHit = mappingPolicy.Read(instruction);
                break;
            case "write":
                SetListOfMemoryBlocks(mappingPolicy.Memory);
                var dataString = HttpContext.Session.GetString(PERSISTED_WRITE_DATA);
                var currentMemoryBlockIndex = HttpContext.Session.GetInt32(PERSISTED_CURRENT_MEMORY_BLOCK_INDEX);
                if (currentMemoryBlockIndex == null) return;
                if (dataString == null) return;
                var data = StringToByteArray(
                    dataString,
                    ListOfMemoryBlocks[(int)currentMemoryBlockIndex],
                    Convert.ToInt32(instruction.Offset, 2));
                isHit = mappingPolicy.Write(instruction, data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        UpdateCacheLineTableRows(mappingPolicy.CacheLines);
        SetListOfMemoryBlocks(mappingPolicy.Memory);
        HitRate = mappingPolicy.Statistics.GetHitRate();

    }

    public int Variant { get; set; }
    
    public void OnPostStepInstruction()
    {
        var mappingPolicyJson = HttpContext.Session.GetString(PERSISTED_MAPPING_POLICY);
        if (mappingPolicyJson == null) return;
        var mappingPolicy = JsonSerializer.Deserialize<FullyAssociativeMapping>(
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
        var instructionType = HttpContext.Session.GetString(PERSISTED_TYPE_OF_INSTRUCTION);
        var instructionJson = HttpContext.Session.GetString(PERSISTED_INSTRUCTION);
        if (instructionJson == null) return;
        var instruction = JsonSerializer.Deserialize<Instruction>(
            instructionJson,
            new JsonSerializerOptions { 
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
        if (instruction == null) return;
        var stepNo = HttpContext.Session.GetInt32(PERSISTED_COUNT_FOR_STEP);
        
        var currentIndex = HttpContext.Session.GetString(PERSISTED_CURRENT_INDEX);

        stepNo++;
        if (stepNo != null)
        {
            StepNo = (int)stepNo;
            switch (instructionType)
            {
                case "read":
                    switch (stepNo)
                    {
                        case 1:
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("Address will be searched in the");
                            sb.AppendLine("whole cache.");
                            ExplanationMessage = sb.ToString();
                            ExplanationColor = "black";
                            ExplanationDivColor = "yellow";
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            Variant = 1;
                            break;
                        }
                        case 2:
                        {
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                    
                            var isHit = mappingPolicy.Read(instruction);
                    
                            HttpContext.Session.SetString(PERSISTED_MAPPING_POLICY, JsonSerializer.Serialize(
                                mappingPolicy,
                                new JsonSerializerOptions { 
                                    IncludeFields = true,
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                                }));
                    
                            var sb = new StringBuilder();
                            if (isHit)
                            {
                                sb.AppendLine("Valid tag is found");
                                sb.AppendLine("=> Cache HIT");
                                ExplanationDivColor = "green";
                            }
                            else
                            {
                                sb.AppendLine($"No cache line contains {instruction.AddressInMemory}");
                                sb.AppendLine("=> Cache MISS");
                                ExplanationDivColor = "red";
                            }
                    
                            ExplanationMessage = sb.ToString();
                            ExplanationColor = "black";

                            if (isHit)
                            {
                                HttpContext.Session.SetInt32(PERSISTED_IS_HIT, 1);
                            }
                            else
                            {
                                HttpContext.Session.SetInt32(PERSISTED_IS_HIT, 0);
                            }
                            
                            
                            Variant = 2;
                            break;
                        }
                        case 3:
                        {
                            var sb = new StringBuilder();
                            var isHit = HttpContext.Session.GetInt32(PERSISTED_IS_HIT) == 1;
                            if (isHit)
                            {
                                sb.AppendLine("Data is loaded from cache.");
                            }
                            else
                            {
                                sb.AppendLine("New cache data will be imported.");
                            }
                            
                            sb.AppendLine("The cache line will be updated accordingly");

                            ExplanationMessage = sb.ToString();
                            
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            break;
                        }
                    }
                    break;
                
                case "write":
                    switch (stepNo)
                    {
                        case 1:
                        {
                            var writePolicy = HttpContext.Session.GetString(PERSISTED_WRITE_POLICY);
                            if (writePolicy == null) return;
                            var sb = new StringBuilder();
                            if (writePolicy == "write_back")
                            {
                                sb.AppendLine("Write back policy will be adopted.");
                                sb.AppendLine("The cache will use a dirty bit.");
                            }
                            else
                            {
                                sb.AppendLine("Write Through policy will be adopted.");
                                sb.AppendLine("Cache and system memory will be updated");
                                sb.AppendLine("at the same time.");
                            }
                    
                            ExplanationMessage = sb.ToString();
                            ExplanationColor = "black";
                            ExplanationDivColor = "yellow";
                    
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            break;
                        }
                        case 2:
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("The address will be searched in the whole cache.");
                            
                            ExplanationMessage = sb.ToString();
                            
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            
                            Variant = 1;
                            break;
                        }
                        case 3:
                        {
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            var dataString = HttpContext.Session.GetString(PERSISTED_WRITE_DATA);
                            var currentMemoryBlockIndex = HttpContext.Session.GetInt32(PERSISTED_CURRENT_MEMORY_BLOCK_INDEX);
                            if (currentMemoryBlockIndex == null) return;
                            if (dataString == null) return;
                            var data = StringToByteArray(
                                dataString,
                            ListOfMemoryBlocks[(int)currentMemoryBlockIndex],
                            Convert.ToInt32(instruction.Offset, 2));
                            var isHit = mappingPolicy.Write(instruction, data);
                    
                            HttpContext.Session.SetString(PERSISTED_MAPPING_POLICY, JsonSerializer.Serialize(
                                mappingPolicy,
                                new JsonSerializerOptions { 
                                    IncludeFields = true,
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                                }));
                            

                            var sb = new StringBuilder();
                            
                            if (isHit)
                            {
                                HttpContext.Session.SetInt32(PERSISTED_IS_HIT, 1);
                                sb.AppendLine("Address is found in the cache.");
                                sb.AppendLine("=> Cache HIT");
                                ExplanationDivColor = "green";
                            }
                            else
                            {
                                HttpContext.Session.SetInt32(PERSISTED_IS_HIT, 0);
                                sb.AppendLine("Address is not found in the cache.");
                                sb.AppendLine("=> Cache MISS");
                                ExplanationDivColor = "red";
                            }

                            ExplanationMessage = sb.ToString();
                            ExplanationColor = "black";
                            
                            Variant = 2;
                            break;
                        }
                        case 4:
                        {
                            UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                            SetListOfMemoryBlocks(mappingPolicy.Memory);
                            var writePolicy = HttpContext.Session.GetString(PERSISTED_WRITE_POLICY);
                            if (writePolicy == null) return;
                            var writeOnMissPolicy = HttpContext.Session.GetString(PERSISTED_WRITE_ON_MISS_POLICY);
                            if (writeOnMissPolicy == null) return;
                            var isHit = HttpContext.Session.GetInt32(PERSISTED_IS_HIT) == 1;
                            var sb = new StringBuilder();
                            if (isHit)
                            {
                                if (writePolicy == "write_back")
                                {
                                    sb.AppendLine("Cache will be updated with dirty bit.");
                                }
                                else
                                {
                                    sb.AppendLine("Cache and system memory will be updated.");
                                }
                            }
                            else
                            {
                                if (writeOnMissPolicy == "write_on_allocate")
                                {
                                    sb.AppendLine("Write on Allocate policy will be adopted.");
                                }
                                else
                                {
                                    sb.AppendLine("Write Around policy will be adopted.");
                                }
                            }
                            ExplanationMessage = sb.ToString();
                            break;
                        }
                    }
                    break;
            }

            if (StepNo > 0)
            {
                UpdateAddressLine();
            }

            if (stepNo >= 4)
            {
                UpdateCacheLineTableRows(mappingPolicy.CacheLines);
                SetListOfMemoryBlocks(mappingPolicy.Memory);
            }
            
            HitRate = mappingPolicy.Statistics.GetHitRate();
            HttpContext.Session.SetInt32(PERSISTED_COUNT_FOR_STEP, StepNo);
        }
    }

    private void UpdateAddressLine()
    {
        var address = Convert.ToInt32(SelectedCurrentAddress, 16);
        var cacheSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_CACHE_SIZE));
        var memorySize = Convert.ToInt32(HttpContext.Session.GetString(PERSISITED_MEMORY_SIZE));
        var blockSize = Convert.ToInt32(HttpContext.Session.GetString(PERSISTED_BLOCK_SIZE));

        
        var instruction = new Instruction(address, cacheSize, memorySize, blockSize);
        AddressTableRows.Add(
            new AddressFullyAssociativeMappingTableRow()
            {
                Tag = instruction.AddressInMemory,
                Offset = instruction.Offset
            });
    }

    private void UpdateCacheLineTableRows(CacheLine[] cacheLines)
    {
        var x = 0;
        CacheLineTableRows = [];
        foreach (var cacheLine in cacheLines)
        {
            if (cacheLine.Data == null)
            {
                CacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = x.ToString(),
                    IsValid = "-",
                    Data = "-",
                    IsDirty = "-",
                    Tag = "-"
                });
            }
            else
            {
                CacheLineTableRows.Add(new CacheLineTableRow
                {
                    Index = x.ToString(),
                    IsValid = cacheLine.IsValid.ToString(),
                    Data = string.Join("-", cacheLine.Data.Select(b => b.ToString("X2"))),
                    IsDirty = cacheLine.IsDirty.ToString(),
                    Tag = Convert.ToString(cacheLine.Tag, 2)
                });
            }
            x++;
        }
    }

    private bool GetPolicies()
    {
        
        switch (SelectedWritePolicyEnum)
        {
            case WritePolicyEnum.WRITE_BACK:
                _writePolicy = new WriteBack();
                HttpContext.Session.SetString(PERSISTED_WRITE_POLICY, "write_back");
                break;
            case WritePolicyEnum.WRITE_THROUGH:
                _writePolicy = new WriteThrough();
                HttpContext.Session.SetString(PERSISTED_WRITE_POLICY, "write_through");
                break;
            default:
                return false;
        }

        switch (SelectedWriteOnMissPolicyEnum)
        {
            case WriteOnMissPolicyEnum.WRITE_ON_ALLOCATE:
                _writeOnMissPolicy = new WriteOnAllocate();
                HttpContext.Session.SetString(PERSISTED_WRITE_ON_MISS_POLICY, "write_on_allocate");
                break;
            case WriteOnMissPolicyEnum.WRITE_AROUND:
                _writeOnMissPolicy = new WriteAround();
                HttpContext.Session.SetString(PERSISTED_WRITE_ON_MISS_POLICY, "write_around");
                break;
            default:
                return false;
        }

        switch (SelectedReplacementPolicyEnum)
        {
            case ReplacementPolicyEnum.LRU:
                _replacementPolicy = new LRU();
                HttpContext.Session.SetString(PERSISTED_REPLACEMENT_POLICY, "lru");
                break;
            case ReplacementPolicyEnum.FIFO:
                _replacementPolicy = new FIFO();
                HttpContext.Session.SetString(PERSISTED_REPLACEMENT_POLICY, "fifo");
                break;
            case ReplacementPolicyEnum.RANDOM:
                _replacementPolicy = new RandomReplacement();
                HttpContext.Session.SetString(PERSISTED_REPLACEMENT_POLICY, "random");
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

