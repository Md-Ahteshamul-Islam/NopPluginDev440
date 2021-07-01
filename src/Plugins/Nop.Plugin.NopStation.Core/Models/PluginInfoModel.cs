﻿using System;

namespace Nop.Plugin.NopStation.Core.Models
{
    public class PluginInfoModel
    {
        public string AssemblyName { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string BuildType { get; set; }

        public string AssemblyVersion { get; set; }

        public string FileVersion { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
