namespace Lampyris.ResourceServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            var resourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

            // ȷ����ԴĿ¼����
            if (!Directory.Exists(resourceDirectory))
            {
                Directory.CreateDirectory(resourceDirectory);
            }

            app.UseEndpoints(endpoints =>
            {
                // �ṩ�汾��Ϣ
                endpoints.MapGet("/version", async context =>
                {
                    var versionFile = Path.Combine(resourceDirectory, "version.json");
                    if (!File.Exists(versionFile))
                    {
                        await context.Response.WriteAsync("{}");
                        return;
                    }

                    var versionJson = await File.ReadAllTextAsync(versionFile);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(versionJson);
                });

                // �ṩ��Դ�ļ�����
                endpoints.MapGet("/download/{fileName}", async context =>
                {
                    var fileName = context.Request.RouteValues["fileName"]?.ToString();
                    var filePath = Path.Combine(resourceDirectory, fileName);

                    if (!File.Exists(filePath))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsync("File not found");
                        return;
                    }

                    context.Response.ContentType = "application/octet-stream";
                    await context.Response.SendFileAsync(filePath);
                });
            });
        }
    }
}
