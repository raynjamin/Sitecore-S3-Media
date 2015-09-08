using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Sitecore.S3.MediaLibrary
{
    public class S3Client
    {
        public static String BucketName = "sitecoreimgix";

        public static Boolean SendImageToS3(String key, Stream imageStream)
        {
            var success = false;

            using (var client = new AmazonS3Client(RegionEndpoint.USWest2))
            {
                try
                {
                    PutObjectRequest request = new PutObjectRequest()
                    {
                        InputStream = imageStream,
                        BucketName = BucketName,
                        Key = key
                    };

                    client.PutObject(request);
                    success = true;
                }

                catch (Exception ex)
                {
                    // swallow everything for now.
                }
            }

            return success;
        }
    }
}