﻿using Microsoft.Extensions.DependencyInjection;
using Sanchez.Processing.Filesystem;
using Sanchez.Processing.Filesystem.Equirectangular;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Database;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Underlay;

namespace Sanchez.Processing.Builders
{
    public static class ProcessingBuilder
    {
        public static IServiceCollection AddProcessing(this IServiceCollection services)
        {
            return services
                .AddUnderlaySupport()
                .AddFilenameProviders()
                .AddSingleton<ISatelliteImageLoader, SatelliteImageLoader>()
                .AddSingleton<IDatabaseMigrator, DatabaseMigrator>()
                .AddSingleton<IImageMatcher, ImageMatcher>()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>()
                .AddSingleton<ISatelliteRegistry, SatelliteRegistry>()
                .AddSingleton<FilenameParserProvider>();
        }

        private static IServiceCollection AddFilenameProviders(this IServiceCollection services)
        {
            return services
                .AddSingleton<StitchedBatchFilenameProvider>()
                .AddSingleton<StitchedFilenameProvider>()
                .AddSingleton<SingleFilenameProvider>();
        }

        private static IServiceCollection AddUnderlaySupport(this IServiceCollection services)
        {
            return services
                .AddSingleton<IUnderlayCache, UnderlayCache>()
                .AddSingleton<IUnderlayService, UnderlayService>()
                .AddSingleton<IUnderlayCacheRepository, UnderlayCacheRepository>();
        }
    }
}