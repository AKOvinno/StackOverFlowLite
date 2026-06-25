using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StackOverflowLite.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StackOverflow Lite API",
                Version = "v1",
                Description = "A simplified Q&A platform API built with Clean Architecture, MediatR, and ASP.NET Core."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token below. Example: Bearer {your_token}"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            options.OperationFilter<DefaultResponseCodesFilter>();
        });

        return services;
    }
}

public class DefaultResponseCodesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses?.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
        operation.Responses?.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses?.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        operation.Responses?.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
    }
}

