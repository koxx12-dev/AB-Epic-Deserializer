using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class CraftingItemBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public InventoryItemType ItemType { get; set; }

    [ProtoMember(3)] public string AssetBaseId { get; set; }

    [ProtoMember(4)] public string LocaBaseId { get; set; }

    [ProtoMember(5)] public int SortPriority { get; set; }

    [ProtoMember(6)] public string Recipe { get; set; }

    [ProtoMember(7)] public string BaseItemNameId { get; set; }

    [ProtoMember(8)] public int ValueOfBaseItem { get; set; }

    [ProtoMember(9)] public string AtlasNameId { get; set; }
}
