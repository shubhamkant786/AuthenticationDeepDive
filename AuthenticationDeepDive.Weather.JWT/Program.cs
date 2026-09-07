using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddPolicyScheme(JwtBearerDefaults.AuthenticationScheme, "JAuthScheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") is true)
        {
            JwtSecurityTokenHandler JwtHandler = new();
            var token = authHeader["Bearer ".Length..].Trim();
            try
            {
                if (JwtHandler.CanReadToken(token))
                {
                    var issuer = JwtHandler.ReadJwtToken(token).Issuer;
                    if (issuer.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) ||
                        issuer.Contains("sts.windows.net", StringComparison.OrdinalIgnoreCase))
                    {
                        return "entraIdScheme";
                    }
                    else if (issuer.Contains("okta.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return "oktajwtScheme";
                    }
                    else if (issuer.Contains("keycloak", StringComparison.OrdinalIgnoreCase))
                    {
                        return "keycloakOAuthScheme";
                    }
                    else if (issuer.Contains("accounts.google.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return "googleScheme";
                    }
                    else
                    {
                        return "legacyJwtScheme";
                    }
                }
                return "legacyJwtScheme";
            }
            catch
            {
                // Fall through to legacy scheme if the token cannot be read
                return "legacyJwtScheme";
            }            
        }
        else
        {
            // If no Authorization header is present, default to legacy scheme
            return "legacyJwtScheme";
        }
    };
})
.AddJwtBearer("legacyJwtScheme", options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.RequireHttpsMetadata = Convert.ToBoolean(builder.Configuration["Authentication:RequireHttpsMetadata"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authentication:Authority"],
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SigningKey"])),
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => throw context.Exception,
    };
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentname}/swagger.json";
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"https://localhost:5003/" },
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

app.MapControllers();

app.Run();
