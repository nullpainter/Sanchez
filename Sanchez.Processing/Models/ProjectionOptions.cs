﻿using JetBrains.Annotations;
using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Models
{
    public class ProjectionOptions
    {
        public ProjectionOptions(ProjectionType projection, InterpolationType interpolation, int imageSize)
        {
            Projection = projection;
            Interpolation = interpolation;
            ImageSize = imageSize;
        }

        public ProjectionType Projection { get; }

        /// <remarks>
        ///     Used only to disambiguate cache entries.
        /// </remarks>
        public InterpolationType Interpolation { [UsedImplicitly] get; }

        /// <remarks>
        ///     Used only to disambiguate cache entries.
        /// </remarks>
        public int ImageSize { [UsedImplicitly] get; }
    }
}