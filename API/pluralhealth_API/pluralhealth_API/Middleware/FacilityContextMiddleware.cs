namespace pluralhealth_API.Middleware
{
    public class FacilityContextMiddleware
    {
        private readonly RequestDelegate _next;
        private const string FacilityIdKey = "FacilityId";
        private const string UserIdKey = "UserId";

        public FacilityContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // For MVP: Mock facility and user context
            var facilityId = context.Request.Headers["X-Facility-Id"].FirstOrDefault();
            var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();

            context.Items[FacilityIdKey] = int.TryParse(facilityId, out var fid) ? fid : 1;
            context.Items[UserIdKey] = int.TryParse(userId, out var uid) ? uid : 1;

            await _next(context);
        }
    }

    public static class FacilityContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseFacilityContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FacilityContextMiddleware>();
        }
    }
}

