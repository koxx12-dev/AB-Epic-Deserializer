namespace ABEpicBalancingDataContainerDecoder.Proto;

public enum CharacterSizeType
{
    Small,
    Medium,
    Large,
    Boss
}

public enum InventoryItemType
{
    None,
    Resources,
    Ingredients,
    MainHandEquipment,
    OffHandEquipment,
    Consumable,
    Premium,
    Story,
    PlayerToken,
    Points,
    Class,
    PlayerStats,
    CraftingRecipes,
    EventBattleItem,
    EventCollectible,
    Mastery,
    BannerTip,
    Banner,
    BannerEmblem,
    EventCampaignItem,
    EventBossItem,
    CollectionComponent,
    Trophy,
    Skin
}

public enum EquipmentSource
{
    Loot,
    Crafting,
    Gatcha,
    SetItem,
    LootBird,
    DailyGift
}

public enum Faction
{
    Birds,
    Pigs,
    None,
    NonAttackablePig
}
