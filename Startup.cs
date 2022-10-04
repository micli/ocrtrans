
namespace OcrTrans;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        _config = configuration;
    }
    private static IConfiguration? _config = null;
    public static IConfiguration? Configuration
    {
        get
        {
                return _config;
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddRazorPages();
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (!app.Environment.IsDevelopment())
        {
            // app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();

        }
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        // app.UseStaticFiles();
        // app.UseRouting();
        // app.MapRazorPages();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}