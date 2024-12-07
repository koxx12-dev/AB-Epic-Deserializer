using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class LoadingHintBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public List<Requirement> ShowRequirements { get; set; }

    [ProtoMember(3)] public float Weight { get; set; }

    [ProtoMember(4)] public List<LoadingArea> TargetAreas { get; set; }
}

public enum LoadingArea
{
    Worldmap,
    Camp,
    Arena,
    Battle,
    ChronicleCave
}
