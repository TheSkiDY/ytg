using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using YTGsr;
using YTGsr.Hubs;

internal class Program
{
    private static void Main(string[] args)
    {
        Manager manager = new Manager();
        var builder = WebApplication.CreateBuilder(args);
        

        builder.Services.AddSignalR();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        builder.Services.AddSingleton<IDictionary<string, UserConnection>>(options => new Dictionary<string, UserConnection>());
        builder.Services.AddSingleton<IManager>(options => manager);
        
        var app = builder.Build();

        var hubContext = app.Services.GetService(typeof(IHubContext<MainHub, IMainHub>));
        manager.SetHubContext(hubContext as IHubContext<MainHub, IMainHub>);
        app.MapGet("/", () => "Hello World!");
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<MainHub>("/room");
        });

        app.Run();
    }
}