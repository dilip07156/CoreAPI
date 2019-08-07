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
using VGER_WAPI.Models;
using VGER_WAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VGER_WAPI.Repositories;
using VGER_WAPI.Repositories.Master;
using System.Net;

namespace VGER_WAPI
{
    public class Startup
    {
        public IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; set; }

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add service and create Policy with options 
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
                        ValidIssuer = Configuration["ValidAudience"],
                        ValidAudience = Configuration["ValidAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]))
                    };
                });

            //services.AddApplicationInsightsTelemetry(Configuration);

            //services.AddMvc(config => {
            //    config.Filters.Add(typeof(ExceptionFilter));
            //});
            services.AddMvc();

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(new CustomExceptionFilterAttribute(_hostingEnvironment, Configuration));
            //});  

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IMasterRepository, MasterRepository>();
            services.AddTransient<IGenericRepository, GenericRepository>();
            services.AddTransient<IQuoteRepository, QuoteRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IProductSRPRepository, ProductSRPRepository>();
            services.AddTransient<IProductPDPRepository, ProductPDPRepository>();
            services.AddTransient<IPositionRepository, PositionRepository>();
            services.AddTransient<IQRFSummaryRepository, QRFSummaryRepository>();
            services.AddTransient<IGuesstimateRepository, GuesstimateRepository>();
            services.AddTransient<IItineraryRepository, ItineraryRepository>();
            services.AddTransient<ICostsheetRepository, CostsheetRepository>();
            services.AddTransient<ICommercialsRepository, CommercialsRepository>();
            services.AddTransient<IProposalRepository, ProposalRepository>();
            services.AddTransient<ICostingRepository, CostingRepository>();
            services.AddTransient<IBookingRepository, BookingRepository>();
            services.AddTransient<IAgentApprovalRepository, AgentApprovalRepository>();
            services.AddTransient<IHandoverRepository, HandoverRepository>();
            services.AddTransient<IAgentRepository, AgentRepository>();
            services.AddTransient<IHotelsDeptRepository, HotelsDeptRepository>();
            services.AddTransient<ISupplierRepository, SupplierRepository>();
            services.AddTransient<IEmailRepository, EmailRepository>();
            services.AddTransient<IDocumentStoreRepository, DocumentStoreRepository>();
			services.AddTransient<ISettingsRepository, SettingsRepository>();
            services.AddTransient<IOperationsRepository, OperationsRepository>();
            services.AddTransient<IMISRepository, MISRepository>();
            services.AddTransient<ICommonRepository, CommonRepository>();
            services.AddTransient<IMSDynamicsRepository, MSDynamicsRepository>();
            services.AddTransient<IPDFRepository, PDFRepository>();

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
                    Title = "Web Api for App authentication",
                    Version = "v1",
                    Description = "Web Api for App authentication",
                    TermsOfService = "N/A",
                    Contact = new Contact { Name = "Cox & Kings", Email = "voyager.support@coxandkings.com", Url = "https://ui.voyager.com" },
                    License = new License { Name = "Cox & Kings", Url = "https://ui.voyager.com/license" }
                });

                // Set the comments path for the Swagger JSON and UI.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "VGER_WAPI.xml");
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<AddRequiredHeaderParameter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //  _hostingEnvironment = env; 

            // global policy, if assigned here (it could be defined indvidually for each controller) 
            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseExceptionHandler(
            //     options =>
            //     {
            //         options.Run(
            //         async context =>
            //         {
            //             context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //             context.Response.ContentType = "application/json";
            //             var ex = context.Features.Get<IExceptionHandlerFeature>();
            //             if (ex != null)
            //             {
            //                 var err = JsonConvert.SerializeObject(new Error()
            //                 {
            //                     Stacktrace = ex.Error.StackTrace,
            //                     Message = ex.Error.Message
            //                 });
            //                 await context.Response.WriteAsync(err).ConfigureAwait(false);
            //             }
            //         });
            //     }
            //    );

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Web Api with Mongo V1");

            });

            app.UseAuthentication();
            app.UseMvc();

        }
    }
}
