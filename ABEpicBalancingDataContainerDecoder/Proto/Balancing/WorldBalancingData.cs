using ABEpicBalancingDataContainerDecoder.Helper;
using ProtoBuf;

namespace ABEpicBalancingDataContainerDecoder.Proto.Balancing;

#pragma warning disable CS8618
[ProtoContract]
[BalancingData]
public class WorldBalancingData
{
    [ProtoMember(1)] public string NameId { get; set; }

    [ProtoMember(2)] public string FirstHotspotNameId { get; set; }

    [ProtoMember(3)] public int MaxPigsInBattle { get; set; }

    [ProtoMember(4)] public float RageMeterIncreasePerHitInPercent { get; set; }

    [ProtoMember(5)] public float RageMeterIncreasePerHiAfterFirstAOEInPercent { get; set; }

    [ProtoMember(6)] public float RageMeterIncreasePerPassiveEffectInPercent { get; set; }

    [ProtoMember(7)] public float RageMeterFullOnTotalHealthLostFactor { get; set; }

    [ProtoMember(8)] public float CostToScrapLootRateCrafting { get; set; }

    [ProtoMember(9)] public float CostToScrapLootRateGacha { get; set; }

    [ProtoMember(10)] public float TimeForResourceRespawn { get; set; }

    [ProtoMember(11)] public float ReferenceAttackValueBase { get; set; }

    [ProtoMember(12)] public float ReferenceAttackValuePerLevelInPercent { get; set; }

    [ProtoMember(13)] public float ReferenceHealthValueBase { get; set; }

    [ProtoMember(14)] public float ReferenceHealthValuePerLevelInPercent { get; set; }

    [ProtoMember(15)] public float ShopFullRefreshCycleTimeInSec { get; set; }

    [ProtoMember(16)] public Requirement RerollCraftingReqirement { get; set; }

    [ProtoMember(17)] public int MaxBirdsInBattle { get; set; }

    [ProtoMember(18)] public int LevelSubstractionForMissingBirdOnDifficultyCalculation { get; set; }

    [ProtoMember(19)] public int MaximumLevelDifferenceForDifficultyCalculation { get; set; }

    [ProtoMember(20)] public int BirdLevelWeightForDifficultyCalculation { get; set; }

    [ProtoMember(21)] public uint TimeGoldenPigSpawn { get; set; }

    [ProtoMember(22)] public uint TimeGoldenPigOnlyClientIfFailedRespawn { get; set; }

    [ProtoMember(23)] public uint TimeForGetFriendBird { get; set; }

    [ProtoMember(24)] public Requirement MightyEagleBirdReqirement { get; set; }

    [ProtoMember(25)] public Requirement ReviveBirdsRequirement { get; set; }

    [ProtoMember(26)] public uint TimeForNextClassUpgrade { get; set; }

    [ProtoMember(27)] public Requirement NextClassSkipRequirement { get; set; }

    [ProtoMember(28)] public List<float> StandardDiceWeights { get; set; }

    [ProtoMember(29)] public List<float> GoldDiceWeights { get; set; }

    [ProtoMember(30)] public List<float> CrystalDiceWeights { get; set; }

    [ProtoMember(31)] public uint TipOfTheDayCount { get; set; }

    [ProtoMember(32)] public int ResourceSpawnAmountPerNode { get; set; }

    [ProtoMember(33)] public int MaximumSpawnableNodes { get; set; }

    [ProtoMember(34)] public Requirement DungeonSkipRequirement { get; set; }

    [ProtoMember(35)] public int GachaUsesFromHighOffer { get; set; }

    [ProtoMember(36)] public string DefaultGlobalDifficultyBalancing { get; set; }

    [ProtoMember(37)] public Dictionary<int, int> AdCooldownBalancing { get; set; }

    [ProtoMember(38)] public Dictionary<string, uint> SponsoredAdCooldownBalancing { get; set; }

    [ProtoMember(39)] public string SponsoredAdPotionType { get; set; }

    [ProtoMember(40)] public string SponsoredAdBuffName { get; set; }

    [ProtoMember(41)] public string DailyHotspotNameId { get; set; }

    [ProtoMember(42)] public int DailyChainLength { get; set; }

    [ProtoMember(43)] public bool IsLimeGreen { get; set; }

    [ProtoMember(44)] public int LimeGreenValue { get; set; }

    [ProtoMember(45)] public int RestedBonusTime { get; set; }

    [ProtoMember(46)] public bool RestedBonusActive { get; set; }

    [ProtoMember(47)] public uint RainbowRiotTime { get; set; }

    [ProtoMember(48)] public int GachaPreviewAmount { get; set; }

    [ProtoMember(49)] public float GachaPreviewPercentStandard { get; set; }

    [ProtoMember(50)] public float GachaPreviewPercentRiot { get; set; }

    [ProtoMember(51)] public float GachaPreviewPercentSet { get; set; }

