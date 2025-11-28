using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebAppFrontend;
using WebAppFrontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<BattleshipState>();
builder.Services.AddScoped<ApiClient>();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5001") 
});

builder.Services.AddScoped<AttackGrpcWebClient>(sp =>
{
    var http = sp.GetRequiredService<HttpClient>();
    
    return new AttackGrpcWebClient(http.BaseAddress!.ToString());
});

await builder.Build().RunAsync();