namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Models
{
    public class CocktailListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double Price { get; set; }
        public string? CategoryName { get; set; }
        public string? PathToPicture { get; set; }
    }
}
