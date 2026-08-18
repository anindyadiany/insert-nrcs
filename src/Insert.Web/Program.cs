using Insert.Web.Components;
using Insert.Web.Components.Account; 
using Insert.Infrastructure;
using Insert.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Insert.Application.Stories;
using Insert.Infrastructure.Stories;

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

app.Run();

