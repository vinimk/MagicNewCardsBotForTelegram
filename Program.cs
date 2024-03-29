namespace MagicNewCardsBot;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration configuration = hostContext.Configuration;

                WorkerOptions options = configuration.GetSection("Configs").Get<WorkerOptions>();
                if (options != null)
                {
                    _ = services.AddSingleton(options);
                }

                _ = services.AddHostedService<Worker>();
            });
    }
}

