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
            CreateMap<AssetInfoModel, AssetInfo>(MemberList.Destination)
                .ConvertUsing(o => new AssetInfo(o.AssetId, o.Weight, o.Price, o.IsDisabled));

            CreateMap<IndexTickPriceModel, IndexTickPrice>(MemberList.Destination);

            CreateMap<TickPriceModel, TickPrice>(MemberList.Destination);

            CreateMap<IndexTickPriceBatchModel, IndexTickPriceBatch>(MemberList.Destination);
        }
    }
}
