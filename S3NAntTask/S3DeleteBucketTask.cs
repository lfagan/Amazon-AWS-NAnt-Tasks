using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3-DeleteBucket")]
    class S3DeleteBucketTask : S3CoreBucketTask
    {
        protected override void ExecuteTask()
        {
            Project.Log(Level.Info, "Deleting S3 bucket: {0}", BucketName);

            /// Check to see if the bucket exists before we try to delete it. 
            if (!BucketExists(BucketName))
            {
                Project.Log(Level.Warning, "Bucket: {0} not found!", BucketName);
                return;
            }
            else 
            {
                using (Client)
                {
                    try
                    {
                        DeleteBucketRequest request = new DeleteBucketRequest();
                        request.BucketName = BucketName;
                        Client.DeleteBucket(request);
                    }
                    catch (AmazonS3Exception ex)
                    {
                        ShowError(ex);
                    }
                }
            }
            
        }

    }

}
