using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Extensions;

namespace WEB_353502_ZGIRSKAYA.UI.Services
{
    public class SessionCart : Cart
    {
        private readonly ISession _session;
        private const string CartSessionKey = "cart";

        public SessionCart(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;

            // Загружаем данные из сессии при создании
            var sessionCart = _session.Get<Cart>(CartSessionKey);
            if (sessionCart != null)
            {
                foreach (var item in sessionCart.CartItems)
                    CartItems[item.Key] = item.Value;
            }
        }

        private void SaveChanges()
        {
            _session.Set(CartSessionKey, this);
        }

        public override void AddToCart(Cocktail cocktail)
        {
            base.AddToCart(cocktail);
            SaveChanges();
        }

        public override void RemoveItems(int id)
        {
            base.RemoveItems(id);
            SaveChanges();
        }

        public override void ClearAll()
        {
            base.ClearAll();
            SaveChanges();
        }
    }
}
