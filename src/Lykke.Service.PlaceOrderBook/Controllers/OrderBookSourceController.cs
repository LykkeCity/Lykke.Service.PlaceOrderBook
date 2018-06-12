using System.Net;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("api/[controller]")]
    public class OrderBookSourceController : Controller
    {
        private readonly IOrderbookSourceService _service;

        public OrderBookSourceController(IOrderbookSourceService service)
        {
            _service = service;
        }

        [HttpPost("configuration")]
        [SwaggerOperation("Configuration")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public IActionResult Configuration([FromBody] OrderbookSourceConfigurationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _service.Configure(vm.ToModel());

            return Ok();
        }
    }
}
