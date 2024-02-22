using SampleDAppNetCore;

namespace SampleDAppNetCore;

class Program
{
    static async Task Main(string[] args) 
    {
        await Executor.InvokeContract();
        Console.ReadKey();
    }
}
