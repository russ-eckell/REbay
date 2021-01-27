using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace REbay.Models
{
    public class SearchResponse
    {
        public string MedianSoldPrice { get; set; }
        public string HighestSoldPrice { get; set; }
        public string LowestSoldPrice { get; set; }
        public string LowestUnsoldPrice { get; set; }
        public string SellRate { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// A list of the completed items
        /// </summary>
        public List<CompletedItem> CompletedItems { get; set; }
        
    }
}
