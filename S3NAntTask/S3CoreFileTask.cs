using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    public abstract class S3CoreFileTask : S3CoreTask
    {
        #region Task Attributes

        [TaskAttribute("file", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string FilePath { get; set; }

        [TaskAttribute("key", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Key { get; set; }

        [TaskAttribute("overwrite", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public bool Overwrite { get; set; }

        /// <summary>Get the name of the file we're sending to S3</summary>
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(Key))
                    return Path.GetFileName(FilePath);
                else
                    return Key;
            }
        }

        #endregion

        protected override void ExecuteTask()
        { 
        
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
                            //Project.Log(Level.Info, "File: " + file.Key);
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
