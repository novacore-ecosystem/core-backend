// Placeholder entry point - only wired enough for `dotnet ef` design-time tooling to build this
// project during the Persistence phase. Replaced with the full composition root in the API phase.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.Run();
