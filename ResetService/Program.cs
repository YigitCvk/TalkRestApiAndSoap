using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using ResetService.Models.Context;
using ResetService.Models.Entity;
using ResetService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();
builder.Services.AddDbContext<Context>();
builder.Services.AddMvc();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(100);
    options.AccessDeniedPath = "/ErrorPage/Index/";
    options.LoginPath = "/Auth/Login/";
});

builder.Services.AddSingleton<WebSocketHandler>();


builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();
//builder.Services.AddScoped<ResourceOwnerPasswordTokenHandler>();
//builder.Services.AddScoped<ClientCredentialTokenHandler>();

//builder.Services.AddScoped<IIdentityService, IdentityService>();
//builder.Services.AddScoped<IClientCredentialTokenService, ClientCredentialTokenService>();

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

app.UseAuthorization();

app.UseWebSockets();
app.Map("/ws", wsApp => wsApp.UseMiddleware<WebSocketMiddleware>());


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
