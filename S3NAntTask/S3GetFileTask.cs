using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    [TaskName("amazon-s3-getFile")]
    public class S3GetFileTask : S3CoreFileTask
    {
        #region Task Attributes

        private string outputfile;

        [TaskAttribute("outputfile", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string Outputfile 
        {
            get
            {
                return outputfile;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    outputfile = Path.GetFileName(Key);
                else
                    outputfile = value;
            }
        
        }

        #endregion

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
                Project.Log(Level.Error, "File not found: {0}", FilePath);
                return;
            }

            // Get the file from S3
            using (Client)
            {
                try
                {
                    Project.Log(Level.Info, "Downloading \r\n    file: {0}\r\n      as: {1}", FilePath, Outputfile);
                    GetObjectRequest request = new GetObjectRequest
                    {
                        Key = FilePath,
                        BucketName = BucketName,
                        Timeout = timeout
                    };

                    using (var response = Client.GetObject(request))
                    {
                        response.WriteResponseStreamToFile(Outputfile);
                    }
                }
                catch (AmazonS3Exception ex)
                {
                    ShowError(ex);
                }
            }
            
            // verify that the file actually downloaded
            if (File.Exists(Outputfile))
                Project.Log(Level.Info, "Download successful.", Outputfile);
            else
                Project.Log(Level.Info, "Download FAILED!", Outputfile);
        }
    }
}
