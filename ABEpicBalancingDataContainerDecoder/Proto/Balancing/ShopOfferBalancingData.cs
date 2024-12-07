using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ShopOfferBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public Dictionary<string, int> OfferContents { get; set; }

    [ProtoMember(3)] public List<Requirement> BuyRequirements { get; set; }

    [ProtoMember(4)] public List<Requirement> ShowRequirements { get; set; }

    [ProtoMember(5)] public int SortPriority { get; set; }

    [ProtoMember(6)] public int Level { get; set; }

    [ProtoMember(7)] public string Category { get; set; }

    [ProtoMember(8)] public string AssetId { get; set; }

    [ProtoMember(9)] public int SlotId { get; set; }

    [ProtoMember(10)] public string LocaId { get; set; }

    [ProtoMember(11)] public RelativeLevelType LevelType { get; set; }

    [ProtoMember(12)] public List<Requirement> BuyRequirementsScaling { get; set; }

    [ProtoMember(13)] public List<Requirement> ShowRequirementsScaling { get; set; }

    [ProtoMember(14)] public bool ManagedExternal { get; set; }

    [ProtoMember(15)] public int LevelAdditional { get; set; }

    [ProtoMember(16)] public int DiscountPrice { get; set; }

    [ProtoMember(17)] public List<Requirement> DiscountRequirements { get; set; }

    [ProtoMember(18)] public int DiscountCooldown { get; set; }

    [ProtoMember(19)] public int DiscountDuration { get; set; }

    [ProtoMember(20)] public bool UniqueOffer { get; set; }
}

public enum RelativeLevelType
{
    ExactLevel,
    RelativeToPlayer,
    PlayerLevelStep
}
