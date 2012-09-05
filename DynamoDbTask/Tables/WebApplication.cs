using System;
using Amazon.DynamoDB.DataModel;

namespace DynamoDbTask.Tables {
    [DynamoDBTable("WebApplications")]
    public class WebApplication {        
        [DynamoDBHashKey]
        public string Name { get; set; }

        //[DynamoDBRangeKey(AttributeName = "LastUpdate")]
        public DateTime LastUpdate { get; set; }

        public string Version { get; set; }

        /// <summary>Constructor</summary>
        public WebApplication() {
            // Default 'no version' version number
            Version = "0.0.0.0";
        }
    }
}