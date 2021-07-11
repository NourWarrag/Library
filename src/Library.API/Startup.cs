using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace Library.API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(sa => {
                sa.ReturnHttpNotAcceptable = true;
                sa.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                sa.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                    implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddHttpCacheHeaders(expirationModelOptions=>
            { expirationModelOptions.MaxAge = 600;  
            },(validationModelOption) => {
                validationModelOption.AddMustRevalidate = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug(LogLevel.Information);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler( appBuilder =>
                {
                    appBuilder.Run(
                        async context =>
                        {
                            var expceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                            if (expceptionHandler != null)
                            {
                                var logger = loggerFactory.CreateLogger("Global execption logger");
                                logger.LogError(500,
                                expceptionHandler.Error, expceptionHandler.Error.Message);
                            }
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                        }
                        );
                });
            }
            AutoMapper.Mapper.Initialize(
                cfg =>
                {
                    cfg.CreateMap<Entities.Author, Models.AuthorDto>().ForMember(dest => dest.Name, 
                        opt => opt.MapFrom(src=> $"{src.FirstName} {src.LastName}")).ForMember(dest => dest.Age,
                        opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));
                    cfg.CreateMap<Entities.Book, Models.BookDto>();
                    cfg.CreateMap<Models.AuthorForCreationDto, Entities.Author>();
                    cfg.CreateMap<Models.BookForCreationDto, Entities.Book>();
                    cfg.CreateMap<Models.BookForUpdateDto, Entities.Book>();

                }
                );
            libraryContext.EnsureSeedDataForContext();
            app.UseHttpCacheHeaders();
            app.UseMvc(); 
        }
    }
}
 