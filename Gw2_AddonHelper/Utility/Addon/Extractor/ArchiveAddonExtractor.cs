using Gw2_AddonHelper.Model.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Utility.Addon.Extractor
{
    public class ArchiveAddonExtractor : BaseAddonExtractor, IAddonExtractor
    {

        public ArchiveAddonExtractor(Model.AddonList.Addon addon) : base(addon)
        {
        }

        public async Task<ExtractionResult> Extract(DownloadResult download, string version)
        {
            ExtractionResult manifest = new ExtractionResult();
            manifest.InstallationTime = DateTime.UtcNow;
            manifest.Version = version;


            string loaderPrefix = _config.GetValue<string>("installation:binary:prefix");
            
            using (MemoryStream zipStream = new MemoryStream(download.FileContent))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipStream))
                {
                    IEnumerable<ZipArchiveEntry> entries = zipArchive.Entries.Where(x => x.Length > 0);
                    string newRootDir = string.Empty;
                    ZipArchiveEntry loaderEntrypointFile = entries.Where(x => x.Name.StartsWith(loaderPrefix)).FirstOrDefault();
                    if(loaderEntrypointFile != null)
                    {
                        manifest.LoaderKey = Path.GetFileNameWithoutExtension(loaderEntrypointFile.Name).Replace(loaderPrefix, string.Empty);
                        newRootDir = Path.GetDirectoryName(loaderEntrypointFile.FullName);
                    }

                    foreach (ZipArchiveEntry entry in entries)
                    {
                        ExtractionResultFile file = new ExtractionResultFile();
                        file.FileName = entry.Name;
                        file.RelativePath = Path.GetDirectoryName(entry.FullName).Replace(newRootDir, string.Empty);

                        using (Stream entryStream = entry.Open())
                        {
                            using (MemoryStream entryMemoryStream = new MemoryStream())
                            {
                                entryStream.CopyTo(entryMemoryStream);
                                file.FileContent = entryMemoryStream.ToArray();
                            }
                        }

                        manifest.AddonFiles.Add(file);
                    }
                }
            }

            return manifest;
        }
    }
}
