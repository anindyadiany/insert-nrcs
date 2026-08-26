using Insert.Web.Components;
using Insert.Web.Components.Account; 
using Insert.Infrastructure;
using Insert.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Insert.Application.Stories;
using Insert.Infrastructure.Stories;
using Insert.Media;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<InsertDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<InsertDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCascadingAuthenticationState();

//1
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
//2
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<StoryWorkflowService>();
builder.Services.AddScoped<StoryService>();
//3
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<AssignmentService>();
//4
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<AuditLogService>();
//5
builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
builder.Services.AddScoped<ScriptService>();
//6
builder.Services.AddScoped<IUserLookupService, UserLookupService>();
//7
builder.Services.AddScoped<IIngestRepository, IngestRepository>();
builder.Services.AddScoped<IngestService>();
//8
builder.Services.AddScoped<IMediaProcessor, FfmpegMediaProcessor>();
//9
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<ApprovalService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await Insert.Infrastructure.Identity.IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapAdditionalIdentityEndpoints();   // ← NEW

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/media-thumbnail/{id:guid}", async (Guid id, InsertDbContext db) =>
{
    var asset = await db.MediaAssets.FindAsync(id);
    if (asset?.ThumbnailPath is null || !File.Exists(asset.ThumbnailPath))
        return Results.NotFound();

    var bytes = await File.ReadAllBytesAsync(asset.ThumbnailPath);
    return Results.File(bytes, "image/jpeg");
});

app.Run();

