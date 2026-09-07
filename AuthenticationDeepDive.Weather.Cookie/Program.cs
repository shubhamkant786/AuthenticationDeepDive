using AuthenticationDeepDive.Weather.Cookie;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

//Get the cookie if it is set from another app- in the blob storage
//This is to ensure that the keys are shared between the apps and the cookies can be validated across apps
var options = new CookieBlobStorageOption();
builder.Configuration.GetSection("CookieBlobStorage").Bind(options);

builder.Services.AddDataProtection()
                .PersistKeysToAzureBlobStorage(options.ConnectionString, options.ContainerName, "__weather_cookie_keys")
                .SetApplicationName("weatherApp")
                .DisableAutomaticKeyGeneration();

builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSingleton<IDistributedCacheService, DistributedCacheService>();


//Enable the Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, ticketStore) =>
    {
        options.Cookie.Name = "__location-api-server";//name of the cookie set from location api service
        options.SessionStore = ticketStore;

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<ITicketStore, DistributedCacheTicketStore>();

builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    options.ForwardLimit = null;
    options.RequireHeaderSymmetry = false;
});


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

//Should be before UseRouting
app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.Always });

app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
