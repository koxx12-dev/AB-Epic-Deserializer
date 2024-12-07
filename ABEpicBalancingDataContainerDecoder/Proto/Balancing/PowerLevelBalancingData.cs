using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class PowerLevelBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public float AttackModifier { get; set; }

    [ProtoMember(3)] public float HealthModifier { get; set; }

    [ProtoMember(4)] [Obsolete] public float PowerBaseWeight { get; set; }

    [ProtoMember(5)] public int ExpectedPlayerPowerlevel { get; set; }
}
