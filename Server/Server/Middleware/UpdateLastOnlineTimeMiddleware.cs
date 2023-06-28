using System.Security.Claims;

using Google.Cloud.Firestore;

namespace Server.Middleware
{
    public class UpdateLastOnlineTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public UpdateLastOnlineTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, FirestoreDb firestoreDb)
        {
            var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var userDocSnapshot = await firestoreDb.Collection(CollectionConstants.UsersCollection).Document(userId).GetSnapshotAsync();
                if (userDocSnapshot.Exists)
                {
                    var user = userDocSnapshot.Reference.UpdateAsync(FieldConstants.LastOnlineTime, FieldValue.ServerTimestamp);
                }
            }

            await _next(httpContext);
        }
    }

    public static class UpdateLastOnlineTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseUpdateLastOnlineTimeMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UpdateLastOnlineTimeMiddleware>();
        }
    }
}
