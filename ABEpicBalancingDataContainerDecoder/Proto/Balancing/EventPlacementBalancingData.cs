using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData("ABH.Shared.Events.BalancingData")]
public class EventPlacementBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public List<Requirement> SpawnAbleRequirements { get; set; }

    [ProtoMember(3)] public string Category { get; set; }

    [ProtoMember(4)] public string OverrideBattleGroundName { get; set; }
}
