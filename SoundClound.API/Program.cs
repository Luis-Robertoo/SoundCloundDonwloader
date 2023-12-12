using Refit;
using SoundClound.API.Helpers;
using SoundClound.API.Interfaces;
using SoundClound.API.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
    services.AddSwaggerGen();

    services.AddScoped<IMusicService, MusicService>();
}
