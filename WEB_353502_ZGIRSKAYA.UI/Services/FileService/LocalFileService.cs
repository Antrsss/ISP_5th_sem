namespace WEB_353502_ZGIRSKAYA.UI.Services.FileService
{
    public class LocalFileService(IWebHostEnvironment environment) : IFileService
    {
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            // сгенерировать имя файла
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // получить путь к сохраняемому файлу
            var imagesPath = Path.Combine(environment.WebRootPath, "Images");
            var filePath = Path.Combine(imagesPath, fileName);

            // создать папку Images, если она не существует
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            // скопировать файл в поток
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("Images", fileName);
        }
    }
}
