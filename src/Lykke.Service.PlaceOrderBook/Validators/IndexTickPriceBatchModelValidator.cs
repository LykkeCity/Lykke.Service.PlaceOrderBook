using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;

namespace Lykke.Service.PlaceOrderBook.Validators
{
    [UsedImplicitly]
    public class IndexTickPriceBatchModelValidator : AbstractValidator<IndexTickPriceBatchModel>
    {
        public IndexTickPriceBatchModelValidator()
        {
            RuleForEach(o => o.TickPrices).SetValidator(new TickPriceModelValidator());
            
            RuleForEach(o => o.IndexTickPrices).SetValidator(new IndexTickPriceModelValidator());
        }
    }
}
