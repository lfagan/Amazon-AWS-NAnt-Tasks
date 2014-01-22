Amazon Web Services NAnt Tasks
==============================

This repository contains a Visual Studio 2010 solution with three projects:

S3NAntTask
----------

This project contains custom NAnt tasks for:

Function|NAnt Task|Comment
--------|---------|-------
Uploading a file|amazon-s3-putFile|
Uploading a file|amazon-s3|(DEPRECATED - for backwards compatibility only)
Downloading a file|amazon-s3-getFile|
Copy a file between buckets|amazon-s3-copyFile|
Deleting a file|amazon-s3-deleteFile|
Deleting all files in a bucket|amazon-s3-deleteAllFiles|
Creating a bucket|amazon-s3-CreateBucket|
Deleting a bucket|amazon-s3-DeleteBucket|

NOTE: Review default.build for usage of tasks

Desired improvements:
The ability to list multiple files (fileset)  to send to an s3 bucket rather than one at a time
The ability to encrypt AWS credentials
The ability to store AWS credentials in a property name earlier in the NAnt build file. This way they won't have to be specified for each task invocation


SNSNAntTask
-----------
This project contains a NAnt task for sending Simple Notification Service messages via Amazon Web Services.


SQSNAntTask
-----------
This project contains a NAnt task for queuing messages via Amazon Web Services Simple Queue Service.



If you have any questions, please feel free to send me an e-mail at lawrence (at) fagan.cc

Regards,

Lawrence Fagan
