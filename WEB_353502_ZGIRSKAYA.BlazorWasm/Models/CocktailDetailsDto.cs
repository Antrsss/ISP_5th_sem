namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Models
{
    public class CocktailDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Price { get; set; }
        public string? CategoryName { get; set; }
        public string? PathToPicture { get; set; }
        public string? MimeType { get; set; }
        public string? Ingredients { get; set; }
        public string? PreparationMethod { get; set; }
        public int? AlcoholPercentage { get; set; }
        public int? VolumeMl { get; set; }
    }
}