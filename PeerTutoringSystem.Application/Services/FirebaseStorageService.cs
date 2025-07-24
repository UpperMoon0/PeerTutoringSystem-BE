using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Threading;

namespace PeerTutoringSystem.Application.Services
{
    public class FirebaseStorageService
    {
        private readonly FirebaseStorage _firebaseStorage;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration configuration)
        {
            _bucketName = configuration["Firebase:BucketName"];
            if (string.IsNullOrEmpty(_bucketName))
            {
                throw new ArgumentNullException(nameof(_bucketName), "Firebase BucketName is not configured.");
            }

            var privateKey = configuration["Firebase:PrivateKey"];
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException(nameof(privateKey), "Firebase PrivateKey is not configured.");
            }
            privateKey = privateKey.Replace("\\n", "\n");

            var authEmail = configuration["Firebase:AuthEmail"];
            if (string.IsNullOrEmpty(authEmail))
            {
                throw new ArgumentNullException(nameof(authEmail), "Firebase AuthEmail is not configured.");
            }

            _firebaseStorage = new FirebaseStorage(
                _bucketName,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = async () =>
                    {
                        var initializer = new ServiceAccountCredential.Initializer(authEmail)
                        {
                            Scopes = new[] { "https://www.googleapis.com/auth/firebase.storage" }
                        }.FromPrivateKey(privateKey);
                        var credential = new ServiceAccountCredential(initializer);
                        var token = await credential.GetAccessTokenForRequestAsync();
                        return token;
                    },
                    ThrowOnCancel = true
                });
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var storagePath = $"{folder}/{fileName}";

            using (var stream = file.OpenReadStream())
            {
                var task = _firebaseStorage
                    .Child(folder)
                    .Child(fileName)
                    .PutAsync(stream);

                return await task;
            }
        }

        public async Task<(byte[] content, string contentType, string fileName)> DownloadFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("File URL is empty or null.");
            }

            var uri = new Uri(fileUrl);
            var path = Uri.UnescapeDataString(uri.AbsolutePath);
            var parts = path.Split(new[] { "/o/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1)
            {
                throw new ArgumentException("Invalid Firebase Storage URL format.");
            }

            var filePathInBucket = parts[1];
            var folderAndFileName = filePathInBucket.Split('/');
            if (folderAndFileName.Length < 2)
            {
                throw new ArgumentException("Invalid file path in bucket.");
            }

            var folder = folderAndFileName[0];
            var fileName = string.Join("/", folderAndFileName.Skip(1));

            var downloadUrl = await _firebaseStorage
                .Child(folder)
                .Child(fileName)
                .GetDownloadUrlAsync();

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);
                var contentType = GetContentType(fileName);
                return (fileBytes, contentType, fileName);
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream",
            };
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return;
            }

            try
            {
                var uri = new Uri(fileUrl);
                var path = Uri.UnescapeDataString(uri.AbsolutePath);
                var parts = path.Split(new[] { "/o/" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var filePathInBucket = parts[1];
                    var folderAndFileName = filePathInBucket.Split('/');
                    if (folderAndFileName.Length >= 2)
                    {
                        var folder = folderAndFileName[0];
                        var fileName = string.Join("/", folderAndFileName.Skip(1));

                        await _firebaseStorage
                            .Child(folder)
                            .Child(fileName)
                            .DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file from Firebase Storage: {ex.Message}");
            }
        }
    }
}