using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace MyShopAPI.Services
{
    /// <summary>
    /// Service for processing image URLs - either external HTTP or static files.
    /// </summary>
    public interface IImageDownloadService
    {
        /// <summary>
        /// Processes an image URL - returns HTTP URLs directly or resolves static filenames.
        /// </summary>
        Task<string?> ProcessImageUrlAsync(string imageUrl);
    }

    public class ImageDownloadService : IImageDownloadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _imageFolder = "images/products";

        public ImageDownloadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task<string?> ProcessImageUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return Task.FromResult<string?>(null);

            imageUrl = imageUrl.Trim();

            // If it's an HTTP/HTTPS URL, use it directly
            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<string?>(imageUrl);
            }

            // Otherwise treat as static filename from wwwroot
            var filePath = Path.Combine(_environment.WebRootPath, _imageFolder, imageUrl);
            
            if (File.Exists(filePath))
                return Task.FromResult<string?>($"/{_imageFolder}/{imageUrl}");
            
            return Task.FromResult<string?>(null);
        }
    }
}
