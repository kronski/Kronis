using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
        
            CancellationTokenSource s = new CancellationTokenSource();

            Task waitkey = Task.Run(()=>{
                Console.ReadKey();
                s.Cancel();
            });

            Task find = Task.Run(async () =>
            {
                await KronisHue.BridgeLocator.FindHueBridgeViaUpnp(device =>
                  {
                      Console.WriteLine(device);
                  }, s);
                Console.WriteLine("Done");
            });

            Task.WaitAll(new Task[] { waitkey, find });
            
            
        }
    }
}
