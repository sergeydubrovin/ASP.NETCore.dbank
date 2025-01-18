﻿using dbank.Application.Abstractions;
using dbank.Application.Services;
using dbank.Domain;
using dbank.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging;

namespace dbank.Web.Extensions
{
    public static class ServiceCollectionsExtensions
    {
        public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpLogging(option =>
                option.LoggingFields = HttpLoggingFields.Duration);

            return builder;
        }

        public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1"
                });

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            return builder;
        }

        public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<BankDbContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Db")));

            return builder;
        }

        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICustomersService, CustomersService>();
            builder.Services.AddScoped<IPaymentsService, PaymentsService>();
            builder.Services.AddScoped<IBalancesService, BalancesService>();
            builder.Services.AddScoped<ICashDepositsService, CashDepositsService>();
            builder.Services.AddScoped<ICreditsService, CreditsService>();

            return builder;
        }

        public static WebApplicationBuilder AddIntegrationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICurrencyImporter, CurrencyImporter>();
            
            return builder;
        }

        public static WebApplicationBuilder AddHttpClients(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient("Cb", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Integrations:CbImporter:BaseUrl"]!);
            });
            
            return builder;
        }
        
        public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<CbOptions>(builder.Configuration.GetSection("Integrations:CbImporter"));
            
            return builder;
        }
    }
}
