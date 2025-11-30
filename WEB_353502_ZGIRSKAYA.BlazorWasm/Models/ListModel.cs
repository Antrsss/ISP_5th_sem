namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Models
{
    public class ListModel<T>
    {
        public List<T> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
