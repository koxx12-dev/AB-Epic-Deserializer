using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoInclude(90, typeof(ChronicleCaveBattleParticipantTableBalancingData))]
[ProtoContract]
[BalancingData]
public class BattleParticipantTableBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public BattleParticipantTableType Type { get; set; }

    [ProtoMember(3)] public VictoryCondition VictoryCondition { get; set; }

    [ProtoMember(4)] public List<BattleParticipantTableEntry> BattleParticipants { get; set; }
}

public enum BattleParticipantTableType
{
    IgnoreStrength,
    Weighted,
    Probability
}

[ProtoContract]
public class VictoryCondition
{
    [ProtoMember(1)] public VictoryConditionTypes Type { get; set; }

    [ProtoMember(2)] public string NameId { get; set; }

    [ProtoMember(3)] public float Value { get; set; }
}

public enum VictoryConditionTypes
{
    DefeatAll,
    DefeatExplicit,
    SurviveTurns,
    CharacterSurviveTurns,
    FinishInTurns
}

[ProtoContract]
public class BattleParticipantTableEntry
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int LevelDifference { get; set; }

    [ProtoMember(3)] public float Probability { get; set; }

    [ProtoMember(4)] public float Amount { get; set; }

    [ProtoMember(5)] public bool Unique { get; set; }

    [ProtoMember(6)] public bool ForcePercent { get; set; }
}
