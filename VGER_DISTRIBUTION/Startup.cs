using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using VGER_DISTRIBUTION.Models;
using VGER_DISTRIBUTION.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VGER_DISTRIBUTION.Repositories;
using VGER_DISTRIBUTION.Repositories.Master;
using System.Net;
using Microsoft.AspNetCore.DataProtection;
using NLog.Web;
using NLog;
using NLog.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace VGER_DISTRIBUTION
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _hostingEnvironment = env;
            var builder = new ConfigurationBuilder()
         .SetBasePath(env.ContentRootPath)
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
         .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
         .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                  builder => builder.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["ValidIssuer"],
                        ValidAudience = Configuration["ValidAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]))
                    };
                });
            services.AddMvc();
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });
            services.AddMvc()
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            services.AddScoped<LogFilter>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IMasterRepository, MasterRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IBookingRepository, BookingRepository>();
            //services.AddTransient<IEventRepository, EventRepository>();
            //services.AddTransient<IGenericRepository, GenericRepository>();
            //services.AddTransient<IQuoteRepository, QuoteRepository>();
            //services.AddTransient<IPositionRepository, PositionRepository>();
            //services.AddTransient<IQRFSummaryRepository, QRFSummaryRepository>();
            //services.AddTransient<IGuesstimateRepository, GuesstimateRepository>();
            //services.AddTransient<IItineraryRepository, ItineraryRepository>();

            services.Configure<MongoSettings>(options =>
            {
                options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database = Configuration.GetSection("MongoConnection:Database").Value;
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Distribution API for CKDMS Voyager",
                    Version = "v1",
                    //Description = "Distribution API for CKDMS Voyager",
                    //TermsOfService = "N/A",
                    //Contact = new Contact { Name = "Cox & Kings", Email = "voyager.support@coxandkings.com", Url = "https://ui.voyager.com" },
                    //License = new License { Name = "Cox & Kings", Url = "https://ui.voyager.com/license" }
                });

                // Set the comments path for the Swagger JSON and UI.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "VGER_DISTRIBUTION.xml");
                //c.IncludeXmlComments(xmlPath);

                XElement xml = null;
                XElement dependentXml = null;
                //build one large xml comments file
                foreach (string fileName in Configuration["XMLCommentFileNames"].Split(','))
                {
                    string qualifiedFileName = basePath + "\\" + fileName;
                    if (xml == null)
                    {
                        xml = XElement.Load(qualifiedFileName);
                    }
                    else
                    {
                        dependentXml = XElement.Load(qualifiedFileName);
                        foreach (XElement ele in dependentXml.Descendants())
                        {
                            xml.Add(ele);
                        }
                    }
                }
                //save comments file, point swagger at it.
                string swaggerFile = basePath +
                                 "\\CombinedDocumentation.xml";
                if (xml != null)
                {
                    xml.Save(swaggerFile);
                    c.IncludeXmlComments(swaggerFile);
                }

                c.OperationFilter<AddRequiredHeaderParameter>();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            GlobalDiagnosticsContext.Set("configDir", Configuration["BaseDir"]);
            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Web Api with Mongo V1");

            });

            app.UseAuthentication();
            loggerFactory.AddNLog();
            app.UseMvc();
        }
    }
}
