﻿using EDennis.AspNetCore.Base.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace EDennis.AspNetCore.Base.Testing {
    public class RepoInterceptor<TRepo, TEntity, TContext> : Interceptor
        where TEntity : class, IHasSysUser, new()
        where TContext : DbContext
        where TRepo : WriteableRepo<TEntity, TContext> {

        public RepoInterceptor(RequestDelegate next) : base(next) { }

        ILogger _logger;

        public async Task InvokeAsync(HttpContext context, IServiceProvider provider, 
            IConfiguration config,
            ILogger<RepoInterceptor<TRepo, TEntity, TContext>> logger) {

            _logger = logger;

            _logger.LogInformation($"RepoInterceptor handling request: {context.Request.Path}");


            if (!context.Request.Path.StartsWithSegments(new PathString("/swagger"))) {

                var header = GetTestingHeader(context);

                if (header.Key == null) {

                    var defaultInstanceName = DEFAULT_NAMED_INSTANCE;
                    try { 
                        if (context.Session != null)
                            defaultInstanceName = context.Session.Id;
                    } catch { }

                    context.Request.Headers.Add(HDR_USE_INMEMORY, defaultInstanceName);
                    header = new KeyValuePair<string, string>(HDR_USE_INMEMORY, defaultInstanceName);
                }

                _logger.LogInformation($"RepoInterceptor processing header {header.Key}: {header.Value}");

                var operation = header.Key;
                var baseInstanceName = header.Value;

                var repo = provider.GetRequiredService(typeof(TRepo)) as TRepo;
                var cache = provider.GetRequiredService(typeof(TestDbContextCache<TContext>))
                        as TestDbContextCache<TContext>;

                var baseDatabaseName = TestDbContextManager<TContext>.BaseDatabaseName(config);

                if (operation == HDR_USE_READONLY)
                    throw new ApplicationException("HDR_USE_READONLY not appropriate for Writeable Repo.");
                else if (operation == HDR_USE_INMEMORY) {
                    GetOrAddInMemoryDatabase(repo, cache, baseInstanceName, baseDatabaseName);
                } else if (operation == HDR_DROP_INMEMORY)
                    DropInMemory(cache, baseInstanceName);

            }

            await _next(context);

        }

        private void GetOrAddInMemoryDatabase(TRepo repo, TestDbContextCache<TContext> cache,
            string instanceName, string baseDatabaseName) {
            if (cache.ContainsKey(instanceName)) {
                repo.Context = cache[instanceName];
                _logger.LogInformation($"Using existing in-memory database {baseDatabaseName}, instance = {instanceName}");
            } else {
                _logger.LogInformation($"Creating in-memory database {baseDatabaseName}, instance = {instanceName}");
                var dbContext = TestDbContextManager<TContext>.CreateInMemoryDatabase(baseDatabaseName, instanceName);
                repo.Context = dbContext;
                repo.Context.Database.EnsureCreated();
                cache.Add(instanceName, repo.Context);
            }
        }

        private void DropInMemory(TestDbContextCache<TContext> cache, string instanceName) {
            if (cache.ContainsKey(instanceName)) {
                _logger.LogInformation($"Dropping in-memory history instance {instanceName} for {typeof(TContext).Name}");
                var context = cache[instanceName];
                TestDbContextManager<TContext>.DropInMemoryDatabase(context);
                cache.Remove(instanceName);
            }
        }




    }

    public static partial class IApplicationBuilderExtensions_RepoInterceptorMiddleware {
        public static IApplicationBuilder UseRepoInterceptor<TRepo, TEntity, TContext>(this IApplicationBuilder app)
                where TEntity : class, IHasSysUser, new()
        where TContext : DbContext
        where TRepo : WriteableRepo<TEntity, TContext> {
            app.UseMiddleware<RepoInterceptor<TRepo, TEntity, TContext>>();
            return app;
        }
    }


}
