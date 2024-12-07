using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ShopBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int Slots { get; set; }

    [ProtoMember(3)] public List<string> Categories { get; set; }

    [ProtoMember(4)] public string AssetId { get; set; }

    [ProtoMember(5)] public string LocaId { get; set; }

    [ProtoMember(6)] public string AtlasId { get; set; }
}
