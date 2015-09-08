using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Pipelines.Upload;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.Web;
using Sitecore.Zip;

namespace Sitecore.S3.MediaLibrary.Processors
{
    public class SaveImagesToS3 : UploadProcessor
    {
        /// <summary>
        ///     Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="T:System.Exception"><c>Exception</c>.</exception>
        public void Process(UploadArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            for (var index = 0; index < args.Files.Count; ++index)
            {
                var file = args.Files[index];
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    try
                    {
                        var flag = IsUnpack(args, file);
                        if (args.FileOnly)
                        {
                            if (flag)
                            {
                                UnpackToFile(args, file);
                            }
                            else
                            {
                                var filename = UploadToFile(args, file);
                                if (index == 0)
                                    args.Properties["filename"] = FileHandle.GetFileHandle(filename);
                            }
                        }
                        else
                        {
                            var mediaUploader = new S3MediaUploader
                            {
                                File = file,
                                Unpack = flag,
                                Folder = args.Folder,
                                Versioned = args.Versioned,
                                Language = args.Language,
                                AlternateText = args.GetFileParameter(file.FileName, "alt"),
                                Overwrite = args.Overwrite,
                                FileBased = args.Destination == UploadDestination.File
                            };
                            List<S3MediaUploadResult> list;
                            using (new SecurityDisabler())
                                list = mediaUploader.Upload();
                            Log.Audit(this, "Upload: {0}", file.FileName);
                            foreach (var S3MediaUploadResult in list)
                                ProcessItem(args, S3MediaUploadResult.Item, S3MediaUploadResult.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not save posted file: " + file.FileName, ex, this);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///     Processes the item.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="mediaItem">The media item.</param>
        /// <param name="path">The path.</param>
        private void ProcessItem(UploadArgs args, MediaItem mediaItem, string path)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(mediaItem, "mediaItem");
            Assert.ArgumentNotNull(path, "path");
            if (args.Destination == UploadDestination.Database)
                Log.Info("Media Item has been uploaded to database: " + path, this);
            else
                Log.Info("Media Item has been uploaded to file system: " + path, this);
            args.UploadedItems.Add(mediaItem.InnerItem);
        }

        /// <summary>
        ///     Unpacks to file.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="file">The file.</param>
        private static void UnpackToFile(UploadArgs args, HttpPostedFile file)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(file, "file");
            var filename = FileUtil.MapPath(TempFolder.GetFilename("temp.zip"));
            file.SaveAs(filename);
            using (var zipReader = new ZipReader(filename))
            {
                foreach (var zipEntry in zipReader.Entries)
                {
                    var str = FileUtil.MakePath(args.Folder, zipEntry.Name, '\\');
                    if (zipEntry.IsDirectory)
                    {
                        Directory.CreateDirectory(str);
                    }
                    else
                    {
                        if (!args.Overwrite)
                            str = FileUtil.GetUniqueFilename(str);
                        Directory.CreateDirectory(Path.GetDirectoryName(str));
                        lock (FileUtil.GetFileLock(str))
                            FileUtil.CreateFile(str, zipEntry.GetStream(), true);
                    }
                }
            }
        }

        /// <summary>
        ///     Uploads to file.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="file">The file.</param>
        /// <returns>
        ///     The name of the uploaded file
        /// </returns>
        private string UploadToFile(UploadArgs args, HttpPostedFile file)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(file, "file");
            var str = FileUtil.MakePath(args.Folder, Path.GetFileName(file.FileName), '\\');
            if (!args.Overwrite)
                str = FileUtil.GetUniqueFilename(str);
            file.SaveAs(str);
            Log.Info("File has been uploaded: " + str, this);
            return Assert.ResultNotNull(str);
        }
    }
}