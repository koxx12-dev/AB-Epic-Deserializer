using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class BuyableShopOfferBalancingData : BasicShopOfferBalancingData
{
    [ProtoMember(52)] public int DiscountPrice { get; set; }

    [ProtoMember(53)] public bool DisplayAfterPurchase { get; set; }

    [Obsolete] [ProtoMember(54)] public bool ExclusiveOffer { get; set; }
}
