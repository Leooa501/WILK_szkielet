using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WILK.Views;
using WILK.Services;
using WILK.Services.Configuration;
using WILK.Services.Repositories;
using System.IO;

namespace WILK
{
    /// <summary>
    /// Application entry point with dependency injection configuration
    /// </summary>
    internal static class Program
    {
        private static ServiceProvider? _serviceProvider;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                // Setup dependency injection with enterprise database
                ConfigureServices();

                // Create and run main form with dependency injection
                var mainForm = _serviceProvider!.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cleanup
                _serviceProvider?.Dispose();
            }
        }

        /// <summary>
        /// Configures dependency injection container with repositories and services
        /// </summary>
        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
            services.AddSingleton(appSettings);

            // Get configuration sections with fallback values
            var connectionStrings = new ConnectionStrings();
            var connectionSection = configuration.GetSection(ConnectionStrings.SectionName);
            if (connectionSection.Exists())
            {
                connectionSection.Bind(connectionStrings);
            }
            else
            {
#if DEBUG 
                connectionStrings.DefaultConnection = "server=vm-pcb-sql.mikronika.com.pl;user=pcb1;password=quaih8eiW2;database=pcb2;";
#elif RELEASE
                connectionStrings.DefaultConnection = "server=vm-pcb-sql.mikronika.com.pl;user=pcb1;password=quaih8eiW2;database=pcb1;";
#elif THT
                connectionStrings.DefaultConnection = "server=vm-pcb-sql.mikronika.com.pl;user=pcb1;password=quaih8eiW2;database=pcb1;";
#endif
            }

            // Register configuration objects
            services.AddSingleton(connectionStrings);

            // Register all repositories with connection string
            services.AddTransient<IComponentRepository>(sp => 
                new ComponentRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IReservationRepository>(sp => 
                new ReservationRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IAlternativeRepository>(sp => 
                new AlternativeRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IWarmUpRepository>(sp => 
                new WarmUpRepository(connectionStrings.DefaultConnection));

            services.AddTransient<ILocationRepository>(sp => 
                new LocationRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IShortageRepository>(sp => 
                new ShortageRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IImportExportRepository>(sp => 
                new ImportExportRepository(connectionStrings.DefaultConnection));
                
            services.AddTransient<IExcessiveUsageRepository>(sp => 
                new ExcessiveUsageRepository(connectionStrings.DefaultConnection));

            services.AddTransient<IErrorsRepository>(sp => 
                new ErrorsRepository(connectionStrings.DefaultConnection));


            // Register enterprise database service (now uses repositories)
            services.AddTransient<IEnterpriseDatabase, EnterpriseDatabase>();

            // Add file processing service
            services.AddTransient<IFileProcessingService, FileProcessingService>();

            // Add views - MainForm creates its own presenter
            services.AddTransient<MainForm>();

            _serviceProvider = services.BuildServiceProvider();
        }
        
        /// <summary>
        /// Load theme preference from appsettings.json
        /// </summary>
        private static void LoadThemePreference()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
            }
            catch
            {
                // If loading fails, use default Light theme
            }
        }
    }
}