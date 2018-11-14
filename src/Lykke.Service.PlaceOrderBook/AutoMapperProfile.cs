using AutoMapper;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices;
using Lykke.Service.PlaceOrderBook.Core;

namespace Lykke.Service.PlaceOrderBook
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IndexTickPriceModel, IndexTickPrice>();

            CreateMap<TickPriceModel, TickPrice>();

            CreateMap<IndexTickPriceBatchModel, IndexTickPriceBatch>(MemberList.Source);
        }
    }
}
