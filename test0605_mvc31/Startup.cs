using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace test0605_mvc31
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.Use(async (context, next) =>
            {
                string GetSharedFxVersion(Type type)
                {
                    var asmPath = type.Assembly.Location;
                    var versionFile = Path.Combine(Path.GetDirectoryName(asmPath), ".version");

                    var simpleVersion = File.Exists(versionFile) ?
                        File.ReadAllLines(versionFile).Last() :
                        "<unknown>";

                    var infoVersion = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<unknown>";

                    return $"{simpleVersion} ({infoVersion})";
                }

                if (context.Request.Path.StartsWithSegments("/.runtime-info"))
                {
                    context.Response.ContentType = "text/plain";
                    var aspnetCoreVersion = GetSharedFxVersion(typeof(IApplicationBuilder));
                    var netCoreVersion = GetSharedFxVersion(typeof(string));
                    await context.Response.WriteAsync($"ASP.NET Core Runtime version: {aspnetCoreVersion}{Environment.NewLine}");
                    await context.Response.WriteAsync($".NET Core Runtime version: {netCoreVersion}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Process Architecture: {RuntimeInformation.ProcessArchitecture}{Environment.NewLine}");
                }
                else
                {
                    await next();
                }
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
