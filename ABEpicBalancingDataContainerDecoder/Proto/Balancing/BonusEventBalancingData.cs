using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData("ABH.Shared.Events.BalancingData")]
public class BonusEventBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public BonusEventType BonusType { get; set; }

    [ProtoMember(3)] public float BonusFactor { get; set; }

    [ProtoMember(4)] public uint StartDate { get; set; }

    [ProtoMember(5)] public uint EndDate { get; set; }

    [ProtoMember(6)] public string IconId { get; set; }

    [ProtoMember(7)] public string AtlasId { get; set; }

    [ProtoMember(8)] public string LocaId { get; set; }

    [ProtoMember(9)] public bool TeasedBeforeRunning { get; set; }
}

public enum BonusEventType
{
    DungeonBonus = 1,
    ArenaPointBonus,
    ShardsForObjective,
    CcLootBonus,
    RainbowbarBonus,
    MasteryBonus
}
