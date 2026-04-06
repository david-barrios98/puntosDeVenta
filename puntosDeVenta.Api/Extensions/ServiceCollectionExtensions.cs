using FluentValidation;
using puntosDeVenta.Application.Ports.Inbound;
using puntosDeVenta.Application.Ports.Outbound;
using puntosDeVenta.Application.UseCases;
using puntosDeVenta.Application.Validators;
using puntosDeVenta.Infrastructure.Messaging;
using puntosDeVenta.Infrastructure.Persistence.Adapters;
using puntosDeVenta.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;

namespace puntosDeVenta.Api.Extensions;

/// <summary>
/// Extensiones de servicios para inyección de dependencias
/// Organiza los registros siguiendo arquitectura hexagonal
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra los casos de uso (Puertos de entrada - Inbound)
    /// </summary>
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<IRegisterSaleUseCase, RegisterSaleUseCase>();

        return services;
    }

    /// <summary>
    /// Registra los puertos de salida (Repositorios y Servicios - Outbound)
    /// </summary>
    public static IServiceCollection AddApplicationPorts(this IServiceCollection services, IConfiguration configuration)
    {

        // Inventario
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IGetCriticalInventoryUseCase, GetCriticalInventoryUseCase>();

        // Ventas (Fase 2)
        services.AddScoped<ISalesRepositoryAdapter, VentasRepositoryAdapter>();

        // ============== MESSAGE PUBLISHER ==============
        // Detectar si usamos RabbitMQ real o mock
        var useRealRabbitMq = configuration.GetValue<bool>("UseRealRabbitMq", false);
        services.AddScoped<IMessagePublisher, MockMessagePublisher>();


        return services;
    }

    /// <summary>
    /// Registra los validadores (FluentValidation)
    /// </summary>
    public static IServiceCollection AddApplicationValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterSaleValidator>();

        return services;
    }
}