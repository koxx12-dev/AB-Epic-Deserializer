using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ConsumableItemBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public InventoryItemType ItemType { get; set; }

    [ProtoMember(3)] public string AssetBaseId { get; set; }

    [ProtoMember(4)] public string LocaBaseId { get; set; }

    [ProtoMember(5)] public int SortPriority { get; set; }

    [ProtoMember(6)] public string SkillNameId { get; set; }

    [ProtoMember(7)] public Dictionary<string, float> SkillParameters { get; set; }

    [ProtoMember(8)] public Dictionary<string, float> SkillParametersDeltaPerLevel { get; set; }

    [ProtoMember(9)] public string ConsumableStatckingType { get; set; }

    [ProtoMember(10)] public int ConversionPoints { get; set; }

    [ProtoMember(11)] public string InstantBuyOfferCategoryId { get; set; }
}
