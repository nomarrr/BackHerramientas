using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configurar servicios
        builder.Services.AddSignalR();
        builder.Services.AddControllers();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.SetIsOriginAllowed(origin => true)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });

        var app = builder.Build();

        // Configurar middleware
        app.UseDeveloperExceptionPage();
        app.UseCors();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapControllers();
        });

        // Obtener puerto de la variable de entorno o usar 5000 por defecto
        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        var url = $"http://0.0.0.0:{port}";
        
        Console.WriteLine($"Servidor SignalR en ejecución en {url}/chatHub");
        
        app.Run(url);
    }
}
