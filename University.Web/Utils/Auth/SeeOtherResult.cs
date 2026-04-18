namespace University.Web.Utils.Auth;

internal sealed class SeeOtherResult(string location) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status303SeeOther;
        httpContext.Response.Headers.Location = location;
        return Task.CompletedTask;
    }
}