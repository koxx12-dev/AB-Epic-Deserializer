using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class ThirdPartyIdBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string PaymentProductId { get; set; }

    [ProtoMember(3)] public string GamecenterAchievementId { get; set; }

    [ProtoMember(4)] public string ChimeraGooglePlayAchievementId { get; set; }

    [ProtoMember(5)] public string RovioGooglePlayAchievementId { get; set; }

    [ProtoMember(6)] public int XBoxLiveAchievementId { get; set; }
}
