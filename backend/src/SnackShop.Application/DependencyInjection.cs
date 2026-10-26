using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SnackShop.Application.Behaviors;
using SnackShop.Application.Common.Catalog;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Application.Features.Orders.PlaceOrder;

namespace SnackShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Order matters: outermost first.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Helpers shared by several handlers (scoped: they use the request's DbContext and user).
        services.AddScoped<AuthSessionIssuer>();
        services.AddScoped<AccountEmails>();
        services.AddScoped<CustomerProfiles>();
        services.AddScoped<CartAccess>();
        services.AddScoped<PurchasableVariants>();
        services.AddScoped<OrderInventory>();
        services.AddScoped<OrderLineFactory>();
        services.AddScoped<BankTransferInstructions>();
        services.AddScoped<StaffOrderAccess>();

        return services;
    }
}
