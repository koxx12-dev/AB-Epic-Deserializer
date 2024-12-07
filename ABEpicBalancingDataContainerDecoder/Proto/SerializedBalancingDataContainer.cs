using ABEpicBalancingDataContainerDecoder.Helper;

using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto;

#pragma warning disable CS8618

[ProtoContract]
public class SerializedBalancingDataContainer
{
    [ProtoMember(1)] public Dictionary<string, byte[]> AllBalancingData { get; set; }

    [ProtoMember(2)] public string Version { get; set; }
}
