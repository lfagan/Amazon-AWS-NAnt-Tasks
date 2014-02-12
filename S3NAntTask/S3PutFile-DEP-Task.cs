using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3")]
    class S3PutFile_DEP_Task : S3CoreFileTask
    {
        /// <summary>DEPRECATED: Execute the NAnt task</summary>
        /// This task exists ONLY to satisfy compatibilty with older versions of the task and script that rely on it
        protected override void ExecuteTask()
        {
            // Ensure the configured bucket exists
            if (!BucketExists(BucketName))
            {
                //Project.Log(Level.Error, "[ERROR] S3 Bucket '{0}' not found!", BucketName);
                S3CreateBucketTask cb = new S3CreateBucketTask();
                try
                {
                    cb.CreateBucket();
                }
                catch (Exception ex)
                {
                    Project.Log(Level.Error, "[ERROR] Error creating bucket. Msg: \r\n" + ex);
                }
                return;
            }

            // Ensure the specified file exists
            if (!File.Exists(FilePath))
            {
                Project.Log(Level.Error, "[ERROR] Local file '{0}' doesn't exist!", FilePath);
                return;
            }

            // Ensure the overwrite is false and the file doesn't already exist in the specified bucket
            if (!Overwrite && FileExists(FileName))
                return;

            // Send the file to S3
            using (Client)
            {
                try
                {
                    Project.Log(Level.Info, "Uploading file: {0}", FileName);
                    PutObjectRequest request = new PutObjectRequest
                    {
                        Key = FileName,
                        BucketName = BucketName,
                        FilePath = FilePath,
                        Timeout = timeout
                    };

                    var response = Client.PutObject(request);

                }
                catch (AmazonS3Exception ex)
                {
                    ShowError(ex);
                }
            }
            if (!FileExists(FileName))
                Project.Log(Level.Error, "Upload FAILED!");
            else
                Project.Log(Level.Info, "Upload successful!");
        }

    }
}
