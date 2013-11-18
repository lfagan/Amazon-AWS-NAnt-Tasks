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

            // Get the file from S3
            using (Client)
            {
                try
                {
                    Project.Log(Level.Info, "Downloading file: {0} as {1}", FileName, Outputfile);
                    GetObjectRequest request = new GetObjectRequest
                    {
                        Key = FileName,
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
                Project.Log(Level.Info, "Download of '{0}' successful.", Outputfile);
            else
                Project.Log(Level.Info, "Download of '{0}' FAILED!", Outputfile);
        }
    }
}
