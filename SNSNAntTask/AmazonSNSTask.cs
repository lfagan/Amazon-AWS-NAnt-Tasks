using System;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace SNSNAntTask {
    [TaskName("amazon-sns")]
    public class AmazonSNSTask : Task {
        [TaskAttribute("accesskey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSAccessKey { get; set; }

        [TaskAttribute("secretkey", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AWSSecretKey { get; set; }

        [TaskAttribute("topic", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Topic { get; set; }

        [TaskAttribute("arn", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ARN { get; set; }

        [TaskAttribute("subject", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Subject { get; set; }

        [TaskAttribute("message", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Message { get; set; }

        /// <summary>Execute NAnt task</summary>
        protected override void ExecuteTask() {
            var isArnSet = !String.IsNullOrEmpty(ARN);
            // Ensure our topic exists
            if (!DoesTopicExist())
                CreateTopic();
            // Ensure the ARN is set
            if (!isArnSet) {
                Project.Log(Level.Info, "Please set the SNS ARN!");
                return;
            }
            Project.Log(Level.Info, "Sending message to '{0}'.", Topic);
            using (Client) {
                try {
                    var request = new PublishRequest {
                        TopicArn = ARN,
                        Subject = Subject,
                        Message = Message
                    };
                    var response = Client.Publish(request);
                    if (response.IsSetPublishResult()) {
                        var result = response.PublishResult;
                        Project.Log(Level.Info, "Successfully published message ID: {0}", result.MessageId);
                        return;
                    }
                } catch(AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Error, "Error publishing message!");
        }

        /// <summary>Create our desired SNS topic</summary>
        private void CreateTopic() {
            using (Client) {
                try {
                    var request = new CreateTopicRequest {
                        Name = Topic
                    };
                    var response = Client.CreateTopic(request);
                    if (response.IsSetCreateTopicResult()) {
                        var result = response.CreateTopicResult;
                        Project.Log(Level.Info, "SNS topic '{0}' created with ARN: {1}", Topic, result.TopicArn);
                        return;
                    }
                    Project.Log(Level.Info, "Could not create SNS topic. Please check the name: {0}", Topic);
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
        }

        /// <summary>Determine if the specified SNS topic exists</summary>
        /// <returns>True if the topic exists</returns>
        private bool DoesTopicExist() {
            Project.Log(Level.Info, "Checking if SNS topic '{0}' exists.", Topic);
            using (Client) {
                try {
                    var request = new ListTopicsRequest();
                    var response = Client.ListTopics(request);
                    foreach (var topic in response.ListTopicsResult.Topics) {
                        Project.Log(Level.Info, "Found SNS topic ARN: {0}", topic.TopicArn);
                        if (topic.TopicArn.Contains(Topic)) {
                            Project.Log(Level.Info, "SNS topic '{0}' already exists!", Topic); 
                            return true;
                        }
                    }
                } catch (AmazonS3Exception ex) {
                    ShowError(ex);
                }
            }
            Project.Log(Level.Info, "SNS topic '{0}' does not exist!", Topic);
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

        /// <summary>Get an Amazon SNS client. Be sure to dispose of the client when done</summary>
        private AmazonSimpleNotificationService Client {
            get {
                var config = new AmazonSimpleNotificationServiceConfig {
                    ServiceURL = "http://sns.us-west-1.amazonaws.com" // US West (N. California)
                };
                return Amazon.AWSClientFactory.CreateAmazonSNSClient(AWSAccessKey, AWSSecretKey, config);
            }
        }
    }
}
