using System.ComponentModel.DataAnnotations;

namespace WEB_353502_ZGIRSKAYA.Blazor.SSR.Models
{
    public class CounterModel
    {
        [Required(ErrorMessage = "Значение обязательно для ввода")]
        [Range(1, 10, ErrorMessage = "Значение должно быть целым числом от 1 до 10")]
        public int CountValue { get; set; }
    }
}