using AuthenticationDeepDive.Weather.OpenIdConnect.Authentication;
using AuthenticationDeepDive.Weather.OpenIdConnect.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

//Authentication
var azureAdOptions = builder.Configuration.GetSection(AzureAdOptions.Name).Get<AzureAdOptions>()
                ?? throw new InvalidOperationException($"Configuration section '{AzureAdOptions.Name}' is missing or invalid.");

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, AzureAdOptions.Name)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDistributedTokenCaches();

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, AzureAdOptions.Name, JwtBearerDefaults.AuthenticationScheme);

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, ticketStore) =>
    {
        options.Cookie.Name = "__location-api-server";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.Path = "/";

        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;

        options.SessionStore = ticketStore;

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = azureAdOptions.Authority;
    options.ClientId = azureAdOptions.ClientId;
    options.ClientSecret = azureAdOptions.ClientSecret;
    options.CallbackPath = azureAdOptions.CallbackPath;
    options.SignedOutCallbackPath = azureAdOptions.SignedOutCallbackPath;
    options.MetadataAddress = azureAdOptions.MetadataAddress;

    options.CorrelationCookie.Path = "/";
    options.CorrelationCookie.Domain = azureAdOptions.CookieDomain;

    options.NonceCookie.Path = "/";
    options.NonceCookie.Domain = azureAdOptions.CookieDomain;

    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;

    options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(1);
    options.GetClaimsFromUserInfoEndpoint = true;

    //Handle if access is denied or token validation fails
    //options.Events.OnAccessDenied = async context =>
    //{
    //    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    //    var error = new { message = "Access denied.", details = context.ProtocolMessage.ErrorDescription };
    //    await context.HttpContext.Response.WriteAsJsonAsync(error);
    //    context.HandleResponse();
    //};

    //Handle what to do after token is validates, e.g., you can add custom claims or perform additional checks
    //options.Events.OnTokenValidated = async context =>
    //        HandleTokenValidated(context, authorizationSettings);

    options.Events.OnRemoteFailure = async context =>
    {
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        var error = new { message = "Authentication failed.", details = context.Failure?.Message };
        await context.HttpContext.Response.WriteAsJsonAsync(error);
        context.HandleResponse();
    };
});

builder.Services.AddSingleton<ITicketStore, DistributedCacheTicketStore>();



//Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.ReadAccess, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AuthRoles.LocationsReadOnly, AuthRoles.LocationsReadWrite);
        policy.RequireClaim("group", "locationapi.read");
    });

    options.AddPolicy(AuthPolicies.ReadWriteAccess, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AuthRoles.LocationsReadWrite);
        policy.RequireClaim("group", "locationapi.readwrite");
    });

    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(AuthRoles.LocationsReadOnly, AuthRoles.LocationsReadWrite)
        .Build();
});


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
