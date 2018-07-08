﻿using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeBook.Data.CosmosDb;
using RecipeBook.Data.Manager;
using RecipeBook.Manager;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace RecipeBook.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.Configure<DocumentDbOptions>(Configuration.GetSection("DocumentDbOptions"));

            services.AddTransient<IRecipeBookDataManager, RecipeBookDataManager>();

            services.AddAutoMapper(typeof(RecipeBookRequestHandler).GetTypeInfo().Assembly);

            services.AddMediatR(typeof(RecipeBookRequestHandler).GetTypeInfo().Assembly);

            services.AddMvc()
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;

                    fv.RegisterValidatorsFromAssembly(typeof(RecipeBookRequestHandler).GetTypeInfo().Assembly);
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
