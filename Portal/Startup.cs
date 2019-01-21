using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telefrek.LDAP;
using Lucent.Common.Hubs;
using Lucent.Common;
using Prometheus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Telefrek.LDAP.Managers;
using Lucent.Portal;

namespace Portal
{
    public class PortalStartup
    {
        public PortalStartup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMediaScanner(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc().AddRazorOptions(options =>
            {
                options.PageViewLocationFormats.Add("/Pages/Partials/{0}.cshtml");
            });

            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddScoped<ICampaignUpdateContext, CampaignUpdateContext>();

            // Add the ldap auth
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                });

                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.Cookie.HttpOnly = false;
                    options.Cookie.Path = "/";
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                });
                services.AddLucentServices(Configuration, true, includePortal: true);
                services.AddSingleton<ILDAPUserManager, LocalLDAPUserManager>();
            }
            else
            {
                services.AddLDAPAuth(Configuration);
                services.AddLucentServices(Configuration, includePortal: true);
            }

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
            app.UseLDAPAuth();

            app.UseMetricServer();
            app.UseApiLatency();

            app.UseMvc();
            app.UseSignalR(routes =>
            {
                routes.MapHub<CampaignHub>("/campaignHub");
            });
        }
    }
}
