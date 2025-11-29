using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.Domain.Entities
{
    public class Cart
    {
        public Dictionary<int, CartItem> CartItems { get; set; } = new();

        public virtual void AddToCart(Cocktail cocktail)
        {
            if (CartItems.ContainsKey(cocktail.Id))
            {
                CartItems[cocktail.Id].Quantity++;
            }
            else
            {
                CartItems.Add(cocktail.Id, new CartItem { Cocktail = cocktail, Quantity = 1 });
            }
        }

        public virtual void RemoveItems(int id)
        {
            if (CartItems.ContainsKey(id))
            {
                CartItems.Remove(id);
            }
        }

        public virtual void ClearAll()
        {
            CartItems.Clear();
        }

        public int Count { get => CartItems.Sum(item => item.Value.Quantity); }

        public double TotalPrice { get => CartItems.Sum(item => item.Value.Cocktail.Price * item.Value.Quantity); }
    }
}