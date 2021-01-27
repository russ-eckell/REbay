using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using REbay.Models;
using REbay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REBay.Controllers
{
    [Produces("application/json")]
    [Route("api/items")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class ItemsController : Controller
    {
        private readonly ItemService itemService;
        public ItemsController(ItemService itemService)
        {
            this.itemService = itemService;
        }

        // GET Completed Items
        [HttpGet]
        public JsonResult Get(string KeywordSearch)
        {
            SearchResponse searchResponse = new SearchResponse();
            searchResponse = itemService.GetSearchResponse(KeywordSearch);

            // return an empty result if nothing is found in the search.
            if (searchResponse == null)
            {
                return Json(new EmptyResult());
            }

            return Json(searchResponse);
        }
    }
}
