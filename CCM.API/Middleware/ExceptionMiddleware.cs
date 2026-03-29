namespace CCM.API.Middleware;
public class ExceptionMiddleware {
    private readonly RequestDelegate _next; private readonly ILogger<ExceptionMiddleware> _log;
    public ExceptionMiddleware(RequestDelegate n, ILogger<ExceptionMiddleware> l){_next=n;_log=l;}
    public async Task InvokeAsync(HttpContext ctx) {
        try{await _next(ctx);}
        catch(Exception ex){_log.LogError(ex,"Unhandled");ctx.Response.StatusCode=500;ctx.Response.ContentType="application/json";await ctx.Response.WriteAsJsonAsync(new{error=ex.Message});}
    }
}