using Microsoft.OpenApi.Models;
using Refit;
using SoundClound.API.Helpers;
using SoundClound.API.Interfaces;
using SoundClound.API.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{

    services.AddAutoMapper(typeof(Mapping));

    services.AddControllers();
    services.AddRefitClient<IAPIService>().ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(configuration.GetValue<string>("UrlApi2"));
    });

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "Download SoundClound - V1",
                Version = "v1"
            });
    });

    services.AddScoped<IMusicService, MusicService>();

    services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
    });
}
