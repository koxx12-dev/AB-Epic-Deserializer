using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class SkillBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string AssetId { get; set; }

    [ProtoMember(3)] public string LocaId { get; set; }

    [ProtoMember(4)] public Dictionary<string, float> SkillParameters { get; set; }

    [ProtoMember(5)] public string SkillTemplateType { get; set; }

    [ProtoMember(6)] public int EffectDuration { get; set; }

    [ProtoMember(7)] public SkillTargetTypes TargetType { get; set; }

    [ProtoMember(8)] public SkillEffectTypes EffectType { get; set; }

    [ProtoMember(9)] public PigTargetingBehavior TargetingBehavior { get; set; }

    [ProtoMember(10)] public bool TargetAlreadyAffectedTargets { get; set; }

    [ProtoMember(11)] public bool TargetSelfPossible { get; set; }

    [ProtoMember(12)] public string IconAssetId { get; set; }

    [ProtoMember(13)] public string IconAtlasId { get; set; }

    [ProtoMember(14)] public string EffectIconAtlasId { get; set; }

    [ProtoMember(15)] public string EffectIconAssetId { get; set; }

    [ProtoMember(16)] public List<string> TargetCulling { get; set; }
}

public enum PigTargetingBehavior
{
    None,
    Weakest,
    WeakestNoOwnBuff,
    Strongest,
    StrongestNoOwnDebuff,
    Cursed,
    Blessed,
    Taunting,
    TauntingNoOwnBuff,
    ChargeTarget,
    MostDebuffWeakest,
    RedBird,
    YellowBird,
    WhiteBird,
    BlackBird,
    BlueBirds,
    Self
}

public enum SkillEffectTypes
{
    None,
    Blessing,
    Curse,
    Passive,
    Environmental,
    SetPassive,
    SetHit
}

public enum SkillTargetTypes
{
    Attack = 1,
    Support,
    Passive,
    Environmental,
    SetPassive,
    SetHit
}
