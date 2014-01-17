using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3-deleteFile")]
    public class S3DeleteFileTask : S3CoreFileTask
    {

        /// <summary>Execute the NAnt task</summary>
        protected override void ExecuteTask()
        {
            // Ensure the configured bucket exists
            if (!BucketExists(BucketName))
            {
                Project.Log(Level.Error, "[ERROR] S3 Bucket: {0}, not found!", BucketName);
                return;
            }

            // Ensure the file exists
            if (!FileExists(FilePath))
            {
                Project.Log(Level.Error, "File not found {0}", FilePath);
                return;
            }
            else
            {
                // Delete the file from S3
                using (Client)
                {
                    try
                    {
                        Project.Log(Level.Info, "Deleting file: {0}", FilePath);
                        DeleteObjectRequest request = new DeleteObjectRequest
                        {
                            Key = FilePath,
                            BucketName = BucketName
                        };

                        var response = Client.DeleteObject(request);

                    }
                    catch (AmazonS3Exception ex)
                    {
                        ShowError(ex);
                    }
                }
                if (FileExists(FilePath))
                    Project.Log(Level.Error, "File delete FAILED!");
                else
                    Project.Log(Level.Info, "File deleted.");

            }
        }

    }
}
