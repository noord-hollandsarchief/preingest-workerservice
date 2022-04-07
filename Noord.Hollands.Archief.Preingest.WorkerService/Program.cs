using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Noord.Hollands.Archief.Preingest.WorkerService.Handler;
using Noord.Hollands.Archief.Preingest.WorkerService.Entities;
using Noord.Hollands.Archief.Preingest.WorkerService.Handler.Creator;

namespace Noord.Hollands.Archief.Preingest.WorkerService
{
    /// <summary>
    /// Default entry point
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //create this service instance
                    services.AddHostedService<Worker>();

                    var appSettingsSection = hostContext.Configuration.GetSection("AppSettings");
                    services.Configure<AppSettings>(appSettingsSection);

                    var settings = appSettingsSection.Get<AppSettings>();                   
                    //create event hub
                    services.Add(new ServiceDescriptor(typeof(AppSettings), settings));
                    services.AddSingleton<PreingestEventHubHandler>();
                    //services.Add(new ServiceDescriptor(typeof(PreingestEventHubHandler), new PreingestEventHubHandler(settings.EventHubUrl, settings.WebApiUrl)));                   

                }).ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     var env = hostingContext.HostingEnvironment;
                    // Probably unexpected: in WorkerServiceContext the same code does not specify `optional: true,
                    // reloadOnChange: true`, and does not load the 2nd JSON file
                     config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);//optional extra provider

                     if (env.IsDevelopment()) { }//different providers in dev                     

                     config.AddEnvironmentVariables();//overwrites previous values

                     if (args != null)
                         config.AddCommandLine(args);
                 });    
    }
}
