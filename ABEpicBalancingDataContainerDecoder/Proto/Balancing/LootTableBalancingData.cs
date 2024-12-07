using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class LootTableBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public List<LootTableEntry> LootTableEntries { get; set; }

    [ProtoMember(3)] public LootTableType Type { get; set; }

    [ProtoMember(4)] public string PrefabId { get; set; }

    [ProtoMember(5)] public string LocaId { get; set; }
}

public enum LootTableType
{
    Probability,
    Weighted,
    Inventory,
    Wheel,
    WheelForced
}

[ProtoContract]
public class LootTableEntry
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int LevelMinIncl { get; set; }

    [ProtoMember(3)] public int LevelMaxExcl { get; set; }

    [ProtoMember(4)] public float Probability { get; set; }

    [ProtoMember(5)] public int BaseValue { get; set; }

    [ProtoMember(6)] public int Span { get; set; }

    [ProtoMember(7)] public int CurrentPlayerLevelDelta { get; set; }
}
