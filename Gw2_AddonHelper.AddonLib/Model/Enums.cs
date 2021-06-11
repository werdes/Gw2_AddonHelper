using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gw2_AddonHelper.AddonLib.Model
{
    public enum HostType
    {
        [EnumMember(Value = "github")]
        Github,
        [EnumMember(Value = "standalone")]
        Standalone
    }

    public enum DownloadType
    {
        [EnumMember(Value = "archive")]
        Archive,

        [EnumMember(Value = ".dll")]
        Dll
    }

    public enum InstallMode
    {
        [EnumMember(Value = "arc")]
        Arc,
        [EnumMember(Value = "binary")]
        Binary,
        [EnumMember(Value = "loader")]
        AddonLoader
    }

    public enum InstallState
    {
        [EnumMember(Value = "not_installed")]
        NotInstalled,
        [EnumMember(Value = "installed_disabled")]
        InstalledDisabled,
        [EnumMember(Value = "installed_enabled")]
        InstalledEnabled,
        [EnumMember(Value = "error")]
        Error
    }

    public enum AddonFlag
    {
        [EnumMember(Value = "self-updating")]
        SelfUpdating,
        [EnumMember(Value = "obscured-filename")]
        ObscuredFilename
    }

    public enum VersioningType
    {
        [EnumMember(Value = "github-commit-sha")]
        GithubCommitSha,
        [EnumMember(Value = "host-file-md5")]
        HostFileMd5,
        [EnumMember(Value = "self-updating")]
        SelfUpdating,
        [EnumMember(Value = "unknown")]
        Unknown
    }

    public enum UiState
    {
        Loading,
        AddonList,
        Installer,
        InstallerProgress,
        Settings,
        Error,
        Conflicts
    }

    public enum InstallerActionType
    {
        Install,
        Enable,
        Disable, 
        Remove,
        Update
    }

    public enum InstallProgress
    {
        Waiting, 
        InProgress,
        Completed,
        Error
    }
}
