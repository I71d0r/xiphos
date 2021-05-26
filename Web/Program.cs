using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Xiphos
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
                    // --Notable-- 
                    // Kestrel uses by default a development certificate but can be also configured
                    // as public facing while utilizing a custom certificate.
                    //
                    // More info on the server certificates configuration:
                    // https://docs.microsoft.com/cs-cz/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-5.0
                    //
                    // Similarly, the Kestrel can be configured to require a client certificate. This applies 
                    // for instance to API servers communicating in a server mesh where the 
                    // authentication is certificate-based.
                    //
                    // More info on certificate-based authentication:
                    // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-5.0

                    // --Notable-- 
                    //  By default Kestrel accepts just localhost calls. UseUrls is one of several ways how to 
                    //  specify what addresses are acceptable.
                    webBuilder.UseUrls("https://*:5001/");
                });
    }
}
