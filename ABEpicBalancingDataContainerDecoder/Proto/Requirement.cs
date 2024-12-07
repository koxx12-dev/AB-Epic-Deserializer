using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto;

#pragma warning disable CS8618
[ProtoContract]
public class Requirement
{
    [ProtoMember(1)] public RequirementType RequirementType { get; set; }

    [ProtoMember(2)] public string NameId { get; set; }

    [ProtoMember(3)] public float Value { get; set; }
}

public enum RequirementType
{
    None,
    PayItem,
    HaveItem,
    NotHaveItem,
    HaveBird,
    Level,
    CooldownFinished,
    IsSpecificWeekday,
    IsNotSpecificWeekday,
    HaveCurrentHotpsotState,
    HavePassedCycleTime,
    NotHavePassedCycleTime,
    NotHaveItemWithLevel,
    HaveItemWithLevel,
    UsedFriends,
    HaveUnlockedHotpsot,
    NotHaveUnlockedHotpsot,
    HaveLessThan,
    HaveBirdCount,
    HaveAllUpgrades,
    NotHaveAllUpgrades,
    NotUseBirdInBattle,
    UseBirdInBattle,
    HaveMasteryFactor,
    NotHaveMasteryFactor,
    NotHaveClass,
    IsConverted,
    HaveEventCampaignHotspotState,
    HaveTotalItemsInCollection,
    LostPvpBattle,
    HaveEventScore,
    HaveClass,
    HaveCurrentChronicleCaveState,
    TutorialCompleted,
    TotalMoneySpent,
    LostUnresolvedHotspot,
    UnlockedAllClasses,
    BirdMasteryFactorMinimum,
    BirdMasteryFactorMaximum,
    HighestLeagueReached,
    TimeSinceLastPurchase,
    DeclinedOffer,
    AcceptedOffer,
    EndedOffer,
    UnlockedAllSkins
}
