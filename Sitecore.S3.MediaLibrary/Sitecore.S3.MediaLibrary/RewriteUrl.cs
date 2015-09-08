using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Imgix_CSharp;
using Sitecore.Resources.Media;

namespace Sitecore.S3.MediaLibrary
{
    public class RewriteUrl
    {
        public void Process(GetMediaStreamPipelineArgs args)
        {
            if (args.MediaData.Extension.Equals("PNG"))
            {
                var builder = new UrlBuilder("bensterrett.imgix.net")
                {
                    SignKey = WebConfigurationManager.AppSettings["imgixSecret"]
                };

                HttpContext.Current.Response.Redirect(builder.BuildUrl(HttpUtility.UrlEncode(args.MediaData.MediaItem.ID.ToString())));

                args.AbortPipeline();
            }
        }
    }
}