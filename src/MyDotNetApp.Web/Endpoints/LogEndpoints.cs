namespace MyDotNetApp.Web.Endpoints;

public static class LogEndpoints
{
    public static IEndpointRouteBuilder MapLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs")
            .WithTags("Logs")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", (IWebHostEnvironment env, int lines = 100) =>
        {
            var logDir = Path.Combine(env.ContentRootPath, "logs");
            if (!Directory.Exists(logDir))
                return Results.Ok(Array.Empty<string>());

            var latestLog = Directory.GetFiles(logDir, "log-*.txt")
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (latestLog is null)
                return Results.Ok(Array.Empty<string>());

            // Read without locking the file
            using var fs = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            var allLines = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line is not null) allLines.Add(line);
            }

            return Results.Ok(allLines.TakeLast(lines));
        })
        .WithName("GetLogs");

        return app;
    }
}
