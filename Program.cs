using Hangfire;
using To_Do_UI;
using To_Do_UI.Models;
using To_Do_UI.Filters;

#region Create Builder
var builder = WebApplication.CreateBuilder(args);
#endregion

#region Hangfire
// 🔹 Configure Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Data Source=MASCOT\\SQLEXPRESS;Initial Catalog=ToDoApp;Integrated Security=true;Encrypt=True;TrustServerCertificate=True"));

builder.Services.AddHangfireServer(); // 🔹 Add Hangfire background job server
#endregion

#region ViewsController
// 🔹 Add controllers with views
builder.Services.AddControllersWithViews();
#endregion

#region MailSettings
// 🔹 Configure mail settings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
#endregion

#region Caching & Session
// 🔹 Add caching
builder.Services.AddDistributedMemoryCache();

// 🔹 Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

#region API Auth & HTTP Client
// 🔹 Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 🔹 Register HttpClientFactory
var webApiBaseUrl = builder.Configuration["WebApiBaseUrl"];
if (string.IsNullOrEmpty(webApiBaseUrl))
{
    throw new InvalidOperationException("WebApiBaseUrl is not configured in appsettings.json");
}

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(webApiBaseUrl);
});

// 🔹 Register ApiAuthBearer as a singleton
builder.Services.AddSingleton<ApiAuthBearer>();
#endregion

var app = builder.Build();

// 🔹 Ensure session middleware is enabled **before** authentication
app.UseSession();

// 🔹 Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Home/PageNotFound");

app.UseAuthentication();
app.UseAuthorization();

#region HangfireDashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireSessionAuthorizationFilter(app.Services.GetRequiredService<ILogger<HangfireSessionAuthorizationFilter>>()) },
     DashboardTitle = "My Custom Hangfire Dashboard",
    AppPath = "javascript:window.history.back();" // 🟢 Uses JavaScript to go back
});
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
