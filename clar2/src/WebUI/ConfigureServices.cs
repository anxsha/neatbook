﻿using clar2.Application.Common.Interfaces;
using clar2.Infrastructure.Persistence;
using clar2.WebUI.Filters;
using clar2.WebUI.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace WebUI;

public static class ConfigureServices {
  public static IServiceCollection AddWebUIServices(this IServiceCollection services) {
    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddScoped<ICurrentUserService, CurrentUserService>();

    services.AddHttpContextAccessor();

    services.AddHealthChecks()
      .AddDbContextCheck<ApplicationDbContext>();

    services.AddControllersWithViews(options =>
        options.Filters.Add<ApiExceptionFilterAttribute>())
      .AddFluentValidation(x => x.AutomaticValidationEnabled = false);

    services.AddRazorPages();

    // Customise default API behaviour
    services.Configure<ApiBehaviorOptions>(options =>
      options.SuppressModelStateInvalidFilter = true);

    services.AddOpenApiDocument(configure => {
      configure.Title = "clar2 API";
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
}