using EnumDictDemo.Data;
using EnumDictDemo.Filters;
using EnumDictDemo.Infrastructure;
using EnumDictDemo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DictTranslateOptions>(builder.Configuration.GetSection(DictTranslateOptions.SectionName));

builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDictService, DictService>();
builder.Services.AddScoped<DictTranslationHelper>();
builder.Services.AddScoped<ObjectVisitor>();
builder.Services.AddScoped<DictTranslateFilter>();

builder.Services.AddSingleton<IEnumInfoService, EnumInfoService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<DictTranslateFilter>();
});

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();