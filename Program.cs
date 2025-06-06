using Microsoft.IdentityModel.Logging;
using PropertiesService.Config;
using PropertiesService.Services;
using PropertiesService.Repositories;
using Microsoft.OpenApi.Models;
using FluentValidation;
using PropertiesService.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

// HotChocolate / GraphQL:
using PropertiesService.GraphQL;
using PropertiesService.GraphQL.Queries;
using PropertiesService.GraphQL.Mutations;
using PropertiesService.GraphQL.Types;
using PropertiesService.Dtos;  // Para los tipos de input

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// Logs JWT detallados (solo para diagnóstico; quitar en producción)
// ─────────────────────────────────────────────────────────────
IdentityModelEventSource.ShowPII = true;

// ─────────────────────────────────────────────────────────────
// Configurar Cassandra
// ─────────────────────────────────────────────────────────────
builder.Services.Configure<CassandraOptions>(
    builder.Configuration.GetSection("Cassandra"));
builder.Services.AddSingleton<CassandraSessionFactory>();

// ─────────────────────────────────────────────────────────────
// Repositorios
// ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<PropertyRepository>();

// ─────────────────────────────────────────────────────────────
// FluentValidation (validador de CreatePropertyRequest)
// ─────────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<CreatePropertyRequestValidator>();

// ─────────────────────────────────────────────────────────────
// Autenticación y Autorización
// ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            NameClaimType = ClaimTypes.NameIdentifier
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine("[JWT] OnMessageReceived: " +
                    (context.Request.Headers.ContainsKey("Authorization")
                        ? context.Request.Headers["Authorization"].ToString()
                        : "(ninguna)"));
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("[JWT] OnTokenValidated – Claims:");
                foreach (var c in context.Principal!.Claims)
                    Console.WriteLine($" • {c.Type} = {c.Value}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("[JWT] OnAuthenticationFailed – " + context.Exception);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("[JWT] OnChallenge – Error: " + context.Error);
                return Task.CompletedTask;
            }
        };
    });

// ─────────────────────────────────────────────────────────────
// Swagger / OpenAPI
// ─────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Properties API", Version = "v1" });

    // Configurar Swagger para aceptar JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escriba 'Bearer {token}' para autenticarse."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────
// GraphQL con HotChocolate
// ─────────────────────────────────────────────────────────────
builder.Services
    .AddGraphQLServer()

    // 1) Clase raíz "Query"
    .AddQueryType<Query>()
    // 2) Extensiones de Query
    .AddTypeExtension<PropertyQuery>()

    // 3) Clase raíz "Mutation"
    .AddMutationType<Mutation>()
    // 4) Extensión de Mutation
    .AddTypeExtension<PropertyMutation>()

    // 5) Tipos de objeto (PropertyType)
    .AddType<PropertyType>()

    // 6) Inputs explícitos
    .AddType<CreatePropertyRequestType>()          // para CreatePropertyRequest
    .AddType<UpdatePropertyInputType>()            // para UpdatePropertyInput
    .AddType<UpdatePropertyStatusInputType>();     // para UpdatePropertyStatusInput

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Montamos GraphQL en "/graphql"
app.MapGraphQL("/graphql");

app.Run();