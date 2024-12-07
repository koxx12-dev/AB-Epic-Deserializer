using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ExperienceMasteryBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int Experience { get; set; }

    [ProtoMember(3)] public int OldExperience { get; set; }

    [ProtoMember(4)] public int AncientExperience { get; set; }

    [ProtoMember(5)] public int StatBonus { get; set; }
}
