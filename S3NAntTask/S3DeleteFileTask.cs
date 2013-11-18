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
            if (!BucketExists())
            {
                Project.Log(Level.Error, "[ERROR] S3 Bucket '{0}' not found!", BucketName);
                return;
            }

            // Ensure the file exists
            if (!FileExists())
            {
                Project.Log(Level.Error, "File not found {0}", FileName);
                return;
            }
            else
            {
                // Delete the file from S3
                using (Client)
                {
                    try
                    {
                        Project.Log(Level.Info, "Deleting file: {0}", FileName);
                        DeleteObjectRequest request = new DeleteObjectRequest
                        {
                            Key = FileName,
                            BucketName = BucketName,
                        };

                        var response = Client.DeleteObject(request);

                    }
                    catch (AmazonS3Exception ex)
                    {
                        ShowError(ex);
                    }
                }
                if (FileExists())
                    Project.Log(Level.Error, "Delete file: {0} FAILED!", FileName);
                else
                    Project.Log(Level.Info, "Delete file: {0} successful.", FileName);

            }
        }

    }
}
