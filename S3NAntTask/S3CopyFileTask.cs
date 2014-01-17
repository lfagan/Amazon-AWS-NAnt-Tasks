using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3-copyFile")]
    class S3CopyFileTask : S3CoreTask
    {
        #region Task Attributes

        [TaskAttribute("sourceFile", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string sourceKey { get; set; }

        [TaskAttribute("targetFile", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string targetKey { get; set; }

        [TaskAttribute("targetBucket", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string TargetBucket { get; set; }

        [TaskAttribute("overwrite", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public bool Overwrite { get; set; }

        #endregion

        /// <summary>Execute the NAnt task</summary>
        protected override void ExecuteTask()
        {

            // Ensure the configured bucket exists
            if (!BucketExists(BucketName))
            {
                Project.Log(Level.Error, "[ERROR] S3 Bucket '{0}' not found!", BucketName);
                return;
            }

            if (!BucketExists(TargetBucket))
            {
                Project.Log(Level.Error, "[ERROR] S3 Bucket '{0}' not found!", TargetBucket);
                return;
            }

            try
            {
                Project.Log(Level.Info, "Copying: " + BucketName + ": " + sourceKey + " to " + TargetBucket + ": " + targetKey);
                CopyObjectRequest request = new CopyObjectRequest
                {
                    SourceBucket = BucketName,
                    SourceKey = sourceKey,
                    DestinationBucket = TargetBucket,
                    DestinationKey = targetKey
                };
                CopyObjectResponse response = Client.CopyObject(request);
            }
            catch (AmazonS3Exception ex)
            {
                Project.Log(Level.Error, "[ERROR] {0}: {1} \r\n{2}", ex.StatusCode, ex.Message, ex.InnerException);
                return;
            }

            if (!FileExists(targetKey))
                Project.Log(Level.Error, "Copy FAILED!");
            else
                Project.Log(Level.Info, "Copy successful!");
        
        }

        /// <summary>Determine if our file already exists in the specified S3 bucket</summary>
        /// <returns>True if the file already exists in the specified bucket</returns>
        public bool FileExists(string fileKey)
        {
            using (Client)
            {
                try
                {
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = BucketName
                    };

                    using (var response = Client.ListObjects(request))
                    {
                        foreach (var file in response.S3Objects)
                        {
                            if (file.Key.Equals(fileKey))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (AmazonS3Exception ex)
                {
                    ShowError(ex);
                }
            }
            return false;
        }

    }
}
