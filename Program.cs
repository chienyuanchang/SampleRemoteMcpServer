using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()            // HTTP / SSE transport
    // .WithTools<GreetingTools>();
    .WithToolsFromAssembly(); // explicitly scan Tools assembly

// Add SignalR services
builder.Services.AddSignalR();

var app = builder.Build();

// optional: health check or other endpoints
app.MapGet("/health", () => "Hello MCP Server");  // use /health instead of /

// map the MCP endpoints
app.MapMcp();   // this hooks up the /sse and /messages paths

// Map the SignalR hub
app.MapHub<SampleRemoteMcpServer.Hubs.ChatHub>("/chatHub");

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
