using System.Security.Claims;
using Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR().AddAzureSignalR(options =>
{
    options.ClaimsProvider = context =>
    [
        // embed whatever you need into the token claims
        new Claim("resourceId", context.Request.Headers["X-Tenant-Id"].ToString()),
        new Claim("userId",context.Request.Headers["X-User-Id"].ToString()),
    ];
});
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapGet("/api/v1/users", () =>
{
    return Results.Ok(AvailableConnection.GetAllUserIds());
});

app.MapPost("/api/v1/feedback", async (
    [FromBody] Feedback feedback,
    [FromServices] IHubContext<NotificationHub> hubContext) =>
{
    var connectionId = AvailableConnection.GetConnection(feedback.UserId);
    await hubContext.Clients.Client(connectionId).SendAsync("Notification", $"{feedback.Comment} is commented");
    return Results.Ok();
});

app.Run();

record Feedback
{
    public required string ResourceId { get; set; }

    public required string UserId { get; set; }

    public required string Comment { get; set; }
}

