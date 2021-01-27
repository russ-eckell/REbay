using REbay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.IO;


namespace REbay.Services
{
    public class ItemService
    {
        public SearchResponse GetSearchResponse(string keywordSearch)
        {
            SearchResponse searchResponse = new SearchResponse();
            List<ItemPrice> itemPrices = new List<ItemPrice>();

            // Should probably move this to a DB once we have this working so a user could update it if eBay changes their URL format.
            string ebayUrl = String.Format("https://www.ebay.com/sch/i.html?_from=R40&_nkw={0}&LH_PrefLoc=1&LH_Complete=1", keywordSearch);

            var response = CallUrl(ebayUrl).Result;

            searchResponse.CompletedItems = BuildCompletedItemsList(response, keywordSearch);

            if (!searchResponse.CompletedItems.Any())
            {
                return null;
            }

            // Now that we have our items, we can do a little analysis.
            searchResponse.Count = searchResponse.CompletedItems.Count();
            
            // Populate our temporary object that we can use to do some calcs.
            foreach(CompletedItem item in searchResponse.CompletedItems)
            {
                ItemPrice itemPrice = new ItemPrice();
                itemPrice.PriceLabel = item.ItemPrice;
                itemPrice.WasSold = item.WasSold;

                // There's a special case where ebay provides a RANGE of prices, formatted "$X.XX to $X.XX". To be least risky to the user, let's take the lowest number.
                if (item.ItemPrice.Contains("to")) {
                    int index = item.ItemPrice.IndexOf(" to");
                    item.ItemPrice = item.ItemPrice.Substring(0, index-1);
                }
                itemPrice.Price = decimal.Parse(item.ItemPrice, System.Globalization.NumberStyles.AllowCurrencySymbol | System.Globalization.NumberStyles.Number);
                itemPrices.Add(itemPrice);
            }

            //temporary variable we'll use to hold some data before converting it back to a string for display on the page.
            decimal tempDecimal;
            string na = "N/A";

            if (itemPrices.Where(w => w.WasSold == true).Any()) { 
                // Get Median Sold Price
                tempDecimal = GetMedian(itemPrices.Where(w => w.WasSold == true).Select(s => s.Price).ToArray());
                searchResponse.MedianSoldPrice = String.Format("${0}",Math.Round(tempDecimal));

                //Get the highest sold price
                tempDecimal = itemPrices.Where(w => w.WasSold == true).Select(s => s.Price).Max();
                searchResponse.HighestSoldPrice = String.Format("${0}", Math.Round(tempDecimal));

                //Get the lowest sold price
                tempDecimal = itemPrices.Where(w => w.WasSold == true).Select(s => s.Price).Min();
                searchResponse.LowestSoldPrice = String.Format("${0}", Math.Round(tempDecimal));
            
                //Get the sold to unsold percentage
                int numberSold = itemPrices.Where(w => w.WasSold == true).Count();
                searchResponse.SellRate = Math.Round((numberSold / (decimal)searchResponse.Count) * 100).ToString("#.##");
            } else
            {
                searchResponse.SellRate = "0%";
                searchResponse.MedianSoldPrice = na;
                searchResponse.HighestSoldPrice = na;
                searchResponse.LowestSoldPrice = na;
            }

            if (itemPrices.Where(w => w.WasSold == false).Any())
            {
                //Get the Lowest Unsold Price
                tempDecimal = itemPrices.Where(w => w.WasSold == false).Select(s => s.Price).Min();
                searchResponse.LowestUnsoldPrice = String.Format("${0}", Math.Round(tempDecimal));
            } else
            {
                searchResponse.LowestUnsoldPrice = na;
            }
            
            return searchResponse;
        }

        public decimal GetMedian(decimal[] array)
        {
            decimal[] tempArray = array;
            int count = tempArray.Length;

            Array.Sort(tempArray);

            decimal medianValue = 0;

            if (count % 2 == 0)
            {
                // count is even, need to get the middle two elements, add them together, then divide by 2
                decimal middleElement1 = tempArray[(count / 2) - 1];
                decimal middleElement2 = tempArray[(count / 2)];
                medianValue = (middleElement1 + middleElement2) / 2;
            }
            else
            {
                // count is odd, simply get the middle element.
                medianValue = tempArray[(count / 2)];
            }

            return medianValue;
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private List<CompletedItem> BuildCompletedItemsList(string html, string keyWords)
        {
            bool isDirectMatch;
            List<CompletedItem> completedItems = new List<CompletedItem>();
            CompletedItem completedItem = new CompletedItem();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var productsHtml = htmlDocument.DocumentNode.Descendants("ul")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("srp-results")).ToList();


            var productListItems = productsHtml[0].Descendants("li")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("s-item")).ToList();
            
            foreach (var productListItem in productListItems)
            {
                isDirectMatch = true;
                // We do not want to include International for now, so if the listing is international, we want to take it out of our list.
                string location = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__location")).FirstOrDefault());
                if (location != "")
                {
                    continue; //on to the next listing
                }

                // Weed out any "related" items ebay wants to throw in there by double-checking our keywords against the title
                string[] keyWordsArray = keyWords.Split(" ");

                string title = GetNodeInnerText(productListItem.Descendants("h3")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__title")).FirstOrDefault());

                foreach (string keyWord in keyWordsArray)
                {
                    if (title.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        isDirectMatch = false;
                    }
                }
                if (!isDirectMatch)
                {
                    continue;
                }

                completedItem = new CompletedItem()
                {
                    Title = title,
                    Condition = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("SECONDARY_INFO")).FirstOrDefault()),
                    ItemPrice = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__price")).FirstOrDefault()),
                    ShippingPrice = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__shipping")).FirstOrDefault()),
                    //ItemLocation = GetNodeInnerText(productListItem.Descendants("span")
                    //    .Where(node => node.GetAttributeValue("class", "")
                    //    .Contains("s-item__location")).FirstOrDefault()),
                    thumbnailUrl = productListItem.Descendants("img")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__image-img")).FirstOrDefault().GetAttributeValue("src",""),
                    url = productListItem.Descendants("a")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("s-item__link")).FirstOrDefault().GetAttributeValue("href", ""),
                    WasSold = !productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("NEGATIVE")).Any(),
                    WasBuyItNow = productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("s-item__purchase-options-with-icon")).Any()
                };

                if (completedItem.WasSold)
                {
                    completedItem.EndDate = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("POSITIVE")).FirstOrDefault());
                } else
                {
                    completedItem.EndDate = GetNodeInnerText(productListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Contains("NEGATIVE")).FirstOrDefault());
                }

                //Extract the ebay identifier from the the URL. Probably a better way to do this via RegEx, but for now I'm just looking for everything between
                // the last slash in the URL and the question mark, which is where the Item ID lives.
                int slashIndex = completedItem.url.LastIndexOf("/") + 1;
                int questionMarkIndex = completedItem.url.LastIndexOf("?");
                completedItem.ItemId = completedItem.url.Substring(slashIndex, questionMarkIndex - slashIndex);

                completedItems.Add(completedItem);
            }

            return completedItems;
        }

        private string GetNodeInnerText(HtmlNode? htmlNode)
        {
            string returnValue = "";
            if (htmlNode != null)
            {
                returnValue = htmlNode.InnerText;
            }
            return returnValue;
        }
    }

    public class ItemPrice
    {
        public string PriceLabel;
        public decimal Price;
        public bool WasSold;

    }



}
