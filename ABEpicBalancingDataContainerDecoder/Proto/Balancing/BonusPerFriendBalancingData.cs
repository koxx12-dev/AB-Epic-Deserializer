using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class BonusPerFriendBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int Count { get; set; }

    [ProtoMember(3)] public float AttackBonus { get; set; }

    [ProtoMember(4)] public float HealthBonus { get; set; }

    [ProtoMember(5)] public float XPBonus { get; set; }

    [ProtoMember(6)] public string UnlockedClassNameId { get; set; }
}
