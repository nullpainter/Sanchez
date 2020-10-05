﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ShellProgressBar;

namespace Sanchez.Workflow.Models.Data
{
    /// <summary>
    ///     Data backing <see cref="Sanchez.Workflow.Workflows.Equirectangular.EquirectangularTimelapseWorkflow"/>.
    /// </summary>
    public class EquirectangularTimelapseWorkflowData : EquirectangularStitchWorkflowData
    {
        internal List<DateTime> TimeIntervals { get; [UsedImplicitly] set;  } = new List<DateTime>();
        
        /// <summary>
        ///     Timestamp of currently-processed step.
        /// </summary>
        public DateTime? TargetTimestamp { get; set; }
        
        /// <summary>
        ///     Progress bar rendering individual image rendering steps.
        /// </summary>
        public IProgressBar? ImageProgressBar { get; [UsedImplicitly] set; }

        public override void Dispose()
        {
            base.Dispose();
            ImageProgressBar?.Dispose();
        }
    }
}