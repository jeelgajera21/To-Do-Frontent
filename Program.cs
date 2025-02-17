using Hangfire;
using To_Do_UI;
using To_Do_UI.Models;


var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services to the container
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Data Source=MASCOT\\SQLEXPRESS;Initial Catalog=ToDoApp;Integrated Security=true;Encrypt=True;TrustServerCertificate=True")); // or another storage provider

builder.Services.AddHangfireServer();  // Add Hangfire background job server

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddDistributedMemoryCache();

// Register IHttpContextAccessor (if not already registered)
builder.Services.AddHttpContextAccessor();

// Register HttpClientFactory
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiBaseUrl"]);
});

// Register ApiAuthBearer as a Singleton
builder.Services.AddSingleton<ApiAuthBearer>();


// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHangfireDashboard();// 🔹 Hangfire Dashboard to monitor jobs
app.UseHangfireServer();     // 🔹 Enables Hangfire job execution

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // ✅ Enable session
app.UseAuthentication(); // If using authentication
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
