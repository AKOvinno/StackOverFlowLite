using Microsoft.EntityFrameworkCore;
using StackOverflowLite.API.Extensions;
using StackOverflowLite.API.Middleware;
using StackOverflowLite.Application;
using StackOverflowLite.Infrastructure;
using StackOverflowLite.Infrastructure.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// Layer registrations — strictly ordered, inner-to-outer
builder.Services.AddApplication();
// builder.Services.AddPersistence(builder.Configuration);    // DbContext
builder.Services.AddInfrastructure(builder.Configuration); // Repos, services, Redis, JWT

// API-layer registrations
builder.Services.AddControllers();
builder.Services.AddIdentityWithJwt(builder.Configuration); // Identity + JWT bearer
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StackOverflow Lite API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

