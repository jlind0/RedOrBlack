using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using RedOrBlack.ViewModels;
using RedOrBlack.Web.Components;
using RedOrBlack.Web.Components.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(provider =>
{
    var acct = new Account(builder.Configuration["Infura:SecretKey"] ?? throw new InvalidOperationException());
    var w3 = new Web3(acct, builder.Configuration["Infura:Endpoint"] ?? throw new InvalidDataException());

    return w3;
});
builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<AlertView.AlertViewModel>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
