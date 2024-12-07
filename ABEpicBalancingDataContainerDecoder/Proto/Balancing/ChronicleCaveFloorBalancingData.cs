using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ChronicleCaveFloorBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int TemplateId { get; set; }

    [ProtoMember(3)] public string BackgroundId { get; set; }

    [ProtoMember(4)] public List<string> ChronicleCaveHotspotIds { get; set; }

    [ProtoMember(5)] public string FirstChronicleCaveHotspotId { get; set; }

    [ProtoMember(6)] public string LastChronicleCaveHotspotId { get; set; }

    [ProtoMember(7)] public string BossNameId { get; set; }

    [ProtoMember(8)] public Dictionary<Faction, string> EnvironmentalEffects { get; set; }
}
