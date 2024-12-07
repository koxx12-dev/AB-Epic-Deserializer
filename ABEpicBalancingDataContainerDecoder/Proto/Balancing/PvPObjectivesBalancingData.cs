using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class PvPObjectivesBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public ObjectivesRequirement Requirement { get; set; }

    [ProtoMember(3)] public string LocaIdent { get; set; }

    [ProtoMember(4)] public string Requirementvalue { get; set; }

    [ProtoMember(5)] public string Requirementvalue2 { get; set; }

    [ProtoMember(6)] public int Amount { get; set; }

    [ProtoMember(7)] public string Difficulty { get; set; }

    [ProtoMember(8)] public string AssetIconID { get; set; }

    [ProtoMember(9)] public int DailyGroupId { get; set; }

    [ProtoMember(10)] public int Reward { get; set; }

    [ProtoMember(11)] public int Playerlevel { get; set; }
}

public enum ObjectivesRequirement
{
    winTotal,
    winRow,
    dontHeal,
    useBird,
    useClass,
    notUseBird,
    notKill,
    notUseRage,
    getAmountStars,
    killWithRage,
    withBirdsAlive,
    killAtOnce,
    killBird,
    protectBird,
    winWhileBirdsDead,
    killWithBanner,
    killBirdsInBattle,
    killBannerInEnemyTurn,
    killWithBird,
    useRage,
    killBirdsInRound,
    multiUseClasses,
    noSupportSkills,
    winAfterCoinLose
}
