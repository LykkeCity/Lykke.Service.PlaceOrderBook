﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Balances.Client;
using Lykke.Service.PlaceOrderBook.AzureRepositories;
using Lykke.Service.PlaceOrderBook.Middleware;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("spot")]
    public class LykkeSpotController : Controller
    {
        private readonly IBalancesClient _balancesClient;
        private readonly PlaceOrderBookSettings _settings;
        private readonly IMatchingEngineClient _meclient;
        private readonly OrderRepository _orderRepository;

        public LykkeSpotController(IBalancesClient balancesClient, PlaceOrderBookSettings settings, IMatchingEngineClient meclient,
            OrderRepository orderRepository)
        {
            _balancesClient = balancesClient;
            _settings = settings;
            _meclient = meclient;
            _orderRepository = orderRepository;
        }

        [HttpGet("getWallets")]
        [XApiKeyAuth]
        [SwaggerOperation("GetWallets")]
        public async Task<WalletsResponse> GetWallets()
        {
            var responce = new WalletsResponse();

            var clientId = this.ClientId();

            var balaces = (await _balancesClient.GetClientBalances(clientId)).ToDictionary(e => e.AssetId, e => e);

            responce.Wallets = _settings.BalanceAssets.Select(e => new Wallet()
            {
                Asset = e,
                Balance = balaces.ContainsKey(e) ? balaces[e].Balance : 0,
                Reserved = balaces.ContainsKey(e) ? balaces[e].Reserved : 0
            }).ToList();

            return responce;
        }

        [HttpGet("GetLimitOrders")]
        [XApiKeyAuth]
        [SwaggerOperation("GetOrders")]
        [ProducesResponseType(typeof(GetLimitOrdersResponse), 200)]
        public IActionResult GetOrders(CancellationToken ct)
        {
            return new StatusCodeResult(501);
        }

        [HttpPost("createLimitOrder")]
        [XApiKeyAuth]
        public async Task<IActionResult> CreateLimitOrder([FromBody] CreateLimitOrderCommand order)
        {
            var orderId = Guid.NewGuid().ToString();
            var mlm = new MultiLimitOrderModel()
            {
                AssetId = order.Instrument,
                ClientId = this.ClientId(),
                CancelMode = CancelMode.BothSides,
                CancelPreviousOrders = false,
                Id = Guid.NewGuid().ToString(),
                Orders = new List<MultiOrderItemModel> {
                    new MultiOrderItemModel()
                        {
                            Id = orderId,
                            OrderAction = order.TradeType == TradeType.Buy ? OrderAction.Buy : OrderAction.Sell,
                            Fee = null,
                            OldId = null,
                            Price = (double)order.Price,
                            Volume = (double)order.Amount
                        }
                    }
            };

            var res = await _meclient.PlaceMultiLimitOrderAsync(mlm);

            var status = res.Statuses.Any() ? res.Statuses[0].Status : MeStatusCodes.BadRequest;
            if (status == MeStatusCodes.Ok)
            {
                var resp = new LimitOrderCreated()
                {
                    Id = orderId
                };

                return Ok(resp);
            }

            return BadRequest($"incorect result: {status}");
        }

        [HttpPost("cancelOrder")]
        [XApiKeyAuth]
        [SwaggerOperation("CancelLimitOrder")]
        [ProducesResponseType(typeof(ContainsOrderId), 200)]
        public async Task<IActionResult> CancelLimitOrder(ContainsOrderId request)
        {
            await _meclient.CancelLimitOrderAsync(request.OrderId);
            return Ok();
        }

        [HttpGet("LimitOrderStatus")]
        [XApiKeyAuth]
        [SwaggerOperation("GetOrderStatus")]
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

            var result = new Lykke.Common.ExchangeAdapter.Contracts.Order()
            {
                OrderId = order.OrderId,
                AvgExecutionPrice = order.AvgExecutionPrice,
                Status = Enum.Parse<OrderStatus>(order.Status),
                Price = order.Price,
                TradeType = Enum.Parse<TradeType>(order.TradeType),
                RemainingAmount = order.RemainingAmount,
                Instrument = order.Instrument,
                ExecutedAmount = order.ExecutedAmount,
                CreatedTime = order.CreatedTime,
                OriginalAmount = order.OriginalAmount
            };

            return Ok(result);
        }
    }
}
