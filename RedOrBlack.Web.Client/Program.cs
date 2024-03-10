using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using RedOrBlack.ViewModels;
using RedOrBlack.Web.Client.Data;
using RedOrBlack.Web.Client.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
