using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace REbay.Models
{
    public class CompletedItem
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public string EndDate { get; set; }
        public bool WasSold { get; set; }
        public string ItemPrice { get; set; }
        public string ShippingPrice { get; set; }
        public bool WasBuyItNow { get; set; }
        public string ItemLocation { get; set; }
        public string thumbnailUrl { get; set; }
        public string url { get; set; }
    }
}
