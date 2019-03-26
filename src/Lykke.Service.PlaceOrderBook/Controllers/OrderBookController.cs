using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.PlaceOrderBook.Client.Models.OrderBooks;
using Lykke.Service.PlaceOrderBook.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("api/[controller]")]
    public class OrderBookController : Controller
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly SettingsService _settingsService;

        public OrderBookController(
            IMatchingEngineClient matchingEngineClient,
            SettingsService settingsService)
        {
            _matchingEngineClient = matchingEngineClient;
            _settingsService = settingsService;
        }

        [HttpPost]
        [SwaggerOperation("UpdateOrderBook")]
        [ProducesResponseType(typeof(List<LimitOrderStatusModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateOrderBook([FromBody] UpdateOrderBookModel model,
            [FromQuery] bool cancelPrevious,
            [FromQuery] long? timeBetweenPlacementMilliseconds)
        {
            if (!_settingsService.TrustedClients.Contains(model.ClientId))
            {
                return BadRequest("Client not supported (not trusted)");
            }

            if (timeBetweenPlacementMilliseconds.HasValue)
            {
                if (cancelPrevious)
                {
                    await _matchingEngineClient.MassCancelLimitOrdersAsync(new LimitOrderMassCancelModel
                    {
                        AssetPairId = model.AssetPair,
                        ClientId = model.ClientId,
                        Id = Guid.NewGuid().ToString()
                    });
                }

                var result = new List<MeResponseModel>();
                
                foreach (var order in model.Orders)
                {
                    result.Add(await _matchingEngineClient.PlaceLimitOrderAsync(new Lykke.MatchingEngine.Connector.Abstractions.Models.LimitOrderModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        AssetPairId = model.AssetPair,
                        ClientId = model.ClientId,
                        CancelPreviousOrders = false,
                        OrderAction = order.TradeType == "Buy" ? OrderAction.Buy : OrderAction.Sell,
                        Price = order.Price,
                        Volume = order.Volume
                    }));

                    await Task.Delay(TimeSpan.FromMilliseconds(timeBetweenPlacementMilliseconds.Value));
                }

                return Ok(result);
            }
            else
            {
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
        public async Task<IActionResult> ReplaceLimitOrder([FromBody] Client.Models.OrderBooks.LimitOrderModel model,
            [FromQuery] string clientId, [FromQuery] string assetPair, [FromQuery] string oldOrderId)
        {
            if (!_settingsService.TrustedClients.Contains(oldOrderId))
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
