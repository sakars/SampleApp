using Microsoft.AspNetCore.Identity;
using SampleApp.DbItems;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication("Identity.Application")
       .AddCookie("Identity.Application", options =>
       {
           options.LoginPath = "/user/login";
           options.LogoutPath = "/user/logout";
           options.Cookie.Name = "Identity.Application";
       });

builder.Services.AddAuthorization();


builder.Services.AddScoped<IUserStore<SampleUser>, SampleUserStore>(serviceProvider =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection") ?? "Data Source=sampleapp.db";
    return new SampleUserStore(connectionString);
});
builder.Services.AddIdentityCore<SampleUser>(options =>
{
    // Configure password rules if you like
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddSignInManager()
    .AddDefaultTokenProviders();
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
