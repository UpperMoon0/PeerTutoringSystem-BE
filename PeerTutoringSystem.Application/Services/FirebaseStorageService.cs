using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                        return privateKey; 
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
                // Firebase Storage URLs are typically in the format:
                // https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{folder}%2F{fileName}?alt=media&token={token}
                // We need to extract the folder and fileName from the path.
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