﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Offset;
using Sanchez.Processing.ImageProcessing.Underlay;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Services.Underlay
{
    public interface IUnderlayService
    {
        /// <summary>
        ///     Retrieves a full-colour underlay image with the target projection. Underlays are cached to disk to speed up
        ///     computation.
        /// </summary>
        /// <param name="options">Underlay generation options</param>
        /// <param name="definition">Optional satellite definition, if projecting underlay to match a satellite IR image</param>
        /// <returns>projected underlay</returns>
        Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionOptions options, SatelliteDefinition? definition = null);
    }

    public class UnderlayService : IUnderlayService
    {
        private readonly ILogger<UnderlayService> _logger;
        private readonly IUnderlayCache _cache;
        private readonly RenderOptions _options;

        public UnderlayService(
            ILogger<UnderlayService> logger,
            IUnderlayCache cache, 
            RenderOptions options)
        {
            _logger = logger;
            _cache = cache;
            _options = options;
        }

        /// <summary>
        ///     Retrieves a full-colour underlay image with the target projection. Underlays are cached to disk to speed up
        ///     computation.
        /// </summary>
        /// <param name="options">Underlay generation options</param>
        /// <param name="definition">Optional satellite definition, if projecting underlay to match a satellite IR image</param>
        /// <returns>projected underlay</returns>
        public async Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionOptions options, SatelliteDefinition? definition = null)
        {
            // Attempt to retrieve underlay from cache
            var cached = await _cache.GetUnderlayAsync(definition, options);
            if (cached != null) return cached;

            // Load master equirectangular underlay image from disk
            var underlay = await Image.LoadAsync<Rgba32>(options.UnderlayPath);

            // Project to match satellite imagery
            var target = GetProjected(underlay, definition, options);
            Resize(options, target);

            // Register underlay in cache
            await _cache.SetUnderlayAsync(target, definition, options);
            return target;
        }

        /// <summary>
        ///     Optionally resizes the underlay based on the target height.
        /// </summary>
        private void Resize(UnderlayProjectionOptions options, Image<Rgba32> underlay)
        {
            if (options.TargetHeight == null) return;

            // Resize underlay to target image size
            var targetHeight = options.TargetHeight.Value;

            // Ensure correct aspect ratio
            var targetWidth = (int) Math.Round(underlay.Width / (float) underlay.Height * targetHeight);
            _logger.LogInformation("Resizing underlay to {width} x {height} px", targetWidth, targetHeight);

            underlay.Mutate(c => c.Resize(targetWidth, targetHeight));
        }

        /// <summary>
        ///     Returns an underlay projected and optionally cropped.
        /// </summary>
        private Image<Rgba32> GetProjected(Image<Rgba32> underlay, SatelliteDefinition? definition, UnderlayProjectionOptions options)
        {
            switch (options.Projection)
            {
                case ProjectionType.Geostationary:
                    if (definition == null) throw new InvalidOperationException("Satellite definition must be provided for geostationary projection");

                    // Project underlay to geostationary, based on the target satellite
                    _logger.LogInformation("{definition:l0} Rendering geostationary underlay", definition.DisplayName);
                    return underlay.ToGeostationaryProjection(definition.Longitude, definition.Height, _options);

                case ProjectionType.Equirectangular:

                    // Optionally crop and offset to specified lat/long range
                    if (options.CropSpecified)
                    {
                        Offset(underlay, options.LongitudeCrop!.Value);
                        Crop(underlay, options.LatitudeCrop!.Value);
                    }

                    return underlay;
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled projection type: {options.Projection}");
            }
        }

        /// <summary>
        ///     Offsets an equirectangular underlay to start at a specified longitude.
        /// </summary>
        private void Offset(Image<Rgba32> underlay, Range longitudeRange)
        {
            var xPixelRange = longitudeRange.ToPixelRangeX(underlay.Width);

            var offset = -xPixelRange.Start;
            _logger.LogInformation("Offsetting underlay by {pixels} px", offset);
            underlay.HorizontalOffset(offset);
        }

        /// <summary>
        ///     Crop to a specified latitude range.
        /// </summary>
        private void Crop(Image<Rgba32> underlay, Range latitudeRange)
        {
            var yPixelRange = latitudeRange.ToPixelRangeY(underlay.Height);

            // Crop underlay to target height
            _logger.LogInformation("Cropping underlay to {min} - {max} px height", yPixelRange.Start, yPixelRange.End);
            underlay.Mutate(c => c.Crop(new Rectangle(0, yPixelRange.Start, underlay.Width, yPixelRange.Range)));
        }
    }
}