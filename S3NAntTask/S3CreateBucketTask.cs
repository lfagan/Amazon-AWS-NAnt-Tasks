using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3-CreateBucket")]
    public class S3CreateBucketTask : S3CoreBucketTask
    {

        protected override void ExecuteTask() 
        {
            if (!BucketExists())
                CreateBucket();
            else
                Project.Log(Level.Error, "Bucket already exists!", BucketName);
        }

        /// <summary>Create the configured bucket</summary>
        public void CreateBucket()
        {
            Project.Log(Level.Info, "Creating S3 bucket '{0}'", BucketName);
            using (Client)
            {
                try
                {
                    var request = new PutBucketRequest
                    {
                        BucketName = BucketName,
                        BucketRegion = _region
                    };
                    Client.PutBucket(request);
                }
                catch (AmazonS3Exception ex)
                {
                    ShowError(ex);
                }
            }
        }
 
    }
}
