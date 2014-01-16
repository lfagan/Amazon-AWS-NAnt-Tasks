Amazon Web Services NAnt Tasks
==============================

This repository contains a Visual Studio 2010 solution with three projects:

S3NAntTask
----------
This project contains the following NAnt tasks for interacting with Amazon S3 storage:

* amazon-s3 (DEPRECATED) - included for compatibility. The task will check to see if the bucket exists before sending the file. If the bucket doesn't exist, it will create the bucket before sending the file.
* amazon-s3-CreateBucket - creates a new bucket
* amazon-s3-DeleteBucket - deletes an *empty* bucket
* amazon-s3-putFile - uploads a file into a bucket
* amazon-s3-getFile - downloads a file from a bucket
* amazon-s3-deleteFile - deletes a file from a bucket

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
