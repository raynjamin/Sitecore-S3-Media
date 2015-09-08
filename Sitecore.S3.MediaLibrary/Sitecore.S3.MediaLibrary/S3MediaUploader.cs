

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Pipelines.GetMediaCreatorOptions;
using Sitecore.Resources.Media;
using Sitecore.Zip;

namespace Sitecore.S3.MediaLibrary
{
    /// <summary>
    /// Represents a MediaUpload.
    /// 
    /// </summary>
    public class S3MediaUploader
    {
        private string _folder = string.Empty;
        private string _alternateText;
        private HttpPostedFile _file;
        private Language _language;
        
        /// <summary>
        /// Gets or sets the alternate text.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The alternate text.
        /// </value>
        public string AlternateText
        {
            get
            {
                return this._alternateText;
            }
            set
            {
                this._alternateText = value;
            }
        }

        /// <summary>
        /// Gets or sets the file.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The file.
        /// </value>
        public HttpPostedFile File
        {
            get
            {
                return this._file;
            }
            set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._file = value;
            }
        }

        /// <summary>
        /// Gets or sets the folder.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The folder.
        /// </value>
        public string Folder
        {
            get
            {
                return this._folder;
            }
            set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._folder = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Sitecore.Resources.Media.MediaUploader"/> is unpack.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if unpack; otherwise, <c>false</c>.
        /// </value>
        public bool Unpack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Sitecore.Resources.Media.MediaUploader"/> is versioned.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if versioned; otherwise, <c>false</c>.
        /// </value>
        public bool Versioned { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The language.
        /// </value>
        public Language Language
        {
            get
            {
                return this._language;
            }
            set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._language = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Sitecore.Resources.Media.MediaUploader"/> is overwrite.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if overwrite; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:Sitecore.Resources.Media.MediaUploader"/> files the based.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if the <see cref="T:Sitecore.Resources.Media.MediaUploader"/> files the based; otherwise, <c>false</c>.
        /// 
        /// </value>
        public bool FileBased { get; set; }

        /// <summary>
        /// Gets or sets the database to create media item in.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The database.
        /// </value>
        public Database Database { get; set; }

        /// <summary>
        /// Uploads this instance.
        /// 
        /// </summary>
        public List<S3MediaUploadResult> Upload()
        {
            List<S3MediaUploadResult> list = new List<S3MediaUploadResult>();

            if (string.Compare(Path.GetExtension(this.File.FileName), ".zip", StringComparison.InvariantCultureIgnoreCase) == 0 && this.Unpack)
                this.UnpackToDatabase(list);
            else if (String.Compare(Path.GetExtension(this.File.FileName), ".png", StringComparison.InvariantCultureIgnoreCase) == 0)
                this.UploadToS3(list);
            else
                this.UploadToDatabase(list);
            return list;
        }

        private void UploadToS3(List<S3MediaUploadResult> list)
        {
            Assert.ArgumentNotNull(list, "list");
            var upload = new S3MediaUploadResult();
            list.Add(upload);
            upload.Path = FileUtil.MakePath(this.Folder, Path.GetFileName(this.File.FileName), '/');
            upload.ValidMediaPath = MediaPathManager.ProposeValidMediaPath(upload.Path);
            MediaCreatorOptions options = new MediaCreatorOptions
            {
                Versioned = this.Versioned,
                Language = this.Language,
                KeepExisting = !this.Overwrite,
                Destination = upload.ValidMediaPath,
                FileBased = this.FileBased,
                AlternateText = this.AlternateText,
                Database = this.Database
            };

            options.Build(GetMediaCreatorOptionsArgs.UploadContext);
                        
            var item = MediaManager.Creator.CreateFromStream(new MemoryStream(), upload.Path, options);

            upload.Item = item;

            S3Client.SendImageToS3(item.ID.ToString(), File.InputStream);
        }

        /// <summary>
        /// Uploads to database.
        /// 
        /// </summary>
        private void UploadToDatabase(List<S3MediaUploadResult> list)
        {
            Assert.ArgumentNotNull((object)list, "list");
            var S3MediaUploadResult = new S3MediaUploadResult();
            list.Add(S3MediaUploadResult);
            S3MediaUploadResult.Path = FileUtil.MakePath(this.Folder, Path.GetFileName(this.File.FileName), '/');
            S3MediaUploadResult.ValidMediaPath = MediaPathManager.ProposeValidMediaPath(S3MediaUploadResult.Path);
            MediaCreatorOptions options = new MediaCreatorOptions
            {
                Versioned = this.Versioned,
                Language = this.Language,
                KeepExisting = !this.Overwrite,
                Destination = S3MediaUploadResult.ValidMediaPath,
                FileBased = this.FileBased,
                AlternateText = this.AlternateText,
                Database = this.Database
            };
            options.Build(GetMediaCreatorOptionsArgs.UploadContext);
            S3MediaUploadResult.Item = MediaManager.Creator.CreateFromStream(this.File.InputStream, S3MediaUploadResult.Path, options);
        }

        /// <summary>
        /// Unpacks to database.
        /// 
        /// </summary>
        private void UnpackToDatabase(List<S3MediaUploadResult> list)
        {
            Assert.ArgumentNotNull((object)list, "list");
            string str = FileUtil.MapPath(TempFolder.GetFilename("temp.zip"));
            this.File.SaveAs(str);
            try
            {
                using (ZipReader zipReader = new ZipReader(str))
                {
                    foreach (ZipEntry zipEntry in zipReader.Entries)
                    {
                        if (!zipEntry.IsDirectory)
                        {
                            S3MediaUploadResult S3MediaUploadResult = new S3MediaUploadResult();
                            list.Add(S3MediaUploadResult);
                            S3MediaUploadResult.Path = FileUtil.MakePath(this.Folder, zipEntry.Name, '/');
                            S3MediaUploadResult.ValidMediaPath = MediaPathManager.ProposeValidMediaPath(S3MediaUploadResult.Path);
                            MediaCreatorOptions options = new MediaCreatorOptions()
                            {
                                Language = this.Language,
                                Versioned = this.Versioned,
                                KeepExisting = !this.Overwrite,
                                Destination = S3MediaUploadResult.ValidMediaPath,
                                FileBased = this.FileBased,
                                Database = this.Database
                            };
                            options.Build(GetMediaCreatorOptionsArgs.UploadContext);
                            Stream stream = zipEntry.GetStream();
                            S3MediaUploadResult.Item = MediaManager.Creator.CreateFromStream(stream, S3MediaUploadResult.Path, options);
                        }
                    }
                }
            }
            finally
            {
                FileUtil.Delete(str);
            }
        }
    }
}