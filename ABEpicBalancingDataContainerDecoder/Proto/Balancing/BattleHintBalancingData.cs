using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class BattleHintBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string LocaId { get; set; }

    [ProtoMember(3)] public string TopIconId { get; set; }

    [ProtoMember(4)] public string TopAtlasId { get; set; }

    [ProtoMember(5)] public string BottomIconId { get; set; }

    [ProtoMember(6)] public string BottomAtlasId { get; set; }

    [ProtoMember(7)] public Dictionary<string, int> RecommendedClasses { get; set; }
}
