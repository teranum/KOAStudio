using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KOAStudio.Core.Models
{
    internal class OrderModel
    {
        public OrderType 매매구분 { get; set; }
        public OrderKind 주문종류 { get; set; }
    }
}
