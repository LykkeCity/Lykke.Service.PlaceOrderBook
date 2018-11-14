using FluentValidation;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;

namespace Lykke.Service.PlaceOrderBook.Validators
{
    public class IndexTickPriceBatchModelValidator : AbstractValidator<IndexTickPriceBatchModel>
    {
        public IndexTickPriceBatchModelValidator()
        {
            RuleForEach(o => o.TickPrices).SetValidator(new TickPriceModelValidator());
        }
    }
}
