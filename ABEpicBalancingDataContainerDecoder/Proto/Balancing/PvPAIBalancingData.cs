using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class PvPAIBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public int BasicBannerWeight { get; set; }

    [ProtoMember(3)] public int BasicBirdWeight { get; set; }

    [ProtoMember(4)] public int AddBannerWeightBelow80 { get; set; }

    [ProtoMember(5)] public int AddBannerWeightBelow60 { get; set; }

    [ProtoMember(6)] public int AddBannerWeightBelow40 { get; set; }

    [ProtoMember(7)] public int AddBannerWeightBelow20 { get; set; }

    [ProtoMember(8)] public int AddBirdWeightBelow50 { get; set; }

    [ProtoMember(9)] public int AddBirdWeightBelow30 { get; set; }

    [ProtoMember(10)] public int RageRedPrio { get; set; }

    [ProtoMember(11)] public int RageYellowPrio { get; set; }

    [ProtoMember(12)] public int RageBlackPrio { get; set; }

    [ProtoMember(13)] public int RageBluePrio { get; set; }

    [ProtoMember(14)] public float ChanceToUseRandomTarget { get; set; }

    [ProtoMember(15)] public int RageWhitePrio { get; set; }

    [ProtoMember(16)] public float ChanceToFocusBirdWith3 { get; set; }

    [ProtoMember(17)] public float ChanceToFocusBirdWith2 { get; set; }

    [ProtoMember(18)] public float ChanceToFocusBirdWith1 { get; set; }
}
