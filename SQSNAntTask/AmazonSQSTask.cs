using System;
using Amazon;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace SQSNAntTask {
    [TaskName("amazon-sqs")]
    public class AmazonSQSTask : Task {
        [TaskAttribute("accesskey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSAccessKey { get; set; }

        [TaskAttribute("secretkey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSSecretKey { get; set; }

        [TaskAttribute("queue", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string QueueName { get; set; }

        [TaskAttribute("url", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string QueueUrl { get; set; }

        [TaskAttribute("message", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Message { get; set; }

        /// <summary>Run the NAnt task</summary>
        protected override void ExecuteTask() {
            var urlIsSet = !String.IsNullOrEmpty(QueueUrl);
            // Ensure the queue exists
            if (!DoesQueueExist())
                CreateQueue();
            // Ensure the queue URL was set
            if (!urlIsSet) {
                Project.Log(Level.Info, "Please set the queue URL: 'url=\"{0}\"'", QueueUrl);
                return;
            }
            Project.Log(Level.Info, "Sending message to queue...");
            using (Client) {
                try {
                    var request = new SendMessageRequest {
                        QueueUrl = QueueUrl,
                        MessageBody = Message
                    };
                    Client.SendMessage(request);
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "Successfully sent message to Amazon SQS!");
        }

        /// <summary>Create our named queue</summary>
        private void CreateQueue() {
            using (Client) {
                try {
                    Project.Log(Level.Info, "Creating queue: {0}", QueueName);
                    var request = new CreateQueueRequest {
                        QueueName = QueueName
                    };
                    var response = Client.CreateQueue(request);
                    QueueUrl = response.CreateQueueResult.QueueUrl;
                    Project.Log(Level.Info, "Created queue '{0}' with URL: {1}", QueueName, QueueUrl);
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
        }

        /// <summary>Check to see if our queue already exists in SQS</summary>
        /// <returns>True if the queue already exists</returns>
        private bool DoesQueueExist() {
            Project.Log(Level.Info, "Checking to see if queue exists...");
            using (Client) {
                try {
                    var request = new ListQueuesRequest();
                    var response = Client.ListQueues(request);
                    if (response.IsSetListQueuesResult()) {
                        var result = response.ListQueuesResult;
                        foreach (var queueUrl in result.QueueUrl) {
                            if (queueUrl.Contains(QueueName)) {
                                Project.Log(Level.Info, "Queue '{0}' exists with URL: {1}", QueueName, queueUrl);
                                return true;
                            }
                        }
                    }
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "Queue '{0}' doesn't exist!", QueueName);
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

        /// <summary>Get an Amazon SQS client. Be sure to dispose of the client when done</summary>
        private AmazonSQS Client {
            get {
                var config = new AmazonSQSConfig {
                    ServiceURL = "http://sqs.us-west-1.amazonaws.com" // US West (N. California)
                };
                return AWSClientFactory.CreateAmazonSQSClient(AWSAccessKey, AWSSecretKey, config);
            }
        }
    }
}
