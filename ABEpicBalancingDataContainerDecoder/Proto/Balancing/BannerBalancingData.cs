using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class BannerBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string AssetId { get; set; }

    [ProtoMember(3)] public string LocaId { get; set; }

    [ProtoMember(4)] public string DefaultInventoryNameId { get; set; }

    [ProtoMember(5)] public int BaseHealth { get; set; }

    [ProtoMember(6)] public int PerLevelHealth { get; set; }

    [ProtoMember(7)] public CharacterSizeType SizeType { get; set; }

    [ProtoMember(8)] public int SortPriority { get; set; }
}
