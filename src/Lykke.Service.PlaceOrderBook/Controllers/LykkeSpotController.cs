﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Balances.Client;
using Lykke.Service.PlaceOrderBook.AzureRepositories.Orders;
using Lykke.Service.PlaceOrderBook.Middleware;
using Lykke.Service.PlaceOrderBook.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("spot")]
    public class LykkeSpotController : Controller
    {
        private readonly IBalancesClient _balancesClient;
        private readonly SettingsService _settingsService;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly OrderRepository _orderRepository;

        public LykkeSpotController(
            IBalancesClient balancesClient,
            SettingsService settingsService,
            IMatchingEngineClient matchingEngineClient,
            OrderRepository orderRepository)
        {
            _balancesClient = balancesClient;
            _settingsService = settingsService;
            _matchingEngineClient = matchingEngineClient;
            _orderRepository = orderRepository;
        }

        [XApiKeyAuth]
        [HttpGet("getWallets")]
        public async Task<GetWalletsResponse> GetWallets()
        {
            var response = new GetWalletsResponse();

            var clientId = this.ClientId();

            var balances = (await _balancesClient.GetClientBalances(clientId))
                .ToDictionary(e => e.AssetId, e => e);

            response.Wallets = _settingsService.BalanceAssets.Select(e => new WalletBalanceModel
            {
                Asset = e,
                Balance = balances.ContainsKey(e) ? balances[e].Balance : 0,
                Reserved = balances.ContainsKey(e) ? balances[e].Reserved : 0
            }).ToList();

            return response;
        }

        [XApiKeyAuth]
        [HttpGet("GetLimitOrders")]
        [ProducesResponseType(typeof(GetLimitOrdersResponse), 200)]
        public IActionResult GetOrders(CancellationToken ct)
        {
            return new StatusCodeResult(501);
        }

        [HttpPost("createLimitOrder")]
        [XApiKeyAuth]
        public async Task<IActionResult> CreateLimitOrder([FromBody] LimitOrderRequest order)
        {
            var orderId = Guid.NewGuid().ToString();

            var mlm = new MultiLimitOrderModel
            {
                AssetId = order.Instrument,
                ClientId = this.ClientId(),
                CancelMode = CancelMode.BothSides,
                CancelPreviousOrders = false,
                Id = Guid.NewGuid().ToString(),
                Orders = new List<MultiOrderItemModel>
                {
                    new MultiOrderItemModel
                    {
                        Id = orderId,
                        OrderAction = order.TradeType == TradeType.Buy ? OrderAction.Buy : OrderAction.Sell,
                        Fee = null,
                        OldId = null,
                        Price = (double) order.Price,
                        Volume = (double) order.Volume
                    }
                }
            };

            var res = await _matchingEngineClient.PlaceMultiLimitOrderAsync(mlm);

            var status = res.Statuses.Any() ? res.Statuses[0].Status : MeStatusCodes.BadRequest;

            if (status == MeStatusCodes.Ok)
            {
                var resp = new OrderIdResponse
                {
                    OrderId = orderId
                };

                return Ok(resp);
            }

            return BadRequest($"incorrect result: {status}");
        }

        [XApiKeyAuth]
        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelLimitOrderRequest request)
        {
            if (request?.OrderId == null)
            {
                return BadRequest("Incorrect parameters - null");
            }

            await _matchingEngineClient.CancelLimitOrderAsync(request.OrderId);
            
            var resp = new CancelLimitOrderResponse
            {
                OrderId = request.OrderId
            };
            
            return Ok(resp);
        }

        [XApiKeyAuth]
        [HttpGet("LimitOrderStatus")]
        public async Task<IActionResult> GetOrderStatus(string orderId)
        {
            var order = await _orderRepository.GetOrder(this.ClientId(), orderId);
            if (order == null)
                await Task.Delay(5000);

            order = await _orderRepository.GetOrder(this.ClientId(), orderId);
            if (order == null)
                await Task.Delay(5000);

            order = await _orderRepository.GetOrder(this.ClientId(), orderId);
            if (order == null)
                return BadRequest("Order not found");

            var result = new OrderModel
            {
                Id = order.OrderId,
                AvgExecutionPrice = order.AvgExecutionPrice,
                ExecutionStatus = Enum.Parse<OrderStatus>(order.Status),
                Price = order.Price,
                TradeType = Enum.Parse<TradeType>(order.TradeType),
                RemainingAmount = order.RemainingAmount,
                Symbol = order.Instrument,
                ExecutedVolume = order.ExecutedAmount,
                Timestamp = order.CreatedTime,
                OriginalVolume = order.OriginalAmount
            };

            return Ok(result);
        }
    }
}
