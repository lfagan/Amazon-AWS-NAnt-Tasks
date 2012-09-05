using System;
using Amazon;
using Amazon.DynamoDB;
using Amazon.DynamoDB.DataModel;
using Amazon.Runtime;
using DynamoDbTask.Tables;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask {
    [TaskName("amazon-dynamodb")]
    public class AmazonDynamoDbTask : Task {
        /// <summary>Region where we will interact with DynamoDB. Default to US standard</summary>
        private readonly RegionEndpoint _region = RegionEndpoint.USWest1;
        /// <summary>DynamoDB context</summary>
        private DynamoDBContext _context;

        [TaskAttribute("accesskey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSAccessKey { get; set; }

        [TaskAttribute("secretkey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSSecretKey { get; set; }

        [TaskAttribute("application", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Application { get; set; }

        [TaskAttribute("version", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Version { get; set; }

        /// <summary>Execute the NAnt task</summary>
        protected override void ExecuteTask() {
            // Log the update
            Project.Log(Level.Info, "Updating application '{0}' to new version '{1}' in DynamoDB", Application, Version);
            // Update DynamoDB
            using (var client = Client) {
                try {
                    _context = new DynamoDBContext(client);
                    var update = new WebApplication {
                        Name = Application,
                        Version = Version,
                        LastUpdate = DateTime.Now
                    };
                    _context.Save(update);
                } catch (AmazonDynamoDBException ex) {
                    ShowError(ex);
                    Environment.Exit(-1);
                }
            }
            Project.Log(Level.Info, "Successfully updated application '{0}' to version '{1}' in DynamoDB", Application, Version);
        }

        /// <summary>Format and display an exception</summary>
        /// <param name="ex">Exception to display</param>
        private void ShowError(AmazonServiceException ex) {
            if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity"))) {
                Project.Log(Level.Error, "Please check the provided AWS Credentials.");
                Project.Log(Level.Error, "If you haven't signed up for Amazon DynamoDB, please visit http://aws.amazon.com/dynamodb");
            } else {
                Project.Log(Level.Error, "An Error, number {0}, occurred with the message '{1}'",
                    ex.ErrorCode, ex.Message);
            }
        }

        /// <summary>Get an Amazon DynamoDB client. Be sure to dispose of the client when done</summary>
        private AmazonDynamoDBClient Client {
            get {
                var credentials = new BasicAWSCredentials(AWSAccessKey, AWSSecretKey);
                return new AmazonDynamoDBClient(credentials, _region);
            }
        }
    }
}