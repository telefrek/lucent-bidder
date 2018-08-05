using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Portal.Data;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telefrek.LDAP;
using Lucent.Portal.Hubs;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Newtonsoft.Json;

namespace Portal
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<PortalDbContext>(options =>
                              options.UseInMemoryDatabase("local"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc().AddRazorOptions(options =>
            {
                options.PageViewLocationFormats.Add("/Pages/Partials/{0}.cshtml");
            });

            services.AddScoped<ICampaignUpdateContext, CampaignUpdateContext>();

            // Add the ldap auth
            //services.AddLDAPAuth(Configuration);
            services.AddSignalR();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseLDAPAuth();

            var routeBuilder = new RouteBuilder(app);

            app.UseMvc();
            app.UseSignalR(routes =>
            {
                routes.MapHub<CampaignHub>("/campaignHub");
            });

            routeBuilder.MapPost("/v1/callback", async (context) =>
            {
                try
                {
                    var req = context.Request;
                    var id = Guid.Empty;
                    var amount = 0d;
                    using (var jsonReader = new JsonTextReader(new StreamReader(req.Body)))
                    {
                        while (await jsonReader.ReadAsync())
                        {
                            switch (jsonReader.TokenType)
                            {
                                case JsonToken.StartArray:
                                    await jsonReader.SkipAsync();
                                    break;
                                case JsonToken.StartObject:
                                    while (await jsonReader.ReadAsync())
                                    {
                                        if (jsonReader.TokenType == JsonToken.EndObject)
                                            break;
                                        if (jsonReader.TokenType == JsonToken.PropertyName)
                                        {
                                            switch (jsonReader.Value.ToString().ToLowerInvariant())
                                            {
                                                case "id":
                                                    id = Guid.Parse(await jsonReader.ReadAsStringAsync());
                                                    break;
                                                case "cpm":
                                                    amount = (await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault(0d);
                                                    break;
                                                default:
                                                    await jsonReader.SkipAsync();
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    await jsonReader.SkipAsync();
                                    break;
                            }
                        }
                    }

                    if (id != Guid.Empty)
                    {
                        await context.RequestServices.GetRequiredService<ICampaignUpdateContext>().UpdateCampaignSpendAsync(id, amount, CancellationToken.None);
                        context.Response.StatusCode = 202;
                    }
                    else
                        context.Response.StatusCode = 204;
                }
                catch
                {
                    context.Response.StatusCode = 204;
                }
            });

            app.UseRouter(routeBuilder.Build());
        }
    }
}
