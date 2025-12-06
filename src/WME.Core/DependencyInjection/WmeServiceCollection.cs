namespace WME.Core.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WME.Core.Configuration;
using WME.Core.Files;
using WME.Core.Resources;

/// <summary>
/// Extension methods for registering WME core services with dependency injection.
/// </summary>
public static class WmeServiceCollection
{
    /// <summary>
    /// Adds all WME core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="gameDirectory">Root game directory path.</param>
    /// <param name="configFilePath">Optional configuration file path.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWmeCore(
        this IServiceCollection services,
        string gameDirectory,
        string? configFilePath = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(gameDirectory))
            throw new ArgumentException("Game directory cannot be null or empty", nameof(gameDirectory));

        // Logging (configured by host application)
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddDebug();
            configure.SetMinimumLevel(LogLevel.Debug);
        });

        // Configuration
        services.AddSingleton<IWmeSettings>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<WmeSettings>>();
            return new WmeSettings(logger, configFilePath);
        });

        // File Management
        services.AddSingleton<IWmeFileManager>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<WmeFileManager>>();
            return new WmeFileManager(logger, gameDirectory);
        });

        // Resource Management
        services.AddSingleton<IWmeResourceManager>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<WmeResourceManager>>();
            var fileManager = provider.GetRequiredService<IWmeFileManager>();
            return new WmeResourceManager(logger, fileManager);
        });

        return services;
    }

    /// <summary>
    /// Adds WME graphics services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWmeGraphics(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Graphics services will be added in Phase 2
        // Placeholder for future implementation

        return services;
    }

    /// <summary>
    /// Adds WME audio services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWmeAudio(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Audio services will be added in Phase 3
        // Placeholder for future implementation

        return services;
    }

    /// <summary>
    /// Adds WME scripting services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWmeScripting(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Scripting services will be added in Phase 4
        // Placeholder for future implementation

        return services;
    }

    /// <summary>
    /// Adds all WME services (shortcut for adding all subsystems).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="gameDirectory">Root game directory path.</param>
    /// <param name="configFilePath">Optional configuration file path.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWintermuteEngine(
        this IServiceCollection services,
        string gameDirectory,
        string? configFilePath = null)
    {
        return services
            .AddWmeCore(gameDirectory, configFilePath)
            .AddWmeGraphics()
            .AddWmeAudio()
            .AddWmeScripting();
    }
}
