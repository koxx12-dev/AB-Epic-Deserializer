using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto;

#pragma warning disable CS8618
[ProtoContract]
public class AiCombo
{
    [ProtoMember(1)] public float Percentage { get; set; }

    [ProtoMember(2)] public List<string> ComboChain { get; set; }
}
