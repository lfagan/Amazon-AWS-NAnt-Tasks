using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask {
    [TaskName("amazon-s3")]
    public class AmazonS3Task : Task {
        /// <summary>Region to create the new bucket in. Default to US standard</summary>
        private S3Region _region = S3Region.US;

        [TaskAttribute("accesskey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSAccessKey { get; set; }

        [TaskAttribute("secretkey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSSecretKey { get; set; }

        [TaskAttribute("bucket", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string BucketName { get; set; }

        [TaskAttribute("file", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string FilePath { get; set; }

        [TaskAttribute("region", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Region {
            get { return _region.ToString(); }
            set {
                _region = (S3Region)Enum.Parse(typeof (S3Region), value);
                Project.Log(Level.Info, String.Format("Set Amazon region to: {0}", _region));
            }
        }

        /// <summary>Execute the NAnt task</summary>
        protected override void ExecuteTask() {
            // List the files included in this send
            Project.Log(Level.Info, "Included file '{0}'", FileName);
            // Ensure the specified file exists
            if (!File.Exists(FilePath)) {
                Project.Log(Level.Error, "File '{0}' doesn't exist!", FilePath);
                return;
            }
            // Ensure the configured bucket exists
            if (!DoesBucketExist())
                CreateBucket();
            // Ensure the file doesn't already exist in the specified bucket
            if (DoesFileExist())
                return;
            // Send the file to S3
            using (Client) {
                try {
                    Project.Log(Level.Info, "Uploading file to Amazon S3: {0}", FileName);
                    var request = new PutObjectRequest {
                        Key = FileName,
                        BucketName = BucketName,
                        FilePath = FilePath,
                        Timeout = 3600000
                    };
                    using (var response = Client.PutObject(request)) {
                        foreach (string key in response.Headers.Keys) {
                            Project.Log(Level.Info, "Response header: {0}, Value {1}", key, response.Headers.Get(key));
                        }
                    }
                }
                catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "Successfully sent file to Amazon S3: {0}", FileName);
        }

        /// <summary>Determine if our file already exists in the specified S3 bucket</summary>
        /// <returns>True if the file already exists in the specified bucket</returns>
        private bool DoesFileExist() {
            Project.Log(Level.Info, "Checking to see if '{0}' exists in bucket '{1}'", FileName, BucketName);
            using (Client) {
                try {
                    var request = new ListObjectsRequest {
                        BucketName = BucketName
                    };
                    using (var response = Client.ListObjects(request)) {
                        foreach (var file in response.S3Objects) {
                            Project.Log(Level.Debug, "Found file '{0}' in bucket.", file.Key);
                            if (file.Key.Equals(FileName)) {
                                Project.Log(Level.Info, "File '{0}' already exists in '{1}'!", FileName, BucketName);
                                return true;
                            }
                        }
                    }
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "File '{0}' doesn't already exist.", FileName);
            return false;
        }

        /// <summary>Create the configured bucket</summary>
        private void CreateBucket() {
            Project.Log(Level.Info, "Creating S3 bucket '{0}'", BucketName);
            using (Client) {
                try {
                    var request = new PutBucketRequest {
                        BucketName = BucketName,
                        BucketRegion = _region
                    };
                    Client.PutBucket(request);
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
        }

        /// <summary>Determine if the specified bucket alredy exists</summary>
        /// <returns>True if the bucket exists</returns>
        private bool DoesBucketExist() {
            using (Client) {
                try {
                    using (var response = Client.ListBuckets()) {
                        if (response.Buckets.Any(bucket => bucket.BucketName.Equals(BucketName))) {
                            Project.Log(Level.Info, "S3 Bucket '{0}' already exists!", BucketName);
                            return true;
                        }
                    }
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "S3 Bucket '{0}' not found!", BucketName);
            return false;
        }

        /// <summary>Format and display an exception</summary>
        /// <param name="ex">Exception to display</param>
        private void ShowError(AmazonS3Exception ex) {
            if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity"))) {
                Project.Log(Level.Error, "Please check the provided AWS Credentials.");
                Project.Log(Level.Error, "If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
            } else {
                Project.Log(Level.Error, "An Error, number {0}, occurred with the message '{1}'",
                    ex.ErrorCode, ex.Message);
            }
        }

        /// <summary>Get the name of the file we're sending to S3</summary>
        private string FileName {
            get { return Path.GetFileName(FilePath); }
        }

        /// <summary>Get an Amazon S3 client. Be sure to dispose of the client when done</summary>
        private AmazonS3 Client {
            get { return Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey); }
        }
    }
}
