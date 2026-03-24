using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Services.Files;

public class FileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

    public FileService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<bool> IsExsisted(string fileName, string FolderName)
    {
        var publicId = $"{FolderName}/{Path.GetFileNameWithoutExtension(fileName)}";
        
        var getResourceParams = new GetResourceParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        try
        {
            var result = await _cloudinary.GetResourceAsync(getResourceParams);
            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> Upload(IFormFile imageFile, string FolderName, string? customFileName = null)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("File is empty");

        if (imageFile.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds 2MB limit");

        var extension = Path.GetExtension(imageFile.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException("Only .jpg, .jpeg, and .png files are allowed");

        using var stream = imageFile.OpenReadStream();

        if (stream.Length < 4)
            throw new ArgumentException("Invalid file structure. File is too small.");

        var headerBytes = new byte[4];
        stream.ReadExactly(headerBytes, 0, 4);

        // JPEG Signature: FF D8 FF
        bool isJpeg = headerBytes[0] == 0xFF && headerBytes[1] == 0xD8 && headerBytes[2] == 0xFF;
        
        // PNG Signature: 89 50 4E 47
        bool isPng = headerBytes[0] == 0x89 && headerBytes[1] == 0x50 && headerBytes[2] == 0x4E && headerBytes[3] == 0x47;

        if (!isJpeg && !isPng)
            throw new ArgumentException("Invalid file signature. File contents do not match a valid image format.");

        // Reset stream position so the Cloudinary uploader reads from the start
        stream.Position = 0;

        var publicId = !string.IsNullOrEmpty(customFileName)
            ? Path.GetFileNameWithoutExtension(customFileName)
            : Path.GetFileNameWithoutExtension(imageFile.FileName);
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageFile.FileName, stream),
            Folder = FolderName,
            PublicId = publicId,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception(uploadResult.Error.Message);

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<bool> Delete(string fileName, string FolderName)
    {
        var publicId = $"{FolderName}/{Path.GetFileNameWithoutExtension(fileName)}";
        
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Result == "ok";
    }
}
