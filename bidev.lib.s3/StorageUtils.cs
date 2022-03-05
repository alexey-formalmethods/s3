using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using bidev.lib.s3.Constants;
using bidev.lib.s3.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace bidev.lib.s3
{
    public class StorageUtils
    {
        private AmazonS3Config s3Config;
        private BasicAWSCredentials s3Credentials;
        public StorageUtils(string serviceUrl, string accessKey, string secretKey)
        {
            this.s3Config = new AmazonS3Config { ServiceURL = serviceUrl }; 
            this.s3Credentials = new BasicAWSCredentials(accessKey, secretKey);
        }
        public async Task WriteTextToFile(string buketName, string fileKey, string fileContent)
        {
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                var request = new Amazon.S3.Model.PutObjectRequest()
                {
                    Key = fileKey,
                    ContentBody = fileContent,
                    BucketName = buketName, 
                    //ContentType = "text/plain",
                    StorageClass = S3StorageClass.Standard

                };
                var result = await s3Client.PutObjectAsync(request);
            }

        }
        public async Task WriteTextToFile(string buketName, string path, string fileName, string fileContent)
        {
            await WriteTextToFile(buketName, path + "/" + fileName, fileContent);
        }
        public async Task<ICollection<FileModel>> GetFiles(string buketName, string path)
        {
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {

                var result = await s3Client.ListObjectsAsync(buketName, path);
                var files = result.S3Objects.Select(x => 
                    new FileModel() 
                    { 
                        FileKey = x.Key, 
                        FileName = Path.GetFileName(x.Key) 
                    }
                ).ToList();
                return files;
            }
        }
        public async Task UploadDirectory(string buketName, string path, string directoryName)
        {
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                TransferUtility tu = new TransferUtility(s3Client);
                var request = new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = buketName,
                    Directory = directoryName,
                    KeyPrefix = path,
                    SearchOption = SearchOption.AllDirectories,
                    SearchPattern = "*.*"
                };
                await tu.UploadDirectoryAsync(request, CancellationToken.None);
               }
        }
        public async Task UploadFile(string buketName, string path, string fileName)
        {
            try
            {
                using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
                {
                    TransferUtility tu = new TransferUtility(s3Client);
                    var request = new TransferUtilityUploadRequest
                    {
                        BucketName = buketName,
                        FilePath = fileName,
                        Key = string.IsNullOrEmpty(path) ? "" : path + "/" + new FileInfo(fileName).Name,

                    };
                    await tu.UploadAsync(request, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
        public async Task<bool> CheckFileExists(string buketName, string fileKey)
        {
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                try
                {
                    var result = await s3Client.GetObjectAsync(buketName, fileKey);
                    return true;
                }
                catch (AmazonS3Exception e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }
        public async Task DownloadFileAsync(string buketName, string fileKey, string pathToDownload)
        {
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                var result = await s3Client.GetObjectAsync(buketName, fileKey);
                await result.WriteResponseStreamToFileAsync($@"{pathToDownload}/{fileKey}", false, CancellationToken.None);
            }
        }
        public string GeneratePreSignedURL(string buketName, string path, string fileName, double durationMilliseconds, string contentType)
        {

            var request = new GetPreSignedUrlRequest
            {
                BucketName = buketName,
                Key = path + "/" + fileName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMilliseconds(durationMilliseconds),
                ContentType = contentType

            };
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                string url = s3Client.GetPreSignedURL(request);
                return url;
            }
        }
        public string GeneratePreSignedDownloadURL(string buketName, string fileKey, double durationMilliseconds)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = buketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMilliseconds(durationMilliseconds)
            };
            using (var s3Client = new AmazonS3Client(s3Credentials, s3Config))
            {
                string url = s3Client.GetPreSignedURL(request);
                return url;
            }

        }
    }
}
