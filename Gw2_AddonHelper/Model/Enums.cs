﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gw2_AddonHelper.Model
{
    public enum HostType
    {
        [EnumMember(Value = @"github")]
        Github,
        [EnumMember(Value = @"standalone")]
        Standalone
    }

    public enum DownloadType
    {
        [EnumMember(Value = @"archive")]
        Archive,

        [EnumMember(Value = @".dll")]
        Dll
    }

    public enum InstallMode
    {
        [EnumMember(Value = @"arc")]
        Arc,
        [EnumMember(Value = @"binary")]
        Binary
    }
}