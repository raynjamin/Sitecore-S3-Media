using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Resources.Media;

namespace Sitecore.S3.MediaLibrary
{
    public class S3MediaProvider : MediaProvider
    {
        public new MediaCreator Creator { get; private set; }
        //public new ImageEffects Effects { get; private set; }

        public S3MediaProvider()
        {
            Creator = new S3MediaCreator();
            //Effects = new ImgixEffects();
        }
    }
}