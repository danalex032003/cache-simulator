using CacheSimulatorWebApp.Models;
using CacheSimulatorWebApp.Models.Policy.Mapping;
using CacheSimulatorWebApp.Models.Policy.Write;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CacheSimulatorWebApp.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public ActionResult Index()
    {
        var model = new CacheSettingsViewModel();
        // Create a new instance of the view model
        Populate();
        return View();
    }

    [HttpPost]
    public ActionResult Index(CacheSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            Populate();
            return View(model);
        }
        return View("Index", model);
    }

    public void Populate()
    {
        // Populate dropdown options for the Write Policy dropdown
        ViewBag.WritePolicies = new List<SelectListItem>
        {
            new SelectListItem { Value = "Write-back", Text = "Write-back" },
            new SelectListItem { Value = "Write-through", Text = "Write-through" }
        };

        // Populate dropdown options for the Write Policy on Miss dropdown
        ViewBag.WritePoliciesOnMiss = new List<SelectListItem>
        {
            new SelectListItem { Value = "Write-on-allocate", Text = "Write-on-allocate" },
            new SelectListItem { Value = "Write-around", Text = "Write-around" }
        };

        // Populate dropdown options for the Memory Size dropdown
        ViewBag.MemorySizes = new List<SelectListItem>
        {
            new SelectListItem { Value = "1024", Text = "1024" },
            new SelectListItem { Value = "2048", Text = "2048" },
            new SelectListItem { Value = "4096", Text = "4096" }
        };

        // Populate dropdown options for the Cache Size dropdown
        ViewBag.CacheSizes = new List<SelectListItem>
        {
            new SelectListItem { Value = "16", Text = "16" },
            new SelectListItem { Value = "32", Text = "32" },
            new SelectListItem { Value = "64", Text = "64" }
        };

        // Pass the view model to the view
        
    }
        
    
    //[HttpPost]
    // public ActionResult SaveSettings(CacheSettingsViewModel model)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         // Access the values from the dropdowns
    //         //var replacementPolicy = model.ReplacementPolicy;
    //         var writePolicyName = model.WritePolicy;
    //         var writeOnMissPolicyName = model.WriteOnMissPolicy;
    //         var memorySize = model.MemorySize;
    //         var cacheSize = model.CacheSize;
    //
    //         IWritePolicy writePolicy = writePolicyName switch
    //         {
    //             "Write-back" => new WriteBack(),
    //             "Write-through" => new WriteThrough(),
    //             _ => new WriteAround()
    //         };
    //
    //         IWritePolicy writeOnMissPolicy = writeOnMissPolicyName switch
    //         {
    //             "Write-around" => new WriteAround(),
    //             "Write-on-allocate" => new WriteOnAllocate(),
    //             _ => new WriteAround()
    //         };
    //
    //         // Do something with the values (e.g., store them, process them, etc.)
    //         GenerateCache(
    //             writePolicy, writeOnMissPolicy, Convert.ToInt32(memorySize), Convert.ToInt32(cacheSize));
    //         // Optionally, return to another view or reload the current view
    //         //return RedirectToAction("Index"); // or return View("SomeOtherView");
    //     }
    //
    //     // If model is invalid, re-display the form
    //     return View("Index", model);
    // }

    public void GenerateCache(IWritePolicy writePolicy, IWritePolicy writeOnMiss, int memorySize, int cacheSize)
    {
        // const int memorySize = 4096;
        // const int cacheSize = 64;
        var memory = new Memory(memorySize, 4);
        Console.WriteLine(memory);
        var instructions = new Instruction[30];
        int[] instr = [
            0xb77, 0x441, 0xba7,170, 0xe0f, 0x45, 0x448, 0xedd, 0x878, 0xa81,
            0x71d, 0x385, 0x60c, 0x39a, 0xdc5, 0xbfb, 0x8e3, 0x479, 0x48e, 0xe2d,
            0xf41, 0x7cb, 0x538, 0xb64, 0x2a3, 0xcae, 0xe3b, 0x478, 0x48e, 0xe2d];
        for (var i = 0; i < 30; i++)
        {
            instructions[i] = new Instruction(instr[i], cacheSize, memorySize, 4);
        }
        //var instruction = new Instruction(1288, cacheSize, memorySize);
    
        var cacheLines = new CacheLine[cacheSize/4];
        for (var i = 0; i < cacheSize/4; i++)
        {
            cacheLines[i] = new CacheLine();
        }
    
        // IWritePolicy writePolicy = new WriteBack();
        // IWritePolicy writeOnMiss = new WriteOnAllocate();
        
        var mappingPolicy = new DirectMapping(cacheLines, writePolicy, writeOnMiss, memory, new Statistics.Statistics());
        for (var i = 0; i < 10; i++)
        {
            mappingPolicy.Read(instructions[i]);
        }

        for (var i = 10; i < 30; i++)
        {
            mappingPolicy.Write(instructions[i]);
        }
        //Console.WriteLine($"Address in memory: {instruction.AddressInMemory}");
        // var cache = new Cache(mappingPolicy, replacementPolicy, writePolicy, cacheSize, 0, 4, memory);
        for (var i = 0; i < cacheSize / 4; i++)
        {
            Console.WriteLine(cacheLines[i]);
        }
    }
}