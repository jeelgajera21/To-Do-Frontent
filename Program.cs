using Hangfire;
using To_Do_UI;
using To_Do_UI.Models;

#region create builder
var builder = WebApplication.CreateBuilder(args);
#endregion

#region Hangfire
// Add Hangfire services to the container
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Data Source=MASCOT\\SQLEXPRESS;Initial Catalog=ToDoApp;Integrated Security=true;Encrypt=True;TrustServerCertificate=True")); // or another storage provider

builder.Services.AddHangfireServer();  // Add Hangfire background job server
#endregion

#region ViewsController
// Add services to the container.
builder.Services.AddControllersWithViews();
#endregion

#region MailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
#endregion

builder.Services.AddDistributedMemoryCache();

#region ApiAuthBearer injection
// Register IHttpContextAccessor (if not already registered)
builder.Services.AddHttpContextAccessor();

// Register HttpClientFactory
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebApiBaseUrl"]);
});

// Register ApiAuthBearer as a Singleton
builder.Services.AddSingleton<ApiAuthBearer>();

#endregion

#region Session
// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
#region HangfireDashboard
app.UseHangfireDashboard();// 🔹 Hangfire Dashboard to monitor jobs
app.UseHangfireServer();     // 🔹 Enables Hangfire job execution
#endregion

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
