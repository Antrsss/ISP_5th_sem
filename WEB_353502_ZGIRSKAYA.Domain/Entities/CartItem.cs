using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.Domain.Entities
{
    public class CartItem
    {
        public Cocktail Cocktail { get; set; } = new();
        public int Quantity { get; set; } = 1;
    }
}