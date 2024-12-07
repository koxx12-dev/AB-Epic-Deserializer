using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class SalesManagerBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public SaleContentType ContentType { get; set; }

    [ProtoMember(3)] public SaleAvailabilityType SaleType { get; set; }

    [ProtoMember(4)] public List<SaleItemDetails> SaleDetails { get; set; }

    [Obsolete] [ProtoMember(5)] public SaleItemGrouping Grouping { get; set; }

    [ProtoMember(6)] public uint StartTime { get; set; }

    [ProtoMember(7)] public uint EndTime { get; set; }

    [ProtoMember(8)] public List<Requirement> Requirements { get; set; }

    [ProtoMember(9)] public int Duration { get; set; }

    [ProtoMember(10)] public int SortPriority { get; set; }

    [ProtoMember(11)] public string PopupIconId { get; set; }

    [ProtoMember(12)] public string PopupAtlasId { get; set; }

    [ProtoMember(13)] public string LocaBaseId { get; set; }

    [ProtoMember(14)] [Obsolete] public List<float> OfferLabelColor { get; set; }

    [ProtoMember(15)] [Obsolete] public List<float> OfferBackgroundColor { get; set; }

    [ProtoMember(16)] public string CheckoutCategory { get; set; }

    [ProtoMember(17)] public int Cooldown { get; set; }

    [ProtoMember(18)] public bool ShowContentsInPopup { get; set; }

    [ProtoMember(19)] public int PriorityInQueue { get; set; }

    [ProtoMember(20)] public bool RecheckRequirements { get; set; }

    [ProtoMember(21)] public string PrefabId { get; set; }

    [ProtoMember(22)] public bool Unique { get; set; }

    [ProtoMember(23)] public bool Infinite { get; set; }
}

public enum SaleItemGrouping
{
    Simultaneous,
    Sequence
}

public enum SaleContentType
{
    ShopItems,
    Mastery,
    RainbowRiot,
    GenericBundle,
    ClassBundle,
    SetBundle,
    LuckyCoinDiscount,
    Chain
}

public enum SaleAvailabilityType
{
    Timed,
    PersonalTimeWindow,
    TimedSequence,
    Conditional,
    ConditionalCooldown
}

[ProtoContract]
public class SaleItemDetails
{
    [ProtoMember(1)] public string SubjectId { get; set; }

    [ProtoMember(2)] public SaleParameter SaleParameter { get; set; }

    [ProtoMember(3)] public int ChangedValue { get; set; }

    [ProtoMember(4)] public string ReplacementProductId { get; set; }
}

public enum SaleParameter
{
    Price,
    Value,
    Special
}
