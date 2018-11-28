using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.PlaceOrderBook.Client.Models.OrderBooks;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("api/[controller]")]
    public class OrderBookController : Controller
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly PlaceOrderBookSettings _settings;

        public OrderBookController(
            IMatchingEngineClient matchingEngineClient,
            PlaceOrderBookSettings settings)
        {
            _matchingEngineClient = matchingEngineClient;
            _settings = settings;
        }

        [HttpPost]
        [SwaggerOperation("UpdateOrderBook")]
        [ProducesResponseType(typeof(List<LimitOrderStatusModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateOrderBook([FromBody] UpdateOrderBookModel model,
            [FromQuery] bool cancelPrevious)
        {
            if (!_settings.TrustedClientIds.Contains(model.ClientId))
            {
                return BadRequest("Client not supported (not trusted)");
            }

            var mlm = new MultiLimitOrderModel
            {
                AssetId = model.AssetPair,
                ClientId = model.ClientId,
                CancelMode = CancelMode.BothSides,
                CancelPreviousOrders = cancelPrevious,
                Id = Guid.NewGuid().ToString(),
                Orders = model.Orders.Select(e => new MultiOrderItemModel
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderAction = e.TradeType == "Buy" ? OrderAction.Buy : OrderAction.Sell,
                    Fee = null,
                    OldId = null,
                    Price = e.Price,
                    Volume = e.Volume
                }).ToList()
            };

            var res = await _matchingEngineClient.PlaceMultiLimitOrderAsync(mlm);
            return Ok(res.Statuses.ToList());
        }

        [HttpPost("Cancel")]
        [SwaggerOperation("UpdateOrderBook")]
        [ProducesResponseType(typeof(MeResponseModel), (int) HttpStatusCode.OK)]
        public async Task<MeResponseModel> CancelOrder(string orderId)
        {
            var res = await _matchingEngineClient.CancelLimitOrderAsync(orderId);
            return res;
        }

        [HttpPost("ReplaceLimitOrder")]
        [SwaggerOperation("ReplaceLimitOrder")]
        [ProducesResponseType(typeof(List<LimitOrderStatusModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ReplaceLimitOrder([FromBody] OrderModel model, [FromQuery] string clientId,
            [FromQuery] string assetPair, [FromQuery] string oldOrderId)
        {
            if (!_settings.TrustedClientIds.Contains(oldOrderId))
                return BadRequest("Client not supported (not trusted)");

            var mlm = new MultiLimitOrderModel()
            {
                AssetId = assetPair,
                ClientId = clientId,
                CancelMode = CancelMode.BothSides,
                CancelPreviousOrders = false,
                Id = Guid.NewGuid().ToString(),
                Orders = new[]
                {
                    new MultiOrderItemModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderAction = model.TradeType == "Buy" ? OrderAction.Buy : OrderAction.Sell,
                        Fee = null,
                        OldId = oldOrderId,
                        Price = model.Price,
                        Volume = model.Volume
                    }
                }
            };
            
            var res = await _matchingEngineClient.PlaceMultiLimitOrderAsync(mlm);
            
            return Ok(res.Statuses.ToList());
        }
    }
}
