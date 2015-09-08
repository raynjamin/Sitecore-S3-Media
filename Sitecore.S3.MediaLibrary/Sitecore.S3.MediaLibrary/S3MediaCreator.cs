using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Resources.Media;

namespace Sitecore.S3.MediaLibrary
{
    public class S3MediaCreator : MediaCreator
    {
        public virtual Item AttachStreamToMediaItem(Stream stream, string itemPath, string fileName, MediaCreatorOptions options)
        {
            Assert.ArgumentNotNull((object)stream, "stream");
            Assert.ArgumentNotNullOrEmpty(fileName, "fileName");
            Assert.ArgumentNotNull((object)options, "options");
            Assert.ArgumentNotNull((object)itemPath, "itemPath");
            Media media = MediaManager.GetMedia(CreateItem(itemPath, fileName, options));
            media.SetStream(stream, FileUtil.GetExtension(fileName));
            return (Item)media.MediaData.MediaItem;
        } 
    }
}