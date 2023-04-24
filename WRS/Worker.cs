namespace WRS;
using System.Diagnostics;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            generate_report();
            await Task.Delay(1000, stoppingToken);
            return;
        }
    }

    public static void test()
    {
        Thread accountsThread = new Thread(() => wrs_accounts.GetAccountsInfo());
        Thread avThread = new Thread(() => wrs_av.GetAntivirusInfo());
        Thread biosThread = new Thread(() => wrs_bios.GetBiosInfo());
        Thread tcpConnectionsThread = new Thread(() => wrs_network.GetTcpConnectionsInfo());
        Thread productsThread = new Thread(() => wrs_products.GetProductsInfo());
        Thread sysinfoThread = new Thread(() => wrs_sysinfo.GetSystemInfo());
        Thread volumesThread = new Thread(() => wrs_volumes.GetVolumesInfo());
        Thread winUpdatesThread = new Thread(() => wrs_winupdates.GetWinUpdatesInfo());

        System.Console.WriteLine("Starting threads");
        System.Console.WriteLine("Starting accountsThread");
        thread_performance_count(accountsThread);
        System.Console.WriteLine("Starting avThread");
        thread_performance_count(avThread);
        System.Console.WriteLine("Starting biosThread");
        thread_performance_count(biosThread);
        System.Console.WriteLine("Starting tcpConnectionsThread");
        thread_performance_count(tcpConnectionsThread);
        System.Console.WriteLine("Starting productsThread");
        thread_performance_count(productsThread);
        System.Console.WriteLine("Starting sysinfoThread");
        thread_performance_count(sysinfoThread);
        System.Console.WriteLine("Starting volumesThread");
        thread_performance_count(volumesThread);
        System.Console.WriteLine("Starting winUpdatesThread");
        thread_performance_count(winUpdatesThread);
    }

    public static void thread_performance_count(Thread thread)
    {
        var sw = new Stopwatch();
        sw.Start();
        thread.Start();
        thread.Join();
        sw.Stop();
        Console.WriteLine($"Thread {thread.Name} took {sw.ElapsedMilliseconds}ms");
    }



    public static void generate_report()
    {
        wrc_generator.GenerateReport();
    }
}
