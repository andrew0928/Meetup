using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra
{
    [Obsolete]
    public class ShopContext
    {
        public int ShopId { get; set; }

        
        [Obsolete]
        public static ShopContext Current { get; set; }
    }
}
