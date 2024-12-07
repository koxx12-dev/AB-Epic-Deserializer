using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class GameConstantsBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string StringValue { get; set; }

    [ProtoMember(3)] public float FloatValue { get; set; }

    [ProtoMember(4)] public Requirement RequirementValue { get; set; }

    [ProtoMember(5)] public List<float> FloatlistValue { get; set; }

    [ProtoMember(6)] public bool BoolValue { get; set; }
}
