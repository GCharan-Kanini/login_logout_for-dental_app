using Api.Auth;
using Api.Errors;
using Api.Sessions;
using Api.Staff;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SessionStoreExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISessionStore, SessionStore>();
builder.Services.AddSingleton<ISessionService, SessionService>();

builder.Services
    .AddAuthentication(SessionAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthHandler>(SessionAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapStaffEndpoints();

app.Run();

public partial class Program { }
