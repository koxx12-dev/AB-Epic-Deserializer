﻿using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class EquipmentBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public InventoryItemType ItemType { get; set; }

    [ProtoMember(3)] public string AssetBaseId { get; set; }

    [ProtoMember(4)] public string LocaBaseId { get; set; }

    [ProtoMember(5)] public int SortPriority { get; set; }

    [ProtoMember(6)] public int BaseStat { get; set; }

    [ProtoMember(7)] public int StatPerLevel { get; set; }

    [ProtoMember(8)] public int StatPerQuality { get; set; }

    [ProtoMember(9)] public List<int> StatPerQualityPercent { get; set; }

    [ProtoMember(10)] public int AnimationIndex { get; set; }

    [ProtoMember(11)] public bool IsRanged { get; set; }

    [ProtoMember(12)] public string ProjectileAssetId { get; set; }

    [ProtoMember(13)] public EquipmentPerk Perk { get; set; }

    [ProtoMember(14)] public string RestrictedBirdId { get; set; }

    [ProtoMember(15)] public bool DirectAssetAndLoca { get; set; }

    [ProtoMember(16)] public string CorrespondingSetItemId { get; set; }

    [ProtoMember(17)] public int AssetLevelOffset { get; set; }

    [ProtoMember(18)] public int AssetCycleCount { get; set; }

    [ProtoMember(19)] public string HitEffectSuffix { get; set; }

    [ProtoMember(20)] public string SetItemSkill { get; set; }

    [ProtoMember(21)] public bool DirectProjectileAssetAndLoca { get; set; }

    [ProtoMember(22)] public EquipmentSource Mainsource { get; set; }

    [ProtoMember(23)] public bool ShowAsNew { get; set; }

    [ProtoMember(24)] public bool HideInPreview { get; set; }

    [ProtoMember(25)] public string PvpSetItemSkill { get; set; }
}

[ProtoContract]
public class EquipmentPerk
{
    [ProtoMember(1)] public PerkType Type { get; set; }

    [ProtoMember(2)] public float ProbablityInPercent { get; set; }

    [ProtoMember(3)] public float PerkValue { get; set; }
}

public enum PerkType
{
    None,
    CriticalStrike,
    Bedtime,
    ChainAttack,
    HocusPokus,
    Dispel,
    Vigor,
    Might,
    Vitality,
    IncreaseRage,
    IncreaseHealing,
    ReduceRespawn,
    ShareBirdDamage,
    Enrage,
    MythicProtection,
    Finisher,
    Stronghold,
    Justice
}
