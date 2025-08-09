using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.Domain.Entities
{
    public class CocktailCategory
    {
        private int id = 0;
        public int Id
        {
            get { return id; }
            set { if (value > 0) id = value; }
        }

        public string? Name { get; set; }
        public string? NormilisedName { get; set; }
    }
}
