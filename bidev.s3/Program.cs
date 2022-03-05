using bidev.lib.s3;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace bidev.s3
{
    public static class CommandExtensions
    {
        public static Command WithHandler(this Command command, string name)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(Program).GetMethod(name, flags);

            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }
    }

    public class Program
    {

        static void UploadFile(string serviceUrl, string accessKey, string secretKey, string bucket, string path, string fullFileName)
        {
            new StorageUtils(serviceUrl, accessKey, secretKey).UploadFile(bucket, path, fullFileName).GetAwaiter().GetResult();
        }
        static void UploadDirectory(string serviceUrl, string accessKey, string secretKey, string bucket, string path, string fullDirectoryName)
        {
            new StorageUtils(serviceUrl, accessKey, secretKey).UploadDirectory(bucket, path, fullDirectoryName).GetAwaiter().GetResult();
        }
        static void GeneratePreSignedDownloadURL(string serviceUrl, string accessKey, string secretKey, string bucket, string fileKey, int durationSeconds)
        {
            var result = new StorageUtils(serviceUrl, accessKey, secretKey).GeneratePreSignedDownloadURL(bucket, fileKey, 1000 * durationSeconds);
            Console.WriteLine(result);
        }

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    var cmd = new RootCommand
                    {
                        new Command("upload", "upload data to S3")
                        {
                            new Command("file", "upload file")
                            {
                                new Argument<string>("fullFileName", "Full File Name"),
                                new Option<string>(new[] { "--serviceUrl", "--service_url", "-su" }, description: "Service Url (for example https://storage.yandexcloud.net)"),
                                new Option<string>(new[] { "--accessKey", "--access_key", "-ak" }, description: "Access Key"),
                                new Option<string>(new[] { "--secretKey", "--secret_key", "-sk" }, description: "Secter Key"),
                                new Option<string>(new[] { "--bucket", "-b" }, description: "Bucket Name"),
                                new Option<string>(new[] { "--path", "-p" }, description: "Path in bucket", getDefaultValue: () => "")
                            }.WithHandler(nameof(UploadFile)),
                            new Command("directory", "upload directory")
                            {
                                new Argument<string>("fullDirectoryName", "Full Directory Name"),
                                new Option<string>(new[] { "--serviceUrl", "--service_url", "-su" }, description: "Service Url (for example https://storage.yandexcloud.net)"),
                                new Option<string>(new[] { "--accessKey", "--access_key", "-ak" }, description: "Access Key"),
                                new Option<string>(new[] { "--secretKey", "--secret_key", "-sk" }, description: "Secter Key"),
                                new Option<string>(new[] { "--bucket", "-b" }, description: "Bucket Name"),
                                new Option<string>(new[] { "--path", "-p" }, description: "Path in bucket", getDefaultValue: () => "")
                            }.WithHandler(nameof(UploadDirectory))
                        },
                        new Command("url", "generate url from S3")
                        {
                            new Command("file", "download file")
                            {
                                new Argument<string>("fileKey", "File Key in bucket"),
                                new Option<string>(new[] { "--serviceUrl", "--service_url", "-su" }, description: "Service Url (for example https://storage.yandexcloud.net)"),
                                new Option<string>(new[] { "--accessKey", "--access_key", "-ak" }, description: "Access Key"),
                                new Option<string>(new[] { "--secretKey", "--secret_key", "-sk" }, description: "Secter Key"),
                                new Option<string>(new[] { "--bucket", "-b" }, description: "Bucket Name"),
                                new Option<int>(new[] { "--durationSeconds", "--duration_seconds", "-d" }, description: "Duration in sceonds url will be active", getDefaultValue: () => 60)
                            }.WithHandler(nameof(GeneratePreSignedDownloadURL)),
                        }
                    };
                    var result = cmd.Invoke(args);
                    return result;
                }
                else
                {
                    throw new ArgumentException("no method passed.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
