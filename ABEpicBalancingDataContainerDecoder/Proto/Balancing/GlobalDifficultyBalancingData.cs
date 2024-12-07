using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class GlobalDifficultyBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public float BirdAttackInPercent { get; set; }

    [ProtoMember(3)] public float BirdHealthInPercent { get; set; }

    [ProtoMember(4)] public float EquipmentInPercent { get; set; }

    [ProtoMember(5)] public float PigAttackInPercent { get; set; }

    [ProtoMember(6)] public float PigHealthInPercent { get; set; }

    [ProtoMember(7)] public float MaxStrengthPointAdjustment { get; set; }
}
