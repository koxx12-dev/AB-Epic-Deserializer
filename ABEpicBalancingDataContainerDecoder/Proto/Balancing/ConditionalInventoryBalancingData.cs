using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ConditionalInventoryBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public List<Requirement> DropRequirements { get; set; }

    [ProtoMember(3)] public Dictionary<string, int> Content { get; set; }

    [ProtoMember(4)] public int InitializingLevel { get; set; }

    [ProtoMember(5)] public ConditionalLootTableDropTrigger Trigger { get; set; }
}

public enum ConditionalLootTableDropTrigger
{
    NotFirstStartUp,
    FirstStartUp,
    RemoveNotFirstStartUp
}
