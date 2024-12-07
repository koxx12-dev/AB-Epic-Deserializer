using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData("ABH.Shared.Events.BalancingData")]
public class EventItemBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public InventoryItemType ItemType { get; set; }

    [ProtoMember(3)] public string AssetBaseId { get; set; }

    [ProtoMember(4)] public string LocaBaseId { get; set; }

    [ProtoMember(5)] public int SortPriority { get; set; }

    [ProtoMember(6)] public List<string> EventParameters { get; set; }

    [ProtoMember(7)] public List<string> SpawnCategories { get; set; }
}
