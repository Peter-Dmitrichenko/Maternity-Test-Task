using BL;
using DB;
using DTO;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(opts => 
opts.UseSqlServer(connectionString, sql => {
    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    sql.CommandTimeout(60); }));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PatientProfile>()); 
builder.Services.AddScoped<GenderIdToCodeResolver>(); 
builder.Services.AddScoped<ActiveIdToCodeResolver>(); 
builder.Services.AddScoped<GenderCodeToIdResolver>(); 
builder.Services.AddScoped<ActiveCodeToIdResolver>();

builder.Services.AddScoped<IDateSearchService, DateSearchService>(); 
builder.Services.AddScoped<IPatientService, PatientService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // This will create the database if it doesn't exist
    dbContext.Database.EnsureCreated();
    await DbInitializer.EnsureLookupsInitializedAsync(dbContext);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
