using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xiphos.Areas.Administration.Validation;
using Xiphos.Credentials;
using Xiphos.Data.ProductDatabase;
using Xiphos.Data.ServiceDatabase;
using Xiphos.Shared.Authentication;

namespace Xiphos
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // --Notable--
            // Environment sensitive configuration
            // There are two options how to configure services based on the environment type.
            //  a) Store the environment type as private property
            //  b) Naming convention for startup class e.g. StartupDevelopment
            // 
            //  Ad b), most of the code would be the same which would lead to a base class
            //  which is less readable. Therefore a) seems to be a better option in our case.
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // --Notable--
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add service credentials
            if (_environment.IsDevelopment())
            {
                services.AddDevelopmentServiceCredentials();
            }
            else
            {
                // --Notable--
                // We distinguish only development and production assuming staging and production is the same.
                // Keep in mind that doesn't have to be true necessarily.
                // In more complicated scenarios other type of environments may exist.
                services.AddProductionServiceCredentials();
            }

            ConfigureDatabaseContexts(services);

            // --Notable--
            // Database developer exception page reveals shows EF migrations issues
            // like when the database in not in correct version.
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;

                    // --Notable--
                    // Identity options are quite rich by default.
                    // Here you can for instance configure your password validation rules.
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;

                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ServiceDbContext>();

            services.AddControllersWithViews();

            AddCustomValidation(services);
        }

        // --Notable--
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                // Developer exception page shows debug information about exception.
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Middleware redirecting HTTP traffic to an HTTPS endpoint
            app.UseHttpsRedirection();

            // Static files are common for websites (*.JS, *.CSS, ...) and uncommon for API servers.
            app.UseStaticFiles();

            // Adds endpoint routing middleware that associates incoming requests with endpoints
            // based on given routing rules.
            app.UseRouting();

            // Middleware for user authentication
            app.UseAuthentication();

            // Middleware for access rights and polices
            app.UseAuthorization();

            // Endpoints and routing rules
            app.UseEndpoints(endpoints =>
            {
                // --Notable--
                //  Routing constraints allow to specify that a certain part of the route will be binded
                //  as argument. In our case it is the Id parameter that will be mapped as action argument.
                //  There are constraints you can apply to this argument.
                //
                //   area:exists
                //      The routing matches only existing areas
                //
                //   ?
                //      The parameter is optional and does not have to be specified. For instance action
                //      Index does not require Id. On the other hand Edit does. Edit action argument
                //      is "int id" and if you call Melody/Edit without the Id the routing will match
                //      but the conversion from null to int will thro exception. Action parameter could
                //      be a nullable type though if this is by design.
                //
                //   :int
                //      The parameter has to be an integer
                //
                //   :alpha
                //      The parameter has to be an alphanumerical string


                // --Notable--
                // Routing is order-dependent, area routing has to be defined BEFORE more specific routing.
                // https://docs.microsoft.com/cs-cz/aspnet/core/mvc/controllers/areas?view=aspnetcore-5.0
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Melody}/{action=Index}/{id?}");

                // Default controller
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Razor pages mapping
                endpoints.MapRazorPages();
            });

            EnsureDefaultRoles(serviceProvider).Wait();
        }

        private static void ConfigureDatabaseContexts(IServiceCollection services)
        {
            // Configure service DB context
            services.AddDbContext<ServiceDbContext>((sc, options) =>
            {
                // Get instance of service credential store
                var credentialStore = sc.GetRequiredService<IServiceCredentialStore>();

                // Retrieve the real connection string
                options.UseSqlServer(credentialStore.GetServiceDatabaseConnectionString());

                // -- Notable --
                //  Selecting the database implementation happens here.
                //  Addin a package reference to MySQL data provider, I'd be able
                //  to call UseMySQL( ... ) where I would pass given connection string.
                //  Note that migrations are data provider specific and cannot be 
                //  used once you do the switch.
                //  You can easily create a fresh one though using the command Add-Migration
            });

            // Configure product DB context
            services.AddDbContext<ProductDbContext>((sc, options) =>
            {
                var credentialStore = sc.GetRequiredService<IServiceCredentialStore>();
                options.UseSqlServer(credentialStore.GetProductDatabaseConnectionString());
            });
        }

        // Extend the existing validation attribute adapters by our own with a new provider.
        private static void AddCustomValidation(IServiceCollection services)
        {
            services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();
        }

        private static async Task EnsureDefaultRoles(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthorizationSetup");

            // --Notable--
            // This methods does a runtime insert of default methods into the identity database.
            // Such data are most likely to be seeded in production but for the sake of demonstration
            // of role and user management we will do it in the runtime.

            // Add missing roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in UserRoles.AllRoles)
            {
                logger.LogDebug("Checking existence of user role: {role}", role);

                if (await roleManager.RoleExistsAsync(role)) continue;

                var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (result.Succeeded)
                {
                    logger.LogInformation("Created user role: {role}", role);
                }
                else
                {
                    logger.LogWarning("Failed to create user role: {role}", role);
                }
            }

            // Ensure all users have at least the user role. Again this is not a good
            // practice to enumerate all users in runtime, just demonstrating the user manager.
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var user in userManager.Users.ToList()) // finish enumeration transaction first
            {
                var isUser = await userManager.IsInRoleAsync(user, UserRoles.User);

                if (isUser) continue;

                logger.LogWarning("Assigning role {role} to user {user}", UserRoles.User, user.UserName);
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }
        }
    }
}
