using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    // Basic configuration
    var oidcConfig = builder.Configuration.GetSection("Authentication:OpenIDConnect");
    options.Authority = oidcConfig["Instance"] + oidcConfig["TenantId"];
    options.ClientId = oidcConfig["ClientId"];
    options.ClientSecret = oidcConfig["ClientSecret"];
    options.CallbackPath = oidcConfig["CallbackPath"];

    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    // Request standard claims
    options.Scope.Add("User.Read");
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.Events = new OpenIdConnectEvents
    {
        //OnTokenValidated = context =>
        //{
        //    // Get all claims
        //    var allClaims = context.Principal.Claims
        //        .Select(c => new { c.Type, c.Value })
        //        .ToList();

        //    // Log to console (you'll see this in debug output)
        //    Console.WriteLine("=== ALL USER CLAIMS ===");
        //    foreach (var claim in allClaims)
        //    {
        //        Console.WriteLine($"{claim.Type}: {claim.Value}");
        //    }

        //    // If you want to save to a file
        //    File.WriteAllText("user_claims.json",
        //        JsonSerializer.Serialize(allClaims, new JsonSerializerOptions { WriteIndented = true }));

        //    return Task.CompletedTask;
        //}
        OnTokenValidated = async context =>
        {
            var accessToken = context.TokenEndpointResponse?.AccessToken;

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var graphResponse = await http.GetStringAsync("https://graph.microsoft.com/v1.0/me");
            var graphData = JsonSerializer.Deserialize<JsonElement>(graphResponse);

            // Combine with claims and other data
            var output = new
            {
                basicClaims = context.Principal.Claims.Select(c => new { c.Type, c.Value }),
                graphData,
                accessToken,
                idToken = context.TokenEndpointResponse?.IdToken,
                expiresIn = context.TokenEndpointResponse?.ExpiresIn,
                approvedScopes = context.TokenEndpointResponse?.Scope
            };

            var json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("=== FULL SSO RESPONSE ===");
            Console.WriteLine(json);

            await File.WriteAllTextAsync("sso_full_response.json", json);
        }



    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
