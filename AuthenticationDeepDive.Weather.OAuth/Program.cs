using AuthenticationDeepDive.Weather.OAuth.Authorization;
using AuthenticationDeepDive.Weather.OAuth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
    options.DescribeAllParametersInCamelCase();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather App", Version = Assembly.GetEntryAssembly().GetName().Version.ToString(3) });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your Bearer token without 'Bearer'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    var key = new OpenApiSecurityScheme()
    {
        Reference = new OpenApiReference()
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        },
        In = ParameterLocation.Header
    };

    var requirement = new OpenApiSecurityRequirement()
    {
        { key, new List<string>() }
    };

    options.AddSecurityRequirement(requirement);
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // IdentityServer URL
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:5001",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your-256-bit-secretyour-256-bit-secretyour-256-bit-secret")),

        };

    });

builder.Services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AccessPolicyRequirement(AccessPrivilegesEnum.ApplicationAccess))
                    .Build());

builder.Services.AddScoped<IAuthorizationHandler, AccessPolicyRequirementAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, AccessPolicyProvider>();
builder.Services.AddScoped<IUserPriviligeVerificationService, UserPriviligeVerificationService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Configure the HTTP request pipeline.
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentname}/swagger.json";
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"https://localhost:5006/" },
                };
        });
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API");
        c.RoutePrefix = "swagger";
    });

    var option = new RewriteOptions();
    option.AddRedirect("^$", "swagger");
    app.UseRewriter(option);
}



app.MapControllers();

app.Run();

//How to verify authentication and authorization in the controller
//Generate a token from the Identity Provider and use it in the Authorization header of the request to the Location API.
//The Weather API will validate the token
//and check if the user has the required privileges to access the requested resource in the userverificationservice.
