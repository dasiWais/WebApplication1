using FormsApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<FormsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("FormsDatabase")));
builder.Services.AddCors(options => options.AddPolicy("Angular", policy =>
    policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FormsDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Angular");

app.UseAuthorization();

app.MapControllers();

app.Run();
