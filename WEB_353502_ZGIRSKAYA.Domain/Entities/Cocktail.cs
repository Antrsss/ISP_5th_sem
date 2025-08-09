using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.Domain.Entities
{
    public class Cocktail
    {
        private int id = 0;
        public int Id
        {
            get { return id; }
            set { if (value > 0) id = value; }
        }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public CocktailCategory? Category { get; set; }

        private double price = 0;
        public double Price
        {
            get { return price; }
            set
            {
                if (value > 0)
                {
                    price = value;
                }
            }
        }

        public string? PathToPicture { get; set; }
        public string? MimeType { get; set; }
    }
}
