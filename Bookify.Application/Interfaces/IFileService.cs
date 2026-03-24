using Microsoft.AspNetCore.Http;

namespace Bookify.Application.Interfaces;

/// <summary>
/// Contract for cloud file storage operations (images only).
/// </summary>
public interface IFileService
{
    /// <summary>Checks whether a file exists in the given folder.</summary>
    Task<bool> IsExsisted(string fileName, string folderName);

    /// <summary>Uploads a .jpg/.jpeg/.png (max 2 MB) and returns the public URL.</summary>
    Task<string> Upload(IFormFile imageFile, string folderName, string? customFileName = null);

    /// <summary>Deletes a file from the given folder.</summary>
    Task<bool> Delete(string fileName, string folderName);
}
