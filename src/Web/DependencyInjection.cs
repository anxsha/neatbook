﻿using System.Text.Json.Serialization;
using Azure.Identity;
using neatbook.Application.Common.Interfaces;
using neatbook.Infrastructure.Data;
using neatbook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using ZymLabs.NSwag.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection {
  public static IServiceCollection AddWebServices(this IServiceCollection services) {
    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddScoped<IUser, CurrentUser>();

    services.AddHttpContextAccessor();

    services.AddHealthChecks()
      .AddDbContextCheck<ApplicationDbContext>();

    services.AddExceptionHandler<CustomExceptionHandler>();

    services.AddRazorPages();

    services.AddScoped(provider => {
      var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
      var loggerFactory = provider.GetService<ILoggerFactory>();

      return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
    });

    // Customise default API behaviour
    services.Configure<ApiBehaviorOptions>(options =>
      options.SuppressModelStateInvalidFilter = true);

    // Make API return Enums as strings
    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => {
      opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddEndpointsApiExplorer();

    services.AddOpenApiDocument((configure, sp) => {
      configure.Title = "neatbook API";

      // Add the fluent validations schema processor
      var fluentValidationSchemaProcessor =
        sp.CreateScope().ServiceProvider.GetRequiredService<FluentValidationSchemaProcessor>();

      configure.SchemaProcessors.Add(fluentValidationSchemaProcessor);

      // Add JWT
      configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: Bearer {your JWT token}."
      });

      configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
    });

    return services;
  }

  public static IServiceCollection AddKeyVaultIfConfigured(this IServiceCollection services,
    ConfigurationManager configuration) {
    var keyVaultUri = configuration["KeyVaultUri"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri)) {
      configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
    }

    return services;
  }
}