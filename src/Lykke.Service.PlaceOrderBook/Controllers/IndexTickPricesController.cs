using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;
using Lykke.Service.PlaceOrderBook.Core;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PlaceOrderBook.Controllers
{
    [Route("api/[controller]")]
    public class IndexTickPricesController : Controller
    {
        private readonly IIndexTickPriceBatchPublisher _indexTickPriceBatchPublisher;
        private readonly ILog _log;

        public IndexTickPricesController(
            IIndexTickPriceBatchPublisher indexTickPriceBatchPublisher,
            ILog log)
        {
            _indexTickPriceBatchPublisher = indexTickPriceBatchPublisher;
            _log = log.CreateComponentScope(nameof(IndexTickPricesController));
        }

        [HttpPost("publish")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PublishTickPrices([FromBody] IndexTickPriceBatchModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var batch = Mapper.Map<IndexTickPriceBatch>(model);

                await _indexTickPriceBatchPublisher.Publish(batch);
            }
            catch (Exception exception)
            {
                await _log.WriteWarningAsync(nameof(PublishTickPrices), model.ToJson(),
                    "Error occurred during publishing tick prices.", exception);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    ErrorMessage = exception.Message
                });
            }

            return Ok();
        }
    }
}
