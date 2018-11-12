using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;

namespace Lykke.Service.PlaceOrderBook.Validators
{
    [UsedImplicitly]
    public class TickPriceModelValidator : AbstractValidator<TickPriceModel>
    {
        public TickPriceModelValidator()
        {
            RuleFor(o => o.Source)
                .NotEmpty()
                .WithMessage("Source required");

            RuleFor(o => o.Asset)
                .NotEmpty()
                .WithMessage("Asset required");

            RuleFor(o => o.Ask)
                .GreaterThan(0)
                .WithMessage("Ask should be greater than zero.");

            RuleFor(o => o.Bid)
                .GreaterThan(0)
                .WithMessage("Bid should be greater than zero.");
        }
    }
}
