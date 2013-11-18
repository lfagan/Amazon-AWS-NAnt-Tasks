using System;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace S3NAntTask
{
    public abstract class S3CoreBucketTask : S3CoreTask
    {
        protected override void ExecuteTask()
        {

        }

    }
}
