//using Microsoft.AspNetCore.DataProtection;
//using System.Security.Claims;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

//////////////////////////////

using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Reverse Proxy and Authentication
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("auth", p => p.RequireAuthenticatedUser());
});

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "Cookies";
    o.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", o =>
{
    o.Cookie.Name = ".myapp";
    o.Cookie.Domain = "localhost";
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.DataProtectionProvider = DataProtectionProvider.Create("yarp-test");
})
.AddOpenIdConnect("oidc", o =>
{
    o.Authority = "https://localhost:7291/auth";
    o.ClientId = "interactive";
    o.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
    o.ResponseType = "code";
    o.SaveTokens = true;
    o.Scope.Clear();
    o.Scope.Add("openid");
    o.Scope.Add("profile");
    o.GetClaimsFromUserInfoEndpoint = true;
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

app.MapControllers();

// Configure Reverse Proxy Endpoints
app.MapGet("/", () => "Hi, try going to /auth or /app");
//app.MapGet("/proxy/has-user", (ClaimsPrincipal user) => user.GetName())
//    .RequireAuthorization();
app.MapGet("/proxy/has-user", (ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name))
    .RequireAuthorization();

app.MapReverseProxy();

app.Run();