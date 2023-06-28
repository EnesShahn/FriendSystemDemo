using System.Text.Json.Serialization;

using FirebaseAdmin;

using Google.Cloud.Firestore;

using Microsoft.AspNetCore.Authentication;

using Server.Middleware;
using Server.Services.AuthenticaionService;
using Server.Services.FriendshipService;
using Server.Services.GroupService;
using Server.Services.InvitationService;
using Server.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

var imvcBuilder = builder.Services.AddControllers();
imvcBuilder.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();

builder.Services.AddSingleton(FirebaseApp.Create());
builder.Services.AddSingleton(FirestoreDb.Create(builder.Configuration["FirebaseProjectID"]));

var authBuilder = builder.Services.AddAuthentication("Bearer");
authBuilder.AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>("Bearer", null);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseCors(o =>
{
    o.AllowAnyHeader();
    o.AllowAnyMethod();
    o.AllowAnyOrigin();
});

app.UseAuthentication();

app.UseAuthorization();

app.UseUpdateLastOnlineTimeMiddleware();

app.MapControllers();

app.Run();
