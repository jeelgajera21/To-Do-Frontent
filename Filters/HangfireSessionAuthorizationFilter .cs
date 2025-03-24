using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace To_Do_UI.Filters
{
    public class HangfireSessionAuthorizationFilter : IDashboardAuthorizationFilter
    {
        #region Ilogger & Filter

        private readonly ILogger<HangfireSessionAuthorizationFilter> _logger;

        public HangfireSessionAuthorizationFilter(ILogger<HangfireSessionAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Authorize
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.Session == null)
            {
                _logger.LogWarning("🚨 Session is null. Returning 404.");
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound; // Send 404 status
                return false;
            }

            var userId = httpContext.Session.GetInt32("UserID");

            if (userId == null || userId != 2)
            {
                _logger.LogWarning("🚨 Unauthorized User! Returning 404.");
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound; // Send 404 status
                return false;
            }

            _logger.LogInformation($"✅ UserId from session: {userId}");
            return true; // Only allow access if UserId == 2
        } 
        #endregion
    }
}
