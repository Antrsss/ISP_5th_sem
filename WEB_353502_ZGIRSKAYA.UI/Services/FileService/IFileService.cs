namespace WEB_353502_ZGIRSKAYA.UI.Services.FileService
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
    }
}
