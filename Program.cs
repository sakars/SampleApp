using Microsoft.AspNetCore.Identity;
using SampleApp.DbItems;

var builder = WebApplication.CreateBuilder(args);

//--- Add services to the container. ---

// Add Razor Pages services
builder.Services.AddRazorPages();

// Implement authentication and identity services
builder.Services.AddAuthentication("Identity.Application")
       .AddCookie("Identity.Application", options =>
       {
           options.LoginPath = "/user/login";
           options.LogoutPath = "/user/logout";
           options.Cookie.Name = "Identity.Application";
       });

builder.Services.AddAuthorization();
builder.Services.AddScoped<SampleUserStore>(serviceProvider =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection") ?? "Data Source=sampleapp.db";
    return new SampleUserStore(connectionString);
});
builder.Services.AddScoped<IUserStore<SampleUser>>(serviceProvider =>
    serviceProvider.GetRequiredService<SampleUserStore>());

builder.Services.AddIdentityCore<SampleUser>(options =>
{
    // Configure password rules if you like
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Add GameDataStore
builder.Services.AddTransient<GameDataStore>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
