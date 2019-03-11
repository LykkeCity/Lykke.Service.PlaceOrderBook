using FluentValidation;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;

namespace Lykke.Service.PlaceOrderBook.Validators
{
    public class AssetsInfoModelValidator : AbstractValidator<AssetInfoModel>
    {
        public AssetsInfoModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");
            
            RuleFor(o => o.Weight)
                .GreaterThan(0)
                .WithMessage("Weight should be greater than zero");
            
            RuleFor(o => o.Price)
                .GreaterThan(0)
                .WithMessage("Price should be greater than zero");
        }
    }
}
