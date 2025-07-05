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
            var privateKey = configuration["Firebase:PrivateKey"].Replace("\\n", "\n");
            var authEmail = configuration["Firebase:AuthEmail"];

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