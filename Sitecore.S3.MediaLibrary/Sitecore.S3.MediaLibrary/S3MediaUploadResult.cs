using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;

namespace Sitecore.S3.MediaLibrary
{
    public class S3MediaUploadResult
    {
        private Item _item;
        private string _path;
        private string _validMediaPath;

        public Item Item
        {
            get
            {
                return this._item;
            }
            internal set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._item = value;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
            internal set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._path = value;
            }
        }

        public string ValidMediaPath
        {
            get
            {
                return this._validMediaPath;
            }
            internal set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._validMediaPath = value;
            }
        }
    }
}