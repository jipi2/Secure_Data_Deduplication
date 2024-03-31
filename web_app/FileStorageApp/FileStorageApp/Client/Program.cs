global using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using FileStorageApp.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7109")});
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddSweetAlert2();

//for file upload
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024; // 1000 MB
});*/

//CORS

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1000 * 1024 * 1024; // 1000 MB
});

await builder.Build().RunAsync();
