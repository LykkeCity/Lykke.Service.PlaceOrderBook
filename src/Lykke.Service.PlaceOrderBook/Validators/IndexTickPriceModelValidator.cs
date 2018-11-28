using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;

namespace Lykke.Service.PlaceOrderBook.Validators
{
    [UsedImplicitly]
    public class IndexTickPriceModelValidator : AbstractValidator<IndexTickPriceModel>
    {
        public IndexTickPriceModelValidator()
        {
            RuleFor(o => o.Source)
                .NotEmpty()
                .WithMessage("Source required");

            RuleFor(o => o.AssetPair)
                .NotEmpty()
                .WithMessage("AssetPair required");

            RuleFor(o => o.Ask)
                .GreaterThan(0)
                .WithMessage("Ask should be greater than zero.");

            RuleFor(o => o.Bid)
                .GreaterThan(0)
                .WithMessage("Bid should be greater than zero.");
            
            RuleForEach(o => o.AssetsInfo).SetValidator(new AssetsInfoModelValidator());
        }
    }
}
