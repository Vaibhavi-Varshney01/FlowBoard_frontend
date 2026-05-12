using Amazon.S3;
using Amazon.S3.Model;

namespace FlowBoard.Comment.Storage
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteFileAsync(string fileUrl);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client   = s3Client;
            _bucketName = configuration["AWS:BucketName"]
                ?? throw new InvalidOperationException("AWS:BucketName not configured.");
        }

        /// <summary>
        /// Uploads a file to AWS S3 and returns the public URL.
        /// </summary>
        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            // Make key unique to avoid overwrite collisions
            var key = $"attachments/{Guid.NewGuid()}_{fileName}";

            var request = new PutObjectRequest
            {
                BucketName  = _bucketName,
                Key         = key,
                InputStream = fileStream,
                ContentType = contentType,
                // Public read so card members can view attachments directly
                CannedACL   = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            // Return public S3 URL
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        /// <summary>
        /// Deletes a file from AWS S3 using its public URL.
        /// </summary>
        public async Task DeleteFileAsync(string fileUrl)
        {
            // Extract the S3 key from the URL
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key        = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}