using Gw2_AddonHelper.AddonLib.Model.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Utility.Addon.Extractor
{
    public class DllAddonExtractor : BaseAddonExtractor, IAddonExtractor
    {
        public DllAddonExtractor(Common.Model.AddonList.Addon addon) : base(addon)
        {
        }

        public async Task<ExtractionResult> Extract(DownloadResult download, string version)
        {

            ExtractionResult extractionResult = new ExtractionResult();
            extractionResult.InstallationTime = DateTime.UtcNow;
            extractionResult.Version = download.Version;
            extractionResult.AddonFiles.Add(new ExtractionResultFile()
            {
                FileContent = download.FileContent,
                FileName = download.FileName,
                RelativePath = string.Empty
            });

            string loaderPrefix = _config.GetValue<string>("installation:binary:prefix");
            if (download.FileName.Contains(loaderPrefix))
            {
                extractionResult.LoaderKey = Path.GetFileNameWithoutExtension(download.FileName).Replace(loaderPrefix, string.Empty);
            }

            return extractionResult;
        }
    }
}