    [ProtoMember(52)] public List<LevelRangeValueTable> LevelRubberBandTables { get; set; }

    [ProtoMember(53)] public List<int> DailyChainAdditionalBonusPerDay { get; set; }

    [ProtoMember(54)] public string DailyChainHeaderLocaId { get; set; }

    [ProtoMember(55)] public uint DailyChainTimerUntilTimestamp { get; set; }

    [ProtoMember(56)] public int GlobalEventDiscount { get; set; }

    [ProtoMember(57)] public List<int> MasteryFromExperienceMultiplier { get; set; }

    [ProtoMember(58)] public List<int> ClassUpgradeToMasteryMapping { get; set; }

    [ProtoMember(59)] public float AllBirdsMasteryChance { get; set; }

    [ProtoMember(60)] public float SingleBirdMasteryChance { get; set; }

    [ProtoMember(61)] public Dictionary<string, int> ItemMaxCaps { get; set; }

    [ProtoMember(62)] public float EnergyRefreshTimeInSeconds { get; set; }

    [ProtoMember(63)] public float MasteryChancePlus { get; set; }

    [ProtoMember(64)] public float MasteryChanceBonusCap { get; set; }

    [ProtoMember(65)] public List<float> DojoOfferDiscount { get; set; }

    [ProtoMember(66)] public List<int> DojoOfferDiscountThresholds { get; set; }

    [ProtoMember(67)] public Dictionary<string, int> ChronicleCaveDailyTreasureLoot { get; set; }

    [ProtoMember(68)] public uint TimeChronicleCaveTreasureSpawn { get; set; }

    [ProtoMember(69)] public Dictionary<string, int> DailyEventAdLoot { get; set; }

    [ProtoMember(70)] public uint TimeDailyEventPopupSpawn { get; set; }

    [ProtoMember(71)] public int PvpGachaUsesFromHighOffer { get; set; }

    [ProtoMember(72)] public float CostToScrapLootRateSet { get; set; }

    [ProtoMember(73)] public Requirement ReviveSingleBirdsRequirement { get; set; }

    [ProtoMember(74)] public int MultiCraftAmount { get; set; }

    [ProtoMember(75)] public Requirement RerollMultiCraftingReqirement { get; set; }

    [ProtoMember(76)] public float CoinFlipChanceChange { get; set; }

    [ProtoMember(77)] public float CoinFlipChanceMaxChange { get; set; }

    [ProtoMember(78)] public bool UseGoldenPigCloudBattle { get; set; }

    [ProtoMember(79)] public int TimeGoldenPigRespawnRandomOffset { get; set; }

    [ProtoMember(80)] public int TimeGoldenPigMoveOn { get; set; }

    [ProtoMember(81)] public int MultiGachaAmount { get; set; }

    [ProtoMember(82)] public int GachaUsesFromNormalOffer { get; set; }

    [ProtoMember(83)] public int PvpGachaUsesFromNormalOffer { get; set; }

    [ProtoMember(84)] public int RainbowRiot1Multi { get; set; }

    [ProtoMember(85)] public int RainbowRiot2Multi { get; set; }

    [ProtoMember(86)] public int GachaVideoTimespan { get; set; }

    [ProtoMember(87)] public float GoldenAnvilBonus { get; set; }

    [ProtoMember(88)] public float DiamondAnvilBonus { get; set; }

    [ProtoMember(89)] public bool EnableCrossPromoButton { get; set; }

    [ProtoMember(90)] public float ChanceToDisplayStaminaVideo { get; set; }

    [ProtoMember(91)] public int RateAppAbortCooldown { get; set; }

    [ProtoMember(92)] public int HPChunksLowest { get; set; }

    [ProtoMember(93)] public int HPChunksHighest { get; set; }

    [ProtoMember(94)] public int MaxHPChunks { get; set; }

    [ProtoMember(95)] public float HPChunkSteps { get; set; }

    [ProtoMember(96)] public List<int> NotificationPopupCooldowns { get; set; }

    [ProtoMember(97)] public bool EnableFriendLeaderboards { get; set; }

    [ProtoMember(98)] public float BonusPercentByBossRewardVideo { get; set; }

    [ProtoMember(99)] public int PvpMaxPowerlevelDiff { get; set; }

    [ProtoMember(100)] public bool OneWorldBoss { get; set; }

    [ProtoMember(101)] public Requirement RerollPvpObjectivesRequirement { get; set; }

    [ProtoMember(102)] public int ObjectivesVideoTimespan { get; set; }

    [ProtoMember(103)] public Requirement RerollChestRequirement { get; set; }

    [ProtoMember(104)] public int MaxPreviewPigsInBps { get; set; }
}

[ProtoContract]
public class LevelRangeValueTable
{
    [ProtoMember(1)] public int FromLevel { get; set; }

    [ProtoMember(2)] public int ToLevel { get; set; }

    [ProtoMember(3)] public int Value { get; set; }
}
