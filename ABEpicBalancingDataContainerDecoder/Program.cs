using System;
using System.IO;
using System.Linq;
using ProtoBuf;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

namespace ABEpicBalancingDataContainerDecoder
{
	class Program
	{
		public static void Main(string[] args)
		{
			string className = null;
			bool skipToAssignClassType = false;
			bool skipToClassNameCheck = false;
			bool classNameExists = false;
			string currClassName = null;
			Type ClassType = null;
			string CurrClassName = null;
			Type TypeDef = null;
			Dictionary<string, byte[]> BalancingDataContainer = null;
			SerializedBalancingDataContainer ReimportedBalancingDataContainer = null;
			string path = null;
			string option = null;
			bool compress = false;
			if (args.Length < 1)
			{
				Console.WriteLine("Do you decode or reimport (Required)?");
				option = Console.ReadLine();
			}
			else
			{
				option = args[0];
			}
			while (!CheckForAvailableArguments(option, " Do you decode or reimport (Required)?", Console.WriteLine))
			{
				option = Console.ReadLine();
			}
			if (args.Length < 2)
			{
				Console.WriteLine("Enter class name (Required):");
				className = Console.ReadLine();
			}
			else
			{
				className = args[1];
			}
		classNameError:
			while (string.IsNullOrEmpty(className))
			{
				Console.WriteLine("Class name is empty! Please enter class name:");
				className = Console.ReadLine();
			}
			if (skipToAssignClassType)
			{
				goto assignClassType;
			}
			if (skipToClassNameCheck)
			{
				goto checkClassName;
			}
			if (args.Length < 3)
			{
				Console.WriteLine("Enter input path in order to read (Required):");
				path = Console.ReadLine();
			}
			else
			{
				path = args[2];
			}
		pathError:
			while (string.IsNullOrEmpty(path))
			{
				Console.WriteLine("Input path is empty! Please enter input path in order to read:");
				path = Console.ReadLine();
			}
			while (!File.Exists(path))
			{
				Console.WriteLine("Input path does not exists! Please enter input path in order to read:");
				path = Console.ReadLine();
				if (string.IsNullOrEmpty(path))
				{
					goto pathError;
				}
			}
			
			SerializedBalancingDataContainer tmp = ProtoDeserialize<SerializedBalancingDataContainer>(GetCompressionService().DecompressIfNecessary(File.ReadAllBytes(path)));
			ReimportedBalancingDataContainer = tmp;
			BalancingDataContainer = tmp.AllBalancingData;
		assignClassType:
			ClassType = typeof(Program);
			CurrClassName = ClassType.FullName + "+" + className;
			TypeDef = Type.GetType("System.Collections.Generic.List`1[" + CurrClassName + "]");
			if (TypeDef == null)
			{
				Console.WriteLine("This class name does not exist! Please enter another class name:");
				className = Console.ReadLine();
				if (string.IsNullOrEmpty(className))
				{
					skipToAssignClassType = true;
					goto classNameError;
				}
				else
                {
					goto assignClassType;
                }
			}
			skipToClassNameCheck = false;
		checkClassName:
			for (int i = 0; i < BalancingDataContainer.Count; i++)
			{
				classNameExists = BalancingDataContainer.ElementAt(i).Key.Split(".").Last() == className;
				if (classNameExists)
                {
					currClassName = BalancingDataContainer.ElementAt(i).Key;
					break;
                }
			}
			if (!classNameExists)
            {
				Console.WriteLine("Class name in balancing data does not exist! Please enter another class name:");
				className = Console.ReadLine();
				if (string.IsNullOrEmpty(className))
				{
					skipToClassNameCheck = true;
					goto classNameError;
				}
				else
                {
					goto checkClassName;
                }
            }
			skipToClassNameCheck = false;
			if (option == "reimport")
			{
				string decodedPath = null;
				if (args.Length < 4)
				{
					Console.WriteLine("Enter decoded path in order to read (Required):");
					decodedPath = Console.ReadLine();
				}
				else
				{
					decodedPath = args[3];
				}
			decodedPathError:
				while (string.IsNullOrEmpty(decodedPath))
				{
					Console.WriteLine("Decoded path is empty! Please enter decoded path in order to read:");
					decodedPath = Console.ReadLine();
				}
				while (!File.Exists(decodedPath))
				{
					Console.WriteLine("Decoded path not exists! Please enter decoded path in order to read:");
					decodedPath = Console.ReadLine();
					if (string.IsNullOrEmpty(decodedPath))
					{
						goto decodedPathError;
					}
				}
				string outputPath = path + "_OUT";
				string tmpStr = null;
				if (args.Length < 5)
				{
					Console.WriteLine("Enter output path in order to write (Optional):");
					tmpStr = Console.ReadLine();
					if (!string.IsNullOrEmpty(tmpStr)) {
						outputPath = tmpStr;
					}
				}
				else
				{
					outputPath = args[4];
				}
				if (args.Length < 6)
				{
					Console.WriteLine("Do you compress the file (Game can read compressed file, optional)?");
					tmpStr = Console.ReadLine();
					compress = tmpStr == "yes" || tmpStr == "yeah" || tmpStr == "true";
				}
				else
				{
					compress = args[5] == "yes" || args[5] == "yeah" || args[5] == "true";
				}
				object DeserializedPlainJson = ClassType.GetMethod(nameof(JsonDeserialize)).MakeGenericMethod(TypeDef).Invoke(ClassType, new object[] { File.ReadAllText(decodedPath) });
				ReimportedBalancingDataContainer.AllBalancingData[currClassName] = (byte[])ClassType.GetMethod(nameof(ProtoSerialize)).MakeGenericMethod(TypeDef).Invoke(ClassType, new object[] { DeserializedPlainJson });
				byte[] ReimportedBalancingDataBytes = ProtoSerialize<SerializedBalancingDataContainer>(ReimportedBalancingDataContainer);
				if (compress)
				{
					try
					{
						ReimportedBalancingDataBytes = GetCompressionService().Compress(ReimportedBalancingDataBytes);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error compressing balancing data container: " + ex.Message);
						return;
					}
				}
				File.WriteAllBytes(outputPath, ReimportedBalancingDataBytes);
				Console.WriteLine("Balancing data has been successfully reimported!");
			}
			else if (option == "decode")
			{
				string outputPathPrefix = null;
				if (args.Length < 4)
				{
					Console.WriteLine("Enter output path prefix (Optional):");
					outputPathPrefix = Console.ReadLine();
				}
				else
				{
					outputPathPrefix = args[3];
				}
				var outputDecodedFile = currClassName + ".json";
				if (!string.IsNullOrEmpty(outputPathPrefix))
				{
					outputDecodedFile = Path.Combine(outputPathPrefix, outputDecodedFile);
				}
				File.WriteAllText(outputDecodedFile, JsonConvert.SerializeObject(ClassType.GetMethod(nameof(ProtoDeserialize)).MakeGenericMethod(TypeDef).Invoke(ClassType, new object[] { BalancingDataContainer[currClassName] })));
				Console.WriteLine("Balancing data has been successfully decoded!");
				return;
			}
			//Console.WriteLine(BalancingDataContainer.ElementAt(0).Key.Split(".").Last());
		}
		static bool CheckForAvailableArguments(string arg, string msg, Action<string> onErrorCallback)
        {
			string errInvalidArg = "Invalid argument! Did you mean: ";
			foreach (string availableArgs in AvailableArguments)
            {
				if (arg == availableArgs)
                {
					return true;
                }
			}
			if (arg.Contains("decode") || arg == "decod" || arg == "decrypt" || arg == "uncrypt")
            {
				onErrorCallback(errInvalidArg + "decode." + msg);
				return false;
			}
			else if (arg.Contains("import") || arg == "encrypt" || arg == "encode" || arg == "encod")
			{
				onErrorCallback(errInvalidArg + "reimport." + msg);
				return false;
			}
			onErrorCallback("Invalid argument!" + msg);
			return false;
        }
		static readonly string[] AvailableArguments = new string[]
		{
			"decode",
			"reimport",
		};
		public static T JsonDeserialize<T>(string data) where T : class
		{
			if (null == data) return null;

			return JsonConvert.DeserializeObject<T>(data);
		}
		public static byte[] ProtoSerialize<T>(T record) where T : class
		{
			if (null == record) return null;

			try
			{
				using (var stream = new MemoryStream())
				{
					Serializer.Serialize(stream, record);
					return stream.ToArray();
				}
			}
			catch
			{
				// Log error
				throw;
			}
		}

		public static T ProtoDeserialize<T>(byte[] data) where T : class
		{
			if (null == data) return null;

			try
			{
				using (var stream = new MemoryStream(data))
				{
					return Serializer.Deserialize<T>(stream);
				}
			}
			catch
			{
				// Log error
				throw;
			}
		}

		public enum BonusEventType
		{
			DungeonBonus = 1,
			ArenaPointBonus,
			ShardsForObjective,
			CcLootBonus,
			RainbowbarBonus,
			MasteryBonus
		}
		[ProtoContract]
		public class BonusEventBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public BonusEventType BonusType { get; set; }

			[ProtoMember(3)]
			public float BonusFactor { get; set; }

			[ProtoMember(4)]
			public uint StartDate { get; set; }

			[ProtoMember(5)]
			public uint EndDate { get; set; }

			[ProtoMember(6)]
			public string IconId { get; set; }

			[ProtoMember(7)]
			public string AtlasId { get; set; }

			[ProtoMember(8)]
			public string LocaId { get; set; }

			[ProtoMember(9)]
			public bool TeasedBeforeRunning { get; set; }
		}

		public enum RequirementType
		{
			None,
			PayItem,
			HaveItem,
			NotHaveItem,
			HaveBird,
			Level,
			CooldownFinished,
			IsSpecificWeekday,
			IsNotSpecificWeekday,
			HaveCurrentHotpsotState,
			HavePassedCycleTime,
			NotHavePassedCycleTime,
			NotHaveItemWithLevel,
			HaveItemWithLevel,
			UsedFriends,
			HaveUnlockedHotpsot,
			NotHaveUnlockedHotpsot,
			HaveLessThan,
			HaveBirdCount,
			HaveAllUpgrades,
			NotHaveAllUpgrades,
			NotUseBirdInBattle,
			UseBirdInBattle,
			HaveMasteryFactor,
			NotHaveMasteryFactor,
			NotHaveClass,
			IsConverted,
			HaveEventCampaignHotspotState,
			HaveTotalItemsInCollection,
			LostPvpBattle,
			HaveEventScore,
			HaveClass,
			HaveCurrentChronicleCaveState,
			TutorialCompleted,
			TotalMoneySpent,
			LostUnresolvedHotspot,
			UnlockedAllClasses,
			BirdMasteryFactorMinimum,
			BirdMasteryFactorMaximum,
			HighestLeagueReached,
			TimeSinceLastPurchase,
			DeclinedOffer,
			AcceptedOffer,
			EndedOffer,
			UnlockedAllSkins
		}

		[ProtoContract]
		public class Requirement
		{
			[ProtoMember(1)]
			public RequirementType RequirementType { get; set; }

			[ProtoMember(2)]
			public string NameId { get; set; }

			[ProtoMember(3)]
			public float Value { get; set; }
		}

		[ProtoContract]
		public class EventBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int MaxAmountOfEventItems { get; set; }

			[ProtoMember(3)]
			public Dictionary<string, int> EventGeneratorItemLootTable { get; set; }

			[ProtoMember(4)]
			public string AssetBaseId { get; set; }

			[ProtoMember(5)]
			public string LocaBaseId { get; set; }

			[ProtoMember(6)]
			public Dictionary<string, int> EventRewardLootTableWheel { get; set; }

			[ProtoMember(7)]
			public List<string> EventBonusLootTablesPerRank { get; set; }

			[ProtoMember(8)]
			public Dictionary<int, int> StarRatingForRanking { get; set; }

			[ProtoMember(9)]
			public Requirement RerollResultRequirement { get; set; }

			[ProtoMember(10)]
			public float TimeForEncounterRespawnInSec { get; set; }

			[ProtoMember(11)]
			public int MaxNumberOfCollectibles { get; set; }

			[ProtoMember(12)]
			public Dictionary<string, int> EventCollectibleGeneratorItemLootTable { get; set; }

			[ProtoMember(13)]
			public float TimeForCollectibleRespawnInSec { get; set; }

			[ProtoMember(14)]
			public float MiniCampaignUnlockDelay { get; set; }

			[ProtoMember(16)]
			public Dictionary<string, int> EventMiniCampaignItemLootTable { get; set; }

			[ProtoMember(17)]
			public Dictionary<string, int> EventBossItemLootTable { get; set; }

			[ProtoMember(18)]
			public string BossId { get; set; }

			[ProtoMember(19)]
			public string EventRewardFirstRank { get; set; }

			[ProtoMember(20)]
			public float RerollResultCostIncrease { get; set; }

			[ProtoMember(21)]
			public float RerollResultCostMax { get; set; }

			[ProtoMember(22)]
			public string RewardChestIdMain { get; set; }

			[ProtoMember(23)]
			public string RewardChestIdFallback { get; set; }
		}

		[ProtoContract]
		public class EventManagerBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string EventId { get; set; }

			[ProtoMember(3)]
			public uint EventTeaserStartTimeStamp { get; set; }

			[ProtoMember(4)]
			public uint EventStartTimeStamp { get; set; }

			[ProtoMember(5)]
			public uint EventEndTimeStamp { get; set; }

			[ProtoMember(6)]
			public uint MaximumMatchmakingPlayers { get; set; }

			[ProtoMember(7)]
			public string MatchmakingStrategy { get; set; }

			[ProtoMember(8)]
			public string LobbyPrefix { get; set; }

			[ProtoMember(9)]
			public int OnlineMatchmakeTimeoutInSec { get; set; }

			[ProtoMember(10)]
			public string OnlineFallbackMatchmakingStrategy { get; set; }

			[ProtoMember(11)]
			public int FailedWithNoPlayersCountTillFallback { get; set; }

			[ProtoMember(12)]
			public string OfflineGetCompetitorsFunction { get; set; }

			[ProtoMember(13)]
			public string OfflineGetCompetitorsFallbackFunction { get; set; }

			[ProtoMember(14)]
			public float WaitTimeForOtherPlayerToFillBossList { get; set; }
		}

		[ProtoContract]
		public class EventPlacementBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public List<Requirement> SpawnAbleRequirements { get; set; }

			[ProtoMember(3)]
			public string Category { get; set; }

			[ProtoMember(4)]
			public string OverrideBattleGroundName { get; set; }
		}

		[ProtoContract]
		public class PvPSeasonManagerBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public uint SeasonStartTimeStamp { get; set; }

			[ProtoMember(3)]
			public uint SeasonEndTimeStamp { get; set; }

			[ProtoMember(4)]
			public int SeasonTurnAmount { get; set; }

			[ProtoMember(5)]
			public List<string> PvPRewardLootTablesPerLeague { get; set; }

			[ProtoMember(6)]
			public List<string> PvPBonusLootTablesPerRank { get; set; }

			[ProtoMember(7)]
			public Dictionary<int, int> StarRatingForRanking { get; set; }

			[ProtoMember(8)]
			public List<Requirement> RerollResultRequirement { get; set; }

			[ProtoMember(9)]
			public uint MaximumMatchmakingPlayers { get; set; }

			[ProtoMember(10)]
			public int MaxLeague { get; set; }

			[ProtoMember(11)]
			public string LocaBaseId { get; set; }

			[ProtoMember(12)]
			public string MatchmakingStrategy { get; set; }

			[ProtoMember(13)]
			public string LobbyPrefix { get; set; }

			[ProtoMember(14)]
			public int OnlineMatchmakeTimeoutInSec { get; set; }

			[ProtoMember(15)]
			public string OnlineFallbackMatchmakingStrategy { get; set; }

			[ProtoMember(16)]
			public int FailedWithNoPlayersCountTillFallback { get; set; }

			[ProtoMember(17)]
			public string OfflineGetCompetitorsFunction { get; set; }

			[ProtoMember(18)]
			public string OfflineGetCompetitorsFallbackFunction { get; set; }

			[ProtoMember(19)]
			public string OfflineGetBattleFunction { get; set; }

			[ProtoMember(20)]
			public int TimeTillMatchmakingBattleRefreshes { get; set; }

			[ProtoMember(21)]
			public int HourOfDayToRefreshEnergyAndObjectives { get; set; }

			[ProtoMember(22)]
			public int MaxMatchmakingDifficulty { get; set; }

			[ProtoMember(23)]
			public List<string> PvpRewardFirstRank { get; set; }

			[ProtoMember(24)]
			public float RerollResultCostIncrease { get; set; }

			[ProtoMember(25)]
			public float RerollResultCostMax { get; set; }

			[ProtoMember(26)]
			public Dictionary<int, string> TresholdRewards { get; set; }

			[ProtoMember(27)]
			public int TrophyId { get; set; }
		}

		[ProtoContract]
		public class AchievementBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Value { get; set; }
		}

		public enum CharacterSizeType
		{
			Small,
			Medium,
			Large,
			Boss
		}

		[ProtoContract]
		public class BannerBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string AssetId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public string DefaultInventoryNameId { get; set; }

			[ProtoMember(5)]
			public int BaseHealth { get; set; }

			[ProtoMember(6)]
			public int PerLevelHealth { get; set; }

			[ProtoMember(7)]
			public CharacterSizeType SizeType { get; set; }

			[ProtoMember(8)]
			public int SortPriority { get; set; }
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

		[ProtoContract]
		public class BannerItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public List<string> SkillNameIds { get; set; }

			[ProtoMember(7)]
			public List<float> ColorVector { get; set; }

			[ProtoMember(8)]
			public Dictionary<string, int> ScrapLoot { get; set; }

			[ProtoMember(9)]
			public int BaseStat { get; set; }

			[ProtoMember(10)]
			public float StatPerLevelInPercent { get; set; }

			[ProtoMember(11)]
			public List<int> StatPerQualityBase { get; set; }

			[ProtoMember(12)]
			public List<int> StatPerQualityPercent { get; set; }

			[ProtoMember(13)]
			public string CorrespondingSetItem { get; set; }

			[ProtoMember(14)]
			public string UnlockableSetSkillNameId { get; set; }

			[ProtoMember(15)]
			public bool FlagAsNew { get; set; }

			[ProtoMember(16)]
			public EquipmentSource Mainsource { get; set; }

			[ProtoMember(17)]
			public bool HideInPreview { get; set; }

			[ProtoMember(18)]
			public int Stars { get; set; }
		}

		[ProtoContract]
		public class BasicItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public int SetAsNewInShop { get; set; }
		}

		[ProtoContract]
		public class BuyableShopOfferBalancingData : BasicShopOfferBalancingData
		{
			[ProtoMember(52)]
			public int DiscountPrice { get; set; }

			[ProtoMember(53)]
			public bool DisplayAfterPurchase { get; set; }

			[Obsolete]
			[ProtoMember(54)]
			public bool ExclusiveOffer { get; set; }
		}

		[ProtoContract]
		public class PremiumShopOfferBalancingData : BasicShopOfferBalancingData
		{
			[ProtoMember(71)]
			public string DiscountPriceIdent { get; set; }

			[ProtoMember(72)]
			public bool Sticky { get; set; }

			[ProtoMember(73)]
			public float StickyCooldown { get; set; }

			[ProtoMember(74)]
			public float DollarPrice { get; set; }

			[ProtoMember(75)]
			public string PrefabId { get; set; }

			[ProtoMember(76)]
			public string PrefabMiniId { get; set; }

			[ProtoMember(77)]
			public string ResultChestId { get; set; }

			[ProtoMember(78)]
			public string BannerLoca { get; set; }
		}

		[ProtoContract]
		public class GachaShopOfferBalancingData : BasicShopOfferBalancingData
		{
		}

		[ProtoContract]
		[ProtoInclude(50, typeof(BuyableShopOfferBalancingData))]
		[ProtoInclude(70, typeof(PremiumShopOfferBalancingData))]
		[ProtoInclude(90, typeof(GachaShopOfferBalancingData))]

		public class BasicShopOfferBalancingData
		{
			[ProtoMember(1)]
			public int Level { get; set; }

			[ProtoMember(2)]
			public Dictionary<string, int> OfferContents { get; set; }

			[ProtoMember(3)]
			public int SortPriority { get; set; }

			[ProtoMember(4)]
			public int SlotId { get; set; }

			[ProtoMember(5)]
			public string Category { get; set; }

			[ProtoMember(6)]
			public string NameId { get; set; }

			[ProtoMember(7)]
			public string AssetId { get; set; }

			[ProtoMember(8)]
			public string LocaId { get; set; }

			[ProtoMember(9)]
			public int DiscountValue { get; set; }

			[ProtoMember(10)]
			[Obsolete]
			public List<Requirement> DiscountRequirements { get; set; }

			[ProtoMember(11)]
			[Obsolete]
			public int DiscountStartDate { get; set; }

			[ProtoMember(12)]
			[Obsolete]
			public int DiscountEndDate { get; set; }

			[ProtoMember(13)]
			[Obsolete]
			public int DiscountDuration { get; set; }

			[ProtoMember(14)]
			public bool UniqueOffer { get; set; }

			[ProtoMember(15)]
			public string AtlasNameId { get; set; }

			[ProtoMember(16)]
			public List<float> SpecialOfferBackgroundColor { get; set; }

			[ProtoMember(17)]
			public List<float> SpecialOfferLabelColor { get; set; }

			[ProtoMember(18)]
			public string SpecialOfferLocaId { get; set; }

			[ProtoMember(19)]
			public bool DisplayAsLarge { get; set; }

			[Obsolete]
			[ProtoMember(20)]
			public bool ShowDiscountPopup { get; set; }

			[Obsolete]
			[ProtoMember(21)]
			public string PopupLoca { get; set; }

			[Obsolete]
			[ProtoMember(22)]
			public string PopupIconId { get; set; }

			[ProtoMember(23)]
			[Obsolete]
			public string PopupAtlasId { get; set; }

			[Obsolete]
			[ProtoMember(24)]
			public int DiscountCooldown { get; set; }

			[ProtoMember(25)]
			public int Duration { get; set; }

			[ProtoMember(26)]
			public int StartDate { get; set; }

			[ProtoMember(27)]
			public int EndDate { get; set; }

			[ProtoMember(28)]
			public List<Requirement> BuyRequirements { get; set; }

			[ProtoMember(29)]
			public List<Requirement> ShowRequirements { get; set; }

			[ProtoMember(30)]
			[Obsolete]
			public int SpecialOfferPrio { get; set; }

			[Obsolete]
			[ProtoMember(31)]
			public string SpeechBubbleLoca { get; set; }

			[ProtoMember(32)]
			[Obsolete]
			public bool AlwaysShowSpeechBubble { get; set; }

			[ProtoMember(33)]
			public bool HideUnlessOnSale { get; set; }
		}

		[ProtoContract]
		public class ChronicleCaveBattleBalancingData : BattleBalancingData
		{
		}

		public enum ScoringStrategy
		{
			FixedMaximum,
			MaximumByStrength,
			PvP
		}

		public enum Faction
		{
			Birds,
			Pigs,
			None,
			NonAttackablePig
		}

		[ProtoContract]
		[ProtoInclude(90, typeof(ChronicleCaveBattleBalancingData))]
		public class BattleBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int BaseLevel { get; set; }

			[ProtoMember(3)]
			public List<string> BattleParticipantsIds { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, int> LootTableWheel { get; set; }

			[ProtoMember(5)]
			public Dictionary<string, int> LootTableAdditional { get; set; }

			[ProtoMember(6)]
			public int StrengthPoints { get; set; }

			[ProtoMember(7)]
			public List<Requirement> BattleRequirements { get; set; }

			[ProtoMember(8)]
			public Dictionary<string, int> LootTableLost { get; set; }

			[ProtoMember(9)]
			public string BackgroundAssetId { get; set; }

			[ProtoMember(10)]
			public int UsableFriendBirdsCount { get; set; }

			[ProtoMember(11)]
			public string SoundAssetId { get; set; }

			[ProtoMember(12)]
			public ScoringStrategy ScoringStrategy { get; set; }

			[ProtoMember(13)]
			public int BonusPoints { get; set; }

			[ProtoMember(14)]
			public Dictionary<Faction, string> EnvironmentalEffects { get; set; }

			[ProtoMember(15)]
			public int EnvironmentalStartWave { get; set; }

			[ProtoMember(16)]
			public int MaxBirdsInBattle { get; set; }

			[ProtoMember(17)]
			public float AdditionalAttackInPercent { get; set; }

			[ProtoMember(18)]
			public float AdditionalHealthInPercent { get; set; }

			[ProtoMember(19)]
			public Dictionary<int, string> LootTableWheelAfterWave { get; set; }

			[ProtoMember(20)]
			public string Force_Character { get; set; }

			[ProtoMember(21)]
			public float PowerLevelThresholdLow { get; set; }

			[ProtoMember(22)]
			public float PowerLevelThresholdHigh { get; set; }

			[ProtoMember(23)]
			public bool ApplyPowerLevelBalancing { get; set; }

			[ProtoMember(24)]
			public int Difficulty { get; set; }

			[ProtoMember(25)]
			public Dictionary<string, int> BonusLoot { get; set; }

			[ProtoMember(26)]
			public string BattleType { get; set; }

			[ProtoMember(27)]
			public float PowerlevelModifier { get; set; }
		}


		[ProtoContract]
		public class BattleHintBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string LocaId { get; set; }

			[ProtoMember(3)]
			public string TopIconId { get; set; }

			[ProtoMember(4)]
			public string TopAtlasId { get; set; }

			[ProtoMember(5)]
			public string BottomIconId { get; set; }

			[ProtoMember(6)]
			public string BottomAtlasId { get; set; }

			[ProtoMember(7)]
			public Dictionary<string, int> RecommendedClasses { get; set; }
		}

		[ProtoContract]
		public class ChronicleCaveBattleParticipantTableBalancingData : BattleParticipantTableBalancingData
		{
		}

		public enum BattleParticipantTableType
		{
			IgnoreStrength,
			Weighted,
			Probability
		}

		public enum VictoryConditionTypes
		{
			DefeatAll,
			DefeatExplicit,
			SurviveTurns,
			CharacterSurviveTurns,
			FinishInTurns
		}

		[ProtoContract]
		public class VictoryCondition
		{
			[ProtoMember(1)]
			public VictoryConditionTypes Type { get; set; }

			[ProtoMember(2)]
			public string NameId { get; set; }

			[ProtoMember(3)]
			public float Value { get; set; }
		}

		[ProtoContract]
		public class BattleParticipantTableEntry
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int LevelDifference { get; set; }

			[ProtoMember(3)]
			public float Probability { get; set; }

			[ProtoMember(4)]
			public float Amount { get; set; }

			[ProtoMember(5)]
			public bool Unique { get; set; }

			[ProtoMember(6)]
			public bool ForcePercent { get; set; }
		}

		[ProtoInclude(90, typeof(ChronicleCaveBattleParticipantTableBalancingData))]
		[ProtoContract]
		public class BattleParticipantTableBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public BattleParticipantTableType Type { get; set; }

			[ProtoMember(3)]
			public VictoryCondition VictoryCondition { get; set; }

			[ProtoMember(4)]
			public List<BattleParticipantTableEntry> BattleParticipants { get; set; }
		}

		[ProtoContract]
		public class BirdBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string AssetId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public string DefaultInventoryNameId { get; set; }

			[ProtoMember(5)]
			public int BaseHealth { get; set; }

			[ProtoMember(6)]
			public int BaseAttack { get; set; }

			[ProtoMember(7)]
			public int PerLevelHealth { get; set; }

			[ProtoMember(8)]
			public int PerLevelAttack { get; set; }

			[ProtoMember(9)]
			public CharacterSizeType SizeType { get; set; }

			[ProtoMember(10)]
			public string RageSkillIdent { get; set; }

			[ProtoMember(11)]
			public int SortPriority { get; set; }

			[ProtoMember(12)]
			public string PvPRageSkillIdent { get; set; }
		}

		[ProtoContract]
		public class BonusPerFriendBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Count { get; set; }

			[ProtoMember(3)]
			public float AttackBonus { get; set; }

			[ProtoMember(4)]
			public float HealthBonus { get; set; }

			[ProtoMember(5)]
			public float XPBonus { get; set; }

			[ProtoMember(6)]
			public string UnlockedClassNameId { get; set; }
		}

		[ProtoContract]
		public class AiCombo
		{
			[ProtoMember(1)]
			public float Percentage { get; set; }

			[ProtoMember(2)]
			public List<string> ComboChain { get; set; }
		}

		[ProtoContract]
		public class BossBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string AssetId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public string DefaultInventoryNameId { get; set; }

			[ProtoMember(5)]
			public int BaseHealth { get; set; }

			[ProtoMember(6)]
			public int BaseAttack { get; set; }

			[ProtoMember(7)]
			public int PerLevelHealth { get; set; }

			[ProtoMember(8)]
			public int PerLevelAttack { get; set; }

			[ProtoMember(9)]
			public List<string> SkillNameIds { get; set; }

			[ProtoMember(10)]
			public List<AiCombo> SkillCombos { get; set; }

			[ProtoMember(11)]
			public CharacterSizeType SizeType { get; set; }

			[ProtoMember(12)]
			public Faction Faction { get; set; }

			[ProtoMember(13)]
			public Dictionary<string, int> LootTableDefeatBonus { get; set; }

			[ProtoMember(14)]
			public float SizeScale { get; set; }

			[ProtoMember(15)]
			public int PigStrength { get; set; }

			[ProtoMember(16)]
			public string PassiveSkillNameId { get; set; }

			[ProtoMember(17)]
			public bool IgnoreDifficulty { get; set; }

			[ProtoMember(18)]
			public int AttacksNeeded { get; set; }

			[ProtoMember(19)]
			public Dictionary<string, int> RewardForKillingBlow { get; set; }

			[ProtoMember(20)]
			public Dictionary<string, int> KillRewardForAll { get; set; }

			[ProtoMember(21)]
			public int ReduceScorePercentageOnBosswin { get; set; }

			[ProtoMember(22)]
			public int DurationOfCampDestruction { get; set; }

			[ProtoMember(23)]
			public int TimeToReachCamp { get; set; }

			[ProtoMember(24)]
			public int TimeToReactivate { get; set; }

			[ProtoMember(25)]
			public string CollectionGroupId { get; set; }

			[ProtoMember(26)]
			public string DefeatedLabelLocaId { get; set; }
		}

		[ProtoContract]
		public class ChronicleCaveBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }
		}

		[ProtoContract]
		public class ChronicleCaveFloorBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int TemplateId { get; set; }

			[ProtoMember(3)]
			public string BackgroundId { get; set; }

			[ProtoMember(4)]
			public List<string> ChronicleCaveHotspotIds { get; set; }

			[ProtoMember(5)]
			public string FirstChronicleCaveHotspotId { get; set; }

			[ProtoMember(6)]
			public string LastChronicleCaveHotspotId { get; set; }

			[ProtoMember(7)]
			public string BossNameId { get; set; }

			[ProtoMember(8)]
			public Dictionary<Faction, string> EnvironmentalEffects { get; set; }
		}

		[ProtoContract]
		public class ChronicleCaveHotspotBalancingData : HotspotBalancingData
		{
		}

		public enum InterruptAction
		{
			none,
			support,
			resetChain
		}

		[ProtoContract]
		public class ClassItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public List<string> SkillNameIds { get; set; }

			[ProtoMember(7)]
			public string RestrictedBirdId { get; set; }

			[ProtoMember(8)]
			public List<float> AttackBoostPerMasteryRank { get; set; }

			[ProtoMember(9)]
			public List<float> HealthBoostPerMasteryRank { get; set; }

			[ProtoMember(10)]
			public List<AiCombo> SkillCombos { get; set; }

			[ProtoMember(11)]
			public string ReplacementClassNameId { get; set; }

			[ProtoMember(12)]
			public List<string> InterruptConditionCombos { get; set; }

			[ProtoMember(13)]
			public InterruptAction InterruptAction { get; set; }

			[ProtoMember(14)]
			public string Mastery { get; set; }

			[ProtoMember(15)]
			public List<AiCombo> PvPSkillCombos { get; set; }

			[ProtoMember(16)]
			public List<string> PvPSkillNameIds { get; set; }

			[ProtoMember(17)]
			public bool IsPremium { get; set; }

			[ProtoMember(18)]
			public uint AvailableAt { get; set; }

			[ProtoMember(19)]
			public uint TeasedAt { get; set; }

			[ProtoMember(20)]
			public bool Inactive { get; set; }
		}

		[ProtoContract]
		public class ClassSkinBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string OriginalClass { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public float BonusHp { get; set; }

			[ProtoMember(7)]
			public float BonusDamage { get; set; }

			[ProtoMember(8)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(9)]
			public bool ShowPreview { get; set; }

			[ProtoMember(10)]
			public bool UseInPvPFallback { get; set; }

			[ProtoMember(11)]
			public string PassiveSkillNameId { get; set; }

			[ProtoMember(12)]
			public bool PartOfSaleBundles { get; set; }
		}

		[ProtoContract]
		public class ClientConfigBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string BundleId { get; set; }

			[ProtoMember(3)]
			public long AppleAppId { get; set; }

			[ProtoMember(4)]
			public string GooglePlayStorePublicKey { get; set; }

			[ProtoMember(5)]
			public string GoogleAppId { get; set; }

			[ProtoMember(6)]
			public string Wp8AppId { get; set; }

			[ProtoMember(7)]
			public string SoundtrackURL { get; set; }

			[ProtoMember(8)]
			public string ABTestingGroup { get; set; }

			[ProtoMember(9)]
			public bool UseAutoBattle { get; set; }

			[ProtoMember(11)]
			public int FacebookFriendsPerRequest { get; set; }

			[ProtoMember(12)]
			public bool UseSkynestTimingService { get; set; }

			[ProtoMember(13)]
			public bool ApplyMinusBillionFix { get; set; }

			[ProtoMember(14)]
			public int TimeZonePersistCooldownInSec { get; set; }

			[ProtoMember(15)]
			public int PvPEquipmentLevelDeltaCap { get; set; }

			[ProtoMember(16)]
			public bool EnableProfileMerging { get; set; }

			[ProtoMember(17)]
			public bool EnableSingleBirdRevive { get; set; }

			[ProtoMember(18)]
			public bool IntroVideoSkippable { get; set; }

			[ProtoMember(19)]
			public List<Requirement> ShowInterstitialRequirements { get; set; }

			[ProtoMember(20)]
			public bool UseChimeraLeaderboards { get; set; }
		}

		[ProtoContract]
		public class CollectionGroupBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string LocaBaseId { get; set; }

			[ProtoMember(3)]
			public Dictionary<string, int> Reward { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, int> FallbackReward { get; set; }

			[ProtoMember(5)]
			public string RewardAssetBaseId { get; set; }

			[ProtoMember(6)]
			public string FallbackRewardAssetBaseId { get; set; }

			[ProtoMember(7)]
			public string RewardLocaId { get; set; }

			[ProtoMember(8)]
			public string FallbackRewardLocaId { get; set; }

			[ProtoMember(9)]
			public List<Requirement> ComponentRequirements { get; set; }

			[ProtoMember(10)]
			public Dictionary<string, int> ComponentFallbackLoot { get; set; }

			[ProtoMember(11)]
			public List<Requirement> FallbackRewardRequirements { get; set; }

			[ProtoMember(12)]
			public Dictionary<string, int> EasyBattleFallbackLoot { get; set; }

			[ProtoMember(13)]
			public Dictionary<string, int> MediumBattleFallbackLoot { get; set; }

			[ProtoMember(14)]
			public Dictionary<string, int> HardBattleFallbackLoot { get; set; }

			[ProtoMember(15)]
			public Dictionary<string, int> HardBattleSecondaryFallbackLoot { get; set; }

			[ProtoMember(16)]
			public Dictionary<string, int> MediumBattleSecondaryFallbackLoot { get; set; }

			[ProtoMember(17)]
			public Dictionary<string, int> EasyBattleSecondaryFallbackLoot { get; set; }
		}

		public enum ConditionalLootTableDropTrigger
		{
			NotFirstStartUp,
			FirstStartUp,
			RemoveNotFirstStartUp
		}

		[ProtoContract]
		public class ConditionalInventoryBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public List<Requirement> DropRequirements { get; set; }

			[ProtoMember(3)]
			public Dictionary<string, int> Content { get; set; }

			[ProtoMember(4)]
			public int InitializingLevel { get; set; }

			[ProtoMember(5)]
			public ConditionalLootTableDropTrigger Trigger { get; set; }
		}

		[ProtoContract]
		public class ConsumableItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public string SkillNameId { get; set; }

			[ProtoMember(7)]
			public Dictionary<string, float> SkillParameters { get; set; }

			[ProtoMember(8)]
			public Dictionary<string, float> SkillParametersDeltaPerLevel { get; set; }

			[ProtoMember(9)]
			public string ConsumableStatckingType { get; set; }

			[ProtoMember(10)]
			public int ConversionPoints { get; set; }

			[ProtoMember(11)]
			public string InstantBuyOfferCategoryId { get; set; }
		}

		[ProtoContract]
		public class CraftingItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public string Recipe { get; set; }

			[ProtoMember(7)]
			public string BaseItemNameId { get; set; }

			[ProtoMember(8)]
			public int ValueOfBaseItem { get; set; }

			[ProtoMember(9)]
			public string AtlasNameId { get; set; }
		}

		[ProtoContract]
		public class CraftingRecipeBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public Dictionary<string, int> ResultLoot { get; set; }

			[ProtoMember(7)]
			public InventoryItemType RecipeCategoryType { get; set; }
		}

		[ProtoContract]
		public class CustomMessageBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string NPCNameId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, int> LootTableReward { get; set; }

			[ProtoMember(5)]
			public string ButtonAtlasId { get; set; }

			[ProtoMember(6)]
			public string ButtonSpriteNameId { get; set; }

			[ProtoMember(7)]
			public string URLToOpen { get; set; }

			[ProtoMember(8)]
			public List<Requirement> AddMessageRequirements { get; set; }
		}

		[ProtoContract]
		public class DailyLoginGiftsBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public Dictionary<string, int> Day1 { get; set; }

			[ProtoMember(3)]
			public Dictionary<string, int> Day2 { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, int> Day3 { get; set; }

			[ProtoMember(5)]
			public Dictionary<string, int> Day4 { get; set; }

			[ProtoMember(6)]
			public Dictionary<string, int> Day5 { get; set; }

			[ProtoMember(7)]
			public Dictionary<string, int> Day6 { get; set; }

			[ProtoMember(8)]
			public Dictionary<string, int> Day7 { get; set; }

			[ProtoMember(9)]
			public Dictionary<string, int> Day8 { get; set; }

			[ProtoMember(10)]
			public Dictionary<string, int> Day9 { get; set; }

			[ProtoMember(11)]
			public Dictionary<string, int> Day10 { get; set; }

			[ProtoMember(12)]
			public Dictionary<string, int> Day11 { get; set; }

			[ProtoMember(13)]
			public Dictionary<string, int> Day12 { get; set; }

			[ProtoMember(14)]
			public Dictionary<string, int> Day13 { get; set; }

			[ProtoMember(15)]
			public Dictionary<string, int> Day14 { get; set; }

			[ProtoMember(16)]
			public Dictionary<string, int> Day15 { get; set; }

			[ProtoMember(17)]
			public Dictionary<string, int> Day16 { get; set; }

			[ProtoMember(18)]
			public Dictionary<string, int> Day17 { get; set; }

			[ProtoMember(19)]
			public Dictionary<string, int> Day18 { get; set; }

			[ProtoMember(20)]
			public Dictionary<string, int> Day19 { get; set; }

			[ProtoMember(21)]
			public Dictionary<string, int> Day20 { get; set; }

			[ProtoMember(22)]
			public Dictionary<string, int> Day21 { get; set; }

			[ProtoMember(23)]
			public Dictionary<string, int> Day22 { get; set; }

			[ProtoMember(24)]
			public Dictionary<string, int> Day23 { get; set; }

			[ProtoMember(25)]
			public Dictionary<string, int> Day24 { get; set; }

			[ProtoMember(26)]
			public Dictionary<string, int> Day25 { get; set; }

			[ProtoMember(27)]
			public Dictionary<string, int> Day26 { get; set; }

			[ProtoMember(28)]
			public Dictionary<string, int> Day27 { get; set; }

			[ProtoMember(29)]
			public Dictionary<string, int> Day28 { get; set; }

			[ProtoMember(30)]
			public Dictionary<string, int> Day29 { get; set; }

			[ProtoMember(31)]
			public Dictionary<string, int> Day30 { get; set; }

			[ProtoMember(32)]
			public Dictionary<string, int> Day31 { get; set; }

			[ProtoMember(33)]
			public List<int> HighLightDays { get; set; }
		}

		[ProtoContract]
		public class EnchantingBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float StatsBoost { get; set; }

			[ProtoMember(3)]
			public bool Stars0Allowed { get; set; }

			[ProtoMember(4)]
			public bool Stars1Allowed { get; set; }

			[ProtoMember(5)]
			public bool Stars2Allowed { get; set; }

			[ProtoMember(6)]
			public bool Stars3Allowed { get; set; }

			[ProtoMember(7)]
			public bool SetAllowed { get; set; }

			[ProtoMember(8)]
			public List<Requirement> BuyRequirements { get; set; }

			[ProtoMember(9)]
			public float ResourceCosts { get; set; }

			[ProtoMember(10)]
			public float Lvl1ResPoints { get; set; }

			[ProtoMember(11)]
			public float Lvl2ResPoints { get; set; }

			[ProtoMember(12)]
			public float Lvl3ResPoints { get; set; }

			[ProtoMember(13)]
			public float ScrappingBonus { get; set; }

			[ProtoMember(14)]
			public List<Requirement> BuyRequirementsSet { get; set; }

			[ProtoMember(15)]
			public List<Requirement> SkipCostRequirement { get; set; }

			[ProtoMember(16)]
			public int EnchantmentLevel { get; set; }

			[ProtoMember(17)]
			public int Levelrange { get; set; }

			[ProtoMember(18)]
			public float BoosterResPoints { get; set; }
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

		[ProtoContract]
		public class EquipmentPerk
		{
			[ProtoMember(1)]
			public PerkType Type { get; set; }

			[ProtoMember(2)]
			public float ProbablityInPercent { get; set; }

			[ProtoMember(3)]
			public float PerkValue { get; set; }
		}

		[ProtoContract]
		public class EquipmentBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public int BaseStat { get; set; }

			[ProtoMember(7)]
			public int StatPerLevel { get; set; }

			[ProtoMember(8)]
			public int StatPerQuality { get; set; }

			[ProtoMember(9)]
			public List<int> StatPerQualityPercent { get; set; }

			[ProtoMember(10)]
			public int AnimationIndex { get; set; }

			[ProtoMember(11)]
			public bool IsRanged { get; set; }

			[ProtoMember(12)]
			public string ProjectileAssetId { get; set; }

			[ProtoMember(13)]
			public EquipmentPerk Perk { get; set; }

			[ProtoMember(14)]
			public string RestrictedBirdId { get; set; }

			[ProtoMember(15)]
			public bool DirectAssetAndLoca { get; set; }

			[ProtoMember(16)]
			public string CorrespondingSetItemId { get; set; }

			[ProtoMember(17)]
			public int AssetLevelOffset { get; set; }

			[ProtoMember(18)]
			public int AssetCycleCount { get; set; }

			[ProtoMember(19)]
			public string HitEffectSuffix { get; set; }

			[ProtoMember(20)]
			public string SetItemSkill { get; set; }

			[ProtoMember(21)]
			public bool DirectProjectileAssetAndLoca { get; set; }

			[ProtoMember(22)]
			public EquipmentSource Mainsource { get; set; }

			[ProtoMember(23)]
			public bool ShowAsNew { get; set; }

			[ProtoMember(24)]
			public bool HideInPreview { get; set; }

			[ProtoMember(25)]
			public string PvpSetItemSkill { get; set; }
		}

		[ProtoContract]
		public class EventItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public List<string> EventParameters { get; set; }

			[ProtoMember(7)]
			public List<string> SpawnCategories { get; set; }
		}

		[ProtoContract]
		public class EventPopupBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public uint StartTimeStamp { get; set; }

			[ProtoMember(3)]
			public uint EndTimeStamp { get; set; }
		}

		[ProtoContract]
		public class ExperienceLevelBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Experience { get; set; }

			[ProtoMember(3)]
			public Dictionary<string, int> LootTableAdditional { get; set; }

			[ProtoMember(4)]
			public int MatchmakingRangeIndex { get; set; }

			[ProtoMember(5)]
			public float MasteryModifier { get; set; }

			[ProtoMember(6)]
			public int OldExperience { get; set; }
		}

		[ProtoContract]
		public class ExperienceMasteryBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Experience { get; set; }

			[ProtoMember(3)]
			public int OldExperience { get; set; }

			[ProtoMember(4)]
			public int AncientExperience { get; set; }

			[ProtoMember(5)]
			public int StatBonus { get; set; }
		}

		[ProtoContract]
		public class ExperienceScalingBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float Difference { get; set; }

			[ProtoMember(3)]
			public float XpModifier { get; set; }
		}

		[ProtoContract]
		public class GlobalDifficultyBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float BirdAttackInPercent { get; set; }

			[ProtoMember(3)]
			public float BirdHealthInPercent { get; set; }

			[ProtoMember(4)]
			public float EquipmentInPercent { get; set; }

			[ProtoMember(5)]
			public float PigAttackInPercent { get; set; }

			[ProtoMember(6)]
			public float PigHealthInPercent { get; set; }

			[ProtoMember(7)]
			public float MaxStrengthPointAdjustment { get; set; }
		}

		public enum HotspotType
		{
			Unknown,
			Battle,
			Resource,
			Node
		}

		[ProtoContract]
		[ProtoInclude(90, typeof(ChronicleCaveHotspotBalancingData))]
		public class HotspotBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public HotspotType Type { get; set; }

			[ProtoMember(3)]
			public List<string> BattleId { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, int> HotspotContents { get; set; }

			[ProtoMember(5)]
			public string ZoneLocaIdent { get; set; }

			[ProtoMember(6)]
			public int ZoneStageIndex { get; set; }

			[ProtoMember(7)]
			public List<Requirement> EnterRequirements { get; set; }

			[ProtoMember(8)]
			public uint CooldownInSeconds { get; set; }

			[ProtoMember(9)]
			public List<Requirement> VisibleRequirements { get; set; }

			[ProtoMember(10)]
			public bool IsSpawnEventPossible { get; set; }

			[ProtoMember(11)]
			public int ProgressId { get; set; }

			[ProtoMember(12)]
			public int OrderId { get; set; }

			[ProtoMember(13)]
			public bool IsSpawnGoldenPigPossible { get; set; }

			[ProtoMember(14)]
			public bool UseProgressIndicator { get; set; }

			[ProtoMember(15)]
			public bool AutoSpawnBirds { get; set; }

			[ProtoMember(16)]
			public bool CountForStars { get; set; }

			[ProtoMember(17)]
			public int MaxLevel { get; set; }
		}

		[ProtoContract]
		public class InventoryBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public Dictionary<string, int> DefaultInventoryContent { get; set; }

			[ProtoMember(3)]
			public int InitializingLevel { get; set; }
		}

		public enum LoadingArea
		{
			Worldmap,
			Camp,
			Arena,
			Battle,
			ChronicleCave
		}

		[ProtoContract]
		public class LoadingHintBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public List<Requirement> ShowRequirements { get; set; }

			[ProtoMember(3)]
			public float Weight { get; set; }

			[ProtoMember(4)]
			public List<LoadingArea> TargetAreas { get; set; }
		}

		[ProtoContract]
		public class LootTableEntry
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int LevelMinIncl { get; set; }

			[ProtoMember(3)]
			public int LevelMaxExcl { get; set; }

			[ProtoMember(4)]
			public float Probability { get; set; }

			[ProtoMember(5)]
			public int BaseValue { get; set; }

			[ProtoMember(6)]
			public int Span { get; set; }

			[ProtoMember(7)]
			public int CurrentPlayerLevelDelta { get; set; }
		}

		public enum LootTableType
		{
			Probability,
			Weighted,
			Inventory,
			Wheel,
			WheelForced
		}

		[ProtoContract]
		public class LootTableBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public List<LootTableEntry> LootTableEntries { get; set; }

			[ProtoMember(3)]
			public LootTableType Type { get; set; }

			[ProtoMember(4)]
			public string PrefabId { get; set; }

			[ProtoMember(5)]
			public string LocaId { get; set; }
		}

		[ProtoContract]
		public class MasteryItemBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public InventoryItemType ItemType { get; set; }

			[ProtoMember(3)]
			public string AssetBaseId { get; set; }

			[ProtoMember(4)]
			public string LocaBaseId { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public string AssociatedBird { get; set; }

			[ProtoMember(7)]
			public string AssociatedClass { get; set; }

			[ProtoMember(8)]
			public string SetAsNewInShop { get; set; }

			[ProtoMember(9)]
			public List<int> MasteryPointsForRankUp { get; set; }

			[ProtoMember(10)]
			public Dictionary<string, int> FallbackLootTable { get; set; }

			[ProtoMember(11)]
			public Dictionary<string, int> FallbackLootTableDailyLogin { get; set; }

			[ProtoMember(12)]
			public List<int> MasteryPointsForRankUpOld { get; set; }
		}

		[ProtoContract]
		public class MiniCampaignBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(3)]
			public List<string> HotspotIds { get; set; }

			[ProtoMember(5)]
			public string PortalEventNodeId { get; set; }

			[ProtoMember(6)]
			public string LocaBaseId { get; set; }

			[ProtoMember(7)]
			public string CollectionGroupId { get; set; }

			[ProtoMember(8)]
			public int ProgressSummand { get; set; }

			[ProtoMember(9)]
			public string MusicTitle { get; set; }
		}

		[ProtoContract]
		public class PigBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string AssetId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public string DefaultInventoryNameId { get; set; }

			[ProtoMember(5)]
			public int BaseHealth { get; set; }

			[ProtoMember(6)]
			public int BaseAttack { get; set; }

			[ProtoMember(7)]
			public int PerLevelHealth { get; set; }

			[ProtoMember(8)]
			public int PerLevelAttack { get; set; }

			[ProtoMember(9)]
			public List<string> SkillNameIds { get; set; }

			[ProtoMember(10)]
			public List<AiCombo> SkillCombos { get; set; }

			[ProtoMember(11)]
			public CharacterSizeType SizeType { get; set; }

			[ProtoMember(12)]
			public Faction Faction { get; set; }

			[ProtoMember(13)]
			public Dictionary<string, int> LootTableDefeatBonus { get; set; }

			[ProtoMember(14)]
			public float SizeScale { get; set; }

			[ProtoMember(15)]
			public int PigStrength { get; set; }

			[ProtoMember(16)]
			public string PassiveSkillNameId { get; set; }

			[ProtoMember(17)]
			public bool IgnoreDifficulty { get; set; }
		}

		[ProtoContract]
		public class PigTypePowerLevelBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float PowerLevelWeight { get; set; }
		}

		[ProtoContract]
		public class PowerLevelBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float AttackModifier { get; set; }

			[ProtoMember(3)]
			public float HealthModifier { get; set; }

			[ProtoMember(4)]
			[Obsolete]
			public float PowerBaseWeight { get; set; }

			[ProtoMember(5)]
			public int ExpectedPlayerPowerlevel { get; set; }
		}

		[ProtoContract]
		public class PvPAIBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int BasicBannerWeight { get; set; }

			[ProtoMember(3)]
			public int BasicBirdWeight { get; set; }

			[ProtoMember(4)]
			public int AddBannerWeightBelow80 { get; set; }

			[ProtoMember(5)]
			public int AddBannerWeightBelow60 { get; set; }

			[ProtoMember(6)]
			public int AddBannerWeightBelow40 { get; set; }

			[ProtoMember(7)]
			public int AddBannerWeightBelow20 { get; set; }

			[ProtoMember(8)]
			public int AddBirdWeightBelow50 { get; set; }

			[ProtoMember(9)]
			public int AddBirdWeightBelow30 { get; set; }

			[ProtoMember(10)]
			public int RageRedPrio { get; set; }

			[ProtoMember(11)]
			public int RageYellowPrio { get; set; }

			[ProtoMember(12)]
			public int RageBlackPrio { get; set; }

			[ProtoMember(13)]
			public int RageBluePrio { get; set; }

			[ProtoMember(14)]
			public float ChanceToUseRandomTarget { get; set; }

			[ProtoMember(15)]
			public int RageWhitePrio { get; set; }

			[ProtoMember(16)]
			public float ChanceToFocusBirdWith3 { get; set; }

			[ProtoMember(17)]
			public float ChanceToFocusBirdWith2 { get; set; }

			[ProtoMember(18)]
			public float ChanceToFocusBirdWith1 { get; set; }
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

		[ProtoContract]
		public class PvPObjectivesBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public ObjectivesRequirement Requirement { get; set; }

			[ProtoMember(3)]
			public string LocaIdent { get; set; }

			[ProtoMember(4)]
			public string Requirementvalue { get; set; }

			[ProtoMember(5)]
			public string Requirementvalue2 { get; set; }

			[ProtoMember(6)]
			public int Amount { get; set; }

			[ProtoMember(7)]
			public string Difficulty { get; set; }

			[ProtoMember(8)]
			public string AssetIconID { get; set; }

			[ProtoMember(9)]
			public int DailyGroupId { get; set; }

			[ProtoMember(10)]
			public int Reward { get; set; }

			[ProtoMember(11)]
			public int Playerlevel { get; set; }
		}

		[ProtoContract]
		public class ResourceCostPerLevelBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Level { get; set; }

			[ProtoMember(3)]
			public string AppliedItemNameId { get; set; }

			[ProtoMember(4)]
			public string FirstMaterialNameId { get; set; }

			[ProtoMember(5)]
			public int FirstMaterialAmount { get; set; }

			[ProtoMember(6)]
			public string SecondMaterialNameId { get; set; }

			[ProtoMember(7)]
			public int SecondMaterialAmount { get; set; }

			[ProtoMember(8)]
			public string ThirdMaterialNameId { get; set; }

			[ProtoMember(9)]
			public int ThirdMaterialAmount { get; set; }

			[ProtoMember(10)]
			public string FallbackItemName { get; set; }

			[ProtoMember(11)]
			public int FallbackItemCount { get; set; }
		}

		public enum SaleContentType
		{
			ShopItems,
			Mastery,
			RainbowRiot,
			GenericBundle,
			ClassBundle,
			SetBundle,
			LuckyCoinDiscount,
			Chain
		}

		public enum SaleAvailabilityType
		{
			Timed,
			PersonalTimeWindow,
			TimedSequence,
			Conditional,
			ConditionalCooldown
		}

		public enum SaleItemGrouping
		{
			Simultaneous,
			Sequence
		}

		public enum SaleParameter
		{
			Price,
			Value,
			Special
		}

		[ProtoContract]
		public class SaleItemDetails
		{
			[ProtoMember(1)]
			public string SubjectId { get; set; }

			[ProtoMember(2)]
			public SaleParameter SaleParameter { get; set; }

			[ProtoMember(3)]
			public int ChangedValue { get; set; }

			[ProtoMember(4)]
			public string ReplacementProductId { get; set; }
		}

		[ProtoContract]
		public class SalesManagerBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public SaleContentType ContentType { get; set; }

			[ProtoMember(3)]
			public SaleAvailabilityType SaleType { get; set; }

			[ProtoMember(4)]
			public List<SaleItemDetails> SaleDetails { get; set; }

			[Obsolete]
			[ProtoMember(5)]
			public SaleItemGrouping Grouping { get; set; }

			[ProtoMember(6)]
			public uint StartTime { get; set; }

			[ProtoMember(7)]
			public uint EndTime { get; set; }

			[ProtoMember(8)]
			public List<Requirement> Requirements { get; set; }

			[ProtoMember(9)]
			public int Duration { get; set; }

			[ProtoMember(10)]
			public int SortPriority { get; set; }

			[ProtoMember(11)]
			public string PopupIconId { get; set; }

			[ProtoMember(12)]
			public string PopupAtlasId { get; set; }

			[ProtoMember(13)]
			public string LocaBaseId { get; set; }

			[ProtoMember(14)]
			[Obsolete]
			public List<float> OfferLabelColor { get; set; }

			[ProtoMember(15)]
			[Obsolete]
			public List<float> OfferBackgroundColor { get; set; }

			[ProtoMember(16)]
			public string CheckoutCategory { get; set; }

			[ProtoMember(17)]
			public int Cooldown { get; set; }

			[ProtoMember(18)]
			public bool ShowContentsInPopup { get; set; }

			[ProtoMember(19)]
			public int PriorityInQueue { get; set; }

			[ProtoMember(20)]
			public bool RecheckRequirements { get; set; }

			[ProtoMember(21)]
			public string PrefabId { get; set; }

			[ProtoMember(22)]
			public bool Unique { get; set; }

			[ProtoMember(23)]
			public bool Infinite { get; set; }
		}

		[ProtoContract]
		public class ScoreBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int ScorePerStarNeeded { get; set; }

			[ProtoMember(3)]
			public int ScoreForAllFullBirds { get; set; }

			[ProtoMember(4)]
			public int ScoreForAllFullPigs { get; set; }

			[ProtoMember(5)]
			public int PigScoreLossPerTurnInPercent { get; set; }

			[ProtoMember(6)]
			public int MinimumBirdScoreInPercent { get; set; }

			[ProtoMember(7)]
			public int MinimumPigScoreInPercent { get; set; }

			[ProtoMember(8)]
			public int ScorePerStrengthPoint { get; set; }

			[ProtoMember(9)]
			public int ScorePerBird { get; set; }

			[ProtoMember(10)]
			public int MinimumBannerScoreInPercent { get; set; }

			[ProtoMember(11)]
			public int ScoreForBannerDefeated { get; set; }

			[ProtoMember(12)]
			public int MaxPvPBirdDefeatsCounted { get; set; }

			[ProtoMember(13)]
			public int ScoreForPvPBirdDefeated { get; set; }

			[ProtoMember(14)]
			public int ScorePerStarNeededPvP { get; set; }

			[ProtoMember(15)]
			public int MaxScoreForBannerSurvive { get; set; }

			[ProtoMember(16)]
			public int PowerLevelFactorForDamage { get; set; }

			[ProtoMember(17)]
			public int PowerLevelFactorPerSetItemBird { get; set; }

			[ProtoMember(18)]
			public int PowerLevelFactorForCompleteSetBird { get; set; }

			[ProtoMember(19)]
			public int PowerLevelFactorPerSetItemBanner { get; set; }

			[ProtoMember(20)]
			public int PowerLevelFactorForCompleteSetBanner { get; set; }

			[ProtoMember(21)]
			public int PowerLevelDivideEndValue { get; set; }

			[ProtoMember(22)]
			public int PigPowerLevelDivideValue { get; set; }
		}

		[ProtoContract]
		public class ShopBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public int Slots { get; set; }

			[ProtoMember(3)]
			public List<string> Categories { get; set; }

			[ProtoMember(4)]
			public string AssetId { get; set; }

			[ProtoMember(5)]
			public string LocaId { get; set; }

			[ProtoMember(6)]
			public string AtlasId { get; set; }
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

		[ProtoContract]
		public class SkillBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string AssetId { get; set; }

			[ProtoMember(3)]
			public string LocaId { get; set; }

			[ProtoMember(4)]
			public Dictionary<string, float> SkillParameters { get; set; }

			[ProtoMember(5)]
			public string SkillTemplateType { get; set; }

			[ProtoMember(6)]
			public int EffectDuration { get; set; }

			[ProtoMember(7)]
			public SkillTargetTypes TargetType { get; set; }

			[ProtoMember(8)]
			public SkillEffectTypes EffectType { get; set; }

			[ProtoMember(9)]
			public PigTargetingBehavior TargetingBehavior { get; set; }

			[ProtoMember(10)]
			public bool TargetAlreadyAffectedTargets { get; set; }

			[ProtoMember(11)]
			public bool TargetSelfPossible { get; set; }

			[ProtoMember(12)]
			public string IconAssetId { get; set; }

			[ProtoMember(13)]
			public string IconAtlasId { get; set; }

			[ProtoMember(14)]
			public string EffectIconAtlasId { get; set; }

			[ProtoMember(15)]
			public string EffectIconAssetId { get; set; }

			[ProtoMember(16)]
			public List<string> TargetCulling { get; set; }
		}

		[ProtoContract]
		public class SocialEnvironmentBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public uint TimeForGetFriendBird { get; set; }

			[ProtoMember(3)]
			public uint TimeForFreeGachaRollSpawn { get; set; }

			[ProtoMember(4)]
			public int HighMaxFreeRolls { get; set; }

			[ProtoMember(5)]
			public int LowMaxFreeRolls { get; set; }

			[ProtoMember(6)]
			public int MinFriendsForMaxFreeRolls { get; set; }

			[ProtoMember(7)]
			public Requirement MightyEagleBirdReqirement { get; set; }

			[ProtoMember(8)]
			public int MaxFriendsInHighscoreList { get; set; }

			[ProtoMember(9)]
			public uint FriendshipGateHelpCooldown { get; set; }

			[ProtoMember(10)]
			public uint FriendshipEssenceCooldown { get; set; }

			[ProtoMember(11)]
			public uint CacheFriendBirdTime { get; set; }

			[ProtoMember(12)]
			public int FriendShipEssenceMessageCap { get; set; }

			[ProtoMember(13)]
			public Dictionary<string, int> FacebookDailyBonus { get; set; }

			[ProtoMember(14)]
			public Requirement SkipCooldownRequirement { get; set; }

			[ProtoMember(15)]
			public int McCoolVisitMinCooldown { get; set; }

			[ProtoMember(16)]
			public int McCoolVisitMaxCooldown { get; set; }

			[ProtoMember(17)]
			public int MaxRewardsForPvp { get; set; }

			[ProtoMember(18)]
			public int PvPFallbackChanceHard { get; set; }

			[ProtoMember(19)]
			public int PvPFallbackChanceMedium { get; set; }

			[ProtoMember(20)]
			public Dictionary<string, int> PvPFallbackEasy { get; set; }

			[ProtoMember(21)]
			public Dictionary<string, int> PvPFallbackMedium { get; set; }

			[ProtoMember(22)]
			public Dictionary<string, int> PvPFallbackHard { get; set; }
		}

		[ProtoContract]
		public class ThirdPartyIdBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string PaymentProductId { get; set; }

			[ProtoMember(3)]
			public string GamecenterAchievementId { get; set; }

			[ProtoMember(4)]
			public string ChimeraGooglePlayAchievementId { get; set; }

			[ProtoMember(5)]
			public string RovioGooglePlayAchievementId { get; set; }

			[ProtoMember(6)]
			public int XBoxLiveAchievementId { get; set; }
		}

		[ProtoContract]
		public class LevelRangeValueTable
		{
			[ProtoMember(1)]
			public int FromLevel { get; set; }

			[ProtoMember(2)]
			public int ToLevel { get; set; }

			[ProtoMember(3)]
			public int Value { get; set; }
		}

		[ProtoContract]
		public class WorldBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string FirstHotspotNameId { get; set; }

			[ProtoMember(3)]
			public int MaxPigsInBattle { get; set; }

			[ProtoMember(4)]
			public float RageMeterIncreasePerHitInPercent { get; set; }

			[ProtoMember(5)]
			public float RageMeterIncreasePerHiAfterFirstAOEInPercent { get; set; }

			[ProtoMember(6)]
			public float RageMeterIncreasePerPassiveEffectInPercent { get; set; }

			[ProtoMember(7)]
			public float RageMeterFullOnTotalHealthLostFactor { get; set; }

			[ProtoMember(8)]
			public float CostToScrapLootRateCrafting { get; set; }

			[ProtoMember(9)]
			public float CostToScrapLootRateGacha { get; set; }

			[ProtoMember(10)]
			public float TimeForResourceRespawn { get; set; }

			[ProtoMember(11)]
			public float ReferenceAttackValueBase { get; set; }

			[ProtoMember(12)]
			public float ReferenceAttackValuePerLevelInPercent { get; set; }

			[ProtoMember(13)]
			public float ReferenceHealthValueBase { get; set; }

			[ProtoMember(14)]
			public float ReferenceHealthValuePerLevelInPercent { get; set; }

			[ProtoMember(15)]
			public float ShopFullRefreshCycleTimeInSec { get; set; }

			[ProtoMember(16)]
			public Requirement RerollCraftingReqirement { get; set; }

			[ProtoMember(17)]
			public int MaxBirdsInBattle { get; set; }

			[ProtoMember(18)]
			public int LevelSubstractionForMissingBirdOnDifficultyCalculation { get; set; }

			[ProtoMember(19)]
			public int MaximumLevelDifferenceForDifficultyCalculation { get; set; }

			[ProtoMember(20)]
			public int BirdLevelWeightForDifficultyCalculation { get; set; }

			[ProtoMember(21)]
			public uint TimeGoldenPigSpawn { get; set; }

			[ProtoMember(22)]
			public uint TimeGoldenPigOnlyClientIfFailedRespawn { get; set; }

			[ProtoMember(23)]
			public uint TimeForGetFriendBird { get; set; }

			[ProtoMember(24)]
			public Requirement MightyEagleBirdReqirement { get; set; }

			[ProtoMember(25)]
			public Requirement ReviveBirdsRequirement { get; set; }

			[ProtoMember(26)]
			public uint TimeForNextClassUpgrade { get; set; }

			[ProtoMember(27)]
			public Requirement NextClassSkipRequirement { get; set; }

			[ProtoMember(28)]
			public List<float> StandardDiceWeights { get; set; }

			[ProtoMember(29)]
			public List<float> GoldDiceWeights { get; set; }

			[ProtoMember(30)]
			public List<float> CrystalDiceWeights { get; set; }

			[ProtoMember(31)]
			public uint TipOfTheDayCount { get; set; }

			[ProtoMember(32)]
			public int ResourceSpawnAmountPerNode { get; set; }

			[ProtoMember(33)]
			public int MaximumSpawnableNodes { get; set; }

			[ProtoMember(34)]
			public Requirement DungeonSkipRequirement { get; set; }

			[ProtoMember(35)]
			public int GachaUsesFromHighOffer { get; set; }

			[ProtoMember(36)]
			public string DefaultGlobalDifficultyBalancing { get; set; }

			[ProtoMember(37)]
			public Dictionary<int, int> AdCooldownBalancing { get; set; }

			[ProtoMember(38)]
			public Dictionary<string, uint> SponsoredAdCooldownBalancing { get; set; }

			[ProtoMember(39)]
			public string SponsoredAdPotionType { get; set; }

			[ProtoMember(40)]
			public string SponsoredAdBuffName { get; set; }

			[ProtoMember(41)]
			public string DailyHotspotNameId { get; set; }

			[ProtoMember(42)]
			public int DailyChainLength { get; set; }

			[ProtoMember(43)]
			public bool IsLimeGreen { get; set; }

			[ProtoMember(44)]
			public int LimeGreenValue { get; set; }

			[ProtoMember(45)]
			public int RestedBonusTime { get; set; }

			[ProtoMember(46)]
			public bool RestedBonusActive { get; set; }

			[ProtoMember(47)]
			public uint RainbowRiotTime { get; set; }

			[ProtoMember(48)]
			public int GachaPreviewAmount { get; set; }

			[ProtoMember(49)]
			public float GachaPreviewPercentStandard { get; set; }

			[ProtoMember(50)]
			public float GachaPreviewPercentRiot { get; set; }

			[ProtoMember(51)]
			public float GachaPreviewPercentSet { get; set; }

			[ProtoMember(52)]
			public List<LevelRangeValueTable> LevelRubberBandTables { get; set; }

			[ProtoMember(53)]
			public List<int> DailyChainAdditionalBonusPerDay { get; set; }

			[ProtoMember(54)]
			public string DailyChainHeaderLocaId { get; set; }

			[ProtoMember(55)]
			public uint DailyChainTimerUntilTimestamp { get; set; }

			[ProtoMember(56)]
			public int GlobalEventDiscount { get; set; }

			[ProtoMember(57)]
			public List<int> MasteryFromExperienceMultiplier { get; set; }

			[ProtoMember(58)]
			public List<int> ClassUpgradeToMasteryMapping { get; set; }

			[ProtoMember(59)]
			public float AllBirdsMasteryChance { get; set; }

			[ProtoMember(60)]
			public float SingleBirdMasteryChance { get; set; }

			[ProtoMember(61)]
			public Dictionary<string, int> ItemMaxCaps { get; set; }

			[ProtoMember(62)]
			public float EnergyRefreshTimeInSeconds { get; set; }

			[ProtoMember(63)]
			public float MasteryChancePlus { get; set; }

			[ProtoMember(64)]
			public float MasteryChanceBonusCap { get; set; }

			[ProtoMember(65)]
			public List<float> DojoOfferDiscount { get; set; }

			[ProtoMember(66)]
			public List<int> DojoOfferDiscountThresholds { get; set; }

			[ProtoMember(67)]
			public Dictionary<string, int> ChronicleCaveDailyTreasureLoot { get; set; }

			[ProtoMember(68)]
			public uint TimeChronicleCaveTreasureSpawn { get; set; }

			[ProtoMember(69)]
			public Dictionary<string, int> DailyEventAdLoot { get; set; }

			[ProtoMember(70)]
			public uint TimeDailyEventPopupSpawn { get; set; }

			[ProtoMember(71)]
			public int PvpGachaUsesFromHighOffer { get; set; }

			[ProtoMember(72)]
			public float CostToScrapLootRateSet { get; set; }

			[ProtoMember(73)]
			public Requirement ReviveSingleBirdsRequirement { get; set; }

			[ProtoMember(74)]
			public int MultiCraftAmount { get; set; }

			[ProtoMember(75)]
			public Requirement RerollMultiCraftingReqirement { get; set; }

			[ProtoMember(76)]
			public float CoinFlipChanceChange { get; set; }

			[ProtoMember(77)]
			public float CoinFlipChanceMaxChange { get; set; }

			[ProtoMember(78)]
			public bool UseGoldenPigCloudBattle { get; set; }

			[ProtoMember(79)]
			public int TimeGoldenPigRespawnRandomOffset { get; set; }

			[ProtoMember(80)]
			public int TimeGoldenPigMoveOn { get; set; }

			[ProtoMember(81)]
			public int MultiGachaAmount { get; set; }

			[ProtoMember(82)]
			public int GachaUsesFromNormalOffer { get; set; }

			[ProtoMember(83)]
			public int PvpGachaUsesFromNormalOffer { get; set; }

			[ProtoMember(84)]
			public int RainbowRiot1Multi { get; set; }

			[ProtoMember(85)]
			public int RainbowRiot2Multi { get; set; }

			[ProtoMember(86)]
			public int GachaVideoTimespan { get; set; }

			[ProtoMember(87)]
			public float GoldenAnvilBonus { get; set; }

			[ProtoMember(88)]
			public float DiamondAnvilBonus { get; set; }

			[ProtoMember(89)]
			public bool EnableCrossPromoButton { get; set; }

			[ProtoMember(90)]
			public float ChanceToDisplayStaminaVideo { get; set; }

			[ProtoMember(91)]
			public int RateAppAbortCooldown { get; set; }

			[ProtoMember(92)]
			public int HPChunksLowest { get; set; }

			[ProtoMember(93)]
			public int HPChunksHighest { get; set; }

			[ProtoMember(94)]
			public int MaxHPChunks { get; set; }

			[ProtoMember(95)]
			public float HPChunkSteps { get; set; }

			[ProtoMember(96)]
			public List<int> NotificationPopupCooldowns { get; set; }

			[ProtoMember(97)]
			public bool EnableFriendLeaderboards { get; set; }

			[ProtoMember(98)]
			public float BonusPercentByBossRewardVideo { get; set; }

			[ProtoMember(99)]
			public int PvpMaxPowerlevelDiff { get; set; }

			[ProtoMember(100)]
			public bool OneWorldBoss { get; set; }

			[ProtoMember(101)]
			public Requirement RerollPvpObjectivesRequirement { get; set; }

			[ProtoMember(102)]
			public int ObjectivesVideoTimespan { get; set; }

			[ProtoMember(103)]
			public Requirement RerollChestRequirement { get; set; }

			[ProtoMember(104)]
			public int MaxPreviewPigsInBps { get; set; }
		}

		public enum RelativeLevelType
		{
			ExactLevel,
			RelativeToPlayer,
			PlayerLevelStep
		}

		[ProtoContract]
		public class ShopOfferBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public Dictionary<string, int> OfferContents { get; set; }

			[ProtoMember(3)]
			public List<Requirement> BuyRequirements { get; set; }

			[ProtoMember(4)]
			public List<Requirement> ShowRequirements { get; set; }

			[ProtoMember(5)]
			public int SortPriority { get; set; }

			[ProtoMember(6)]
			public int Level { get; set; }

			[ProtoMember(7)]
			public string Category { get; set; }

			[ProtoMember(8)]
			public string AssetId { get; set; }

			[ProtoMember(9)]
			public int SlotId { get; set; }

			[ProtoMember(10)]
			public string LocaId { get; set; }

			[ProtoMember(11)]
			public RelativeLevelType LevelType { get; set; }

			[ProtoMember(12)]
			public List<Requirement> BuyRequirementsScaling { get; set; }

			[ProtoMember(13)]
			public List<Requirement> ShowRequirementsScaling { get; set; }

			[ProtoMember(14)]
			public bool ManagedExternal { get; set; }

			[ProtoMember(15)]
			public int LevelAdditional { get; set; }

			[ProtoMember(16)]
			public int DiscountPrice { get; set; }

			[ProtoMember(17)]
			public List<Requirement> DiscountRequirements { get; set; }

			[ProtoMember(18)]
			public int DiscountCooldown { get; set; }

			[ProtoMember(19)]
			public int DiscountDuration { get; set; }

			[ProtoMember(20)]
			public bool UniqueOffer { get; set; }
		}

		[ProtoContract]
		public class GameConstantsBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public string StringValue { get; set; }

			[ProtoMember(3)]
			public float FloatValue { get; set; }

			[ProtoMember(4)]
			public Requirement RequirementValue { get; set; }

			[ProtoMember(5)]
			public List<float> FloatlistValue { get; set; }

			[ProtoMember(6)]
			public bool BoolValue { get; set; }
		}

		[ProtoContract]
		public class SetFusionBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public float ChanceWith3Different { get; set; }

			[ProtoMember(3)]
			public float ChanceWith2SameOn2 { get; set; }

			[ProtoMember(4)]
			public float ChanceWith2SameOn1 { get; set; }

			[ProtoMember(5)]
			public float ChanceWith3Same { get; set; }

			[ProtoMember(6)]
			public float BannerChanceWith3Same { get; set; }

			[ProtoMember(7)]
			public float BannerChanceWith2SameOn2 { get; set; }

			[ProtoMember(8)]
			public List<Requirement> BuyRequirements { get; set; }

			[ProtoMember(9)]
			public List<Requirement> RerollcostBase { get; set; }

			[ProtoMember(10)]
			public float RerollcostIncrease { get; set; }

			[ProtoMember(11)]
			public float RerollcostMax { get; set; }

			[ProtoMember(12)]
			public float AncientChance { get; set; }

			[ProtoMember(13)]
			public float AncientChanceRerollIncrease { get; set; }

			[ProtoMember(14)]
			public float AncientChanceRerollMax { get; set; }

			[ProtoMember(15)]
			public int AncientItemEnchLevel { get; set; }

			[ProtoMember(16)]
			public float IncreaseAncientChancePerAncientItem { get; set; }

			[ProtoMember(17)]
			public List<Requirement> BannerFusionBuyRequirements { get; set; }

			[ProtoMember(18)]
			public List<Requirement> RerollBannerCostBase { get; set; }

			[ProtoMember(19)]
			public float RerollBannerCostIncrease { get; set; }

			[ProtoMember(20)]
			public float RerollBannerCostMax { get; set; }
		}

		[ProtoContract]
		public class SplashScreenBalancingData
		{
			[ProtoMember(1)]
			public string NameId { get; set; }

			[ProtoMember(2)]
			public uint StartTimestamp { get; set; }

			[ProtoMember(3)]
			public uint EndTimestamp { get; set; }
		}

		[ProtoContract]
		public class SerializedBalancingDataContainer
		{
			[ProtoMember(1)]
			public Dictionary<string, byte[]> AllBalancingData { get; set; }

			[ProtoMember(2)]
			public string Version { get; set; }
		}

		public sealed class Adler
		{
			public static uint Adler32(uint adler, byte[] buf, int index, int len)
			{
				uint result;
				if (buf == null)
				{
					result = 1U;
				}
				else
				{
					uint num = adler & 65535U;
					uint num2 = (adler >> 16) & 65535U;
					while (len > 0)
					{
						int i = ((len < Adler.NMAX) ? len : Adler.NMAX);
						len -= i;
						while (i >= 16)
						{
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							num += (uint)buf[index++];
							num2 += num;
							i -= 16;
						}
						if (i != 0)
						{
							do
							{
								num += (uint)buf[index++];
								num2 += num;
							}
							while (--i != 0);
						}
						num %= Adler.BASE;
						num2 %= Adler.BASE;
					}
					result = (num2 << 16) | num;
				}
				return result;
			}

			private static readonly uint BASE = 65521U;

			private static readonly int NMAX = 5552;
		}

		internal enum BlockState
		{
			NeedMore,
			BlockDone,
			FinishStarted,
			FinishDone
		}

		public enum CompressionLevel
		{
			None,
			Level0 = 0,
			BestSpeed,
			Level1 = 1,
			Level2,
			Level3,
			Level4,
			Level5,
			Default,
			Level6 = 6,
			Level7,
			Level8,
			BestCompression,
			Level9 = 9
		}

		public enum CompressionMode
		{
			Compress,
			Decompress
		}

		public enum CompressionStrategy
		{
			Default,
			Filtered,
			HuffmanOnly
		}

		[ComVisible(true)]
		[Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
		/// [ClassInterface(ClassInterfaceType.AutoDispatch)]
		public class CRC32
		{
			public CRC32()
				: this(false)
			{
			}

			public CRC32(bool reverseBits)
				: this(-306674912, reverseBits)
			{
			}

			public CRC32(int polynomial, bool reverseBits)
			{
				this.reverseBits = reverseBits;
				this.dwPolynomial = (uint)polynomial;
				this.GenerateLookupTable();
			}

			public long TotalBytesRead
			{
				get
				{
					return this._TotalBytesRead;
				}
			}

			public int Crc32Result
			{
				get
				{
					return (int)(~(int)this._register);
				}
			}

			public void SlurpBlock(byte[] block, int offset, int count)
			{
				if (block == null)
				{
					throw new Exception("The data buffer must not be null.");
				}
				for (int i = 0; i < count; i++)
				{
					int num = offset + i;
					byte b = block[num];
					if (this.reverseBits)
					{
						uint num2 = (this._register >> 24) ^ (uint)b;
						this._register = (this._register << 8) ^ this.crc32Table[(int)((UIntPtr)num2)];
					}
					else
					{
						uint num2 = (this._register & 255U) ^ (uint)b;
						this._register = (this._register >> 8) ^ this.crc32Table[(int)((UIntPtr)num2)];
					}
				}
				this._TotalBytesRead += (long)count;
			}

			private static uint ReverseBits(uint data)
			{
				uint num = ((data & 1431655765U) << 1) | ((data >> 1) & 1431655765U);
				num = ((num & 858993459U) << 2) | ((num >> 2) & 858993459U);
				num = ((num & 252645135U) << 4) | ((num >> 4) & 252645135U);
				return (num << 24) | ((num & 65280U) << 8) | ((num >> 8) & 65280U) | (num >> 24);
			}

			private static byte ReverseBits(byte data)
			{
				uint num = (uint)data * 131586U;
				uint num2 = 17055760U;
				uint num3 = num & num2;
				uint num4 = (num << 2) & (num2 << 1);
				return (byte)(16781313U * (num3 + num4) >> 24);
			}

			private void GenerateLookupTable()
			{
				this.crc32Table = new uint[256];
				byte b = 0;
				do
				{
					uint num = (uint)b;
					for (byte b2 = 8; b2 > 0; b2 -= 1)
					{
						if ((num & 1U) == 1U)
						{
							num = (num >> 1) ^ this.dwPolynomial;
						}
						else
						{
							num >>= 1;
						}
					}
					if (this.reverseBits)
					{
						this.crc32Table[(int)CRC32.ReverseBits(b)] = CRC32.ReverseBits(num);
					}
					else
					{
						this.crc32Table[(int)b] = num;
					}
					b += 1;
				}
				while (b != 0);
			}

			private const int BUFFER_SIZE = 8192;

			private uint dwPolynomial;

			private long _TotalBytesRead;

			private bool reverseBits;

			private uint[] crc32Table;

			private uint _register = uint.MaxValue;
		}

		internal enum DeflateFlavor
		{
			Store,
			Fast,
			Slow
		}

		internal sealed class DeflateManager
		{
			internal DeflateManager()
			{
				this.dyn_ltree = new short[DeflateManager.HEAP_SIZE * 2];
				this.dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2];
				this.bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2];
			}

			private void _InitializeLazyMatch()
			{
				this.window_size = 2 * this.w_size;
				Array.Clear(this.head, 0, this.hash_size);
				this.config = DeflateManager.Config.Lookup(this.compressionLevel);
				this.SetDeflater();
				this.strstart = 0;
				this.block_start = 0;
				this.lookahead = 0;
				this.match_length = (this.prev_length = DeflateManager.MIN_MATCH - 1);
				this.match_available = 0;
				this.ins_h = 0;
			}

			private void _InitializeTreeData()
			{
				this.treeLiterals.dyn_tree = this.dyn_ltree;
				this.treeLiterals.staticTree = StaticTree.Literals;
				this.treeDistances.dyn_tree = this.dyn_dtree;
				this.treeDistances.staticTree = StaticTree.Distances;
				this.treeBitLengths.dyn_tree = this.bl_tree;
				this.treeBitLengths.staticTree = StaticTree.BitLengths;
				this.bi_buf = 0;
				this.bi_valid = 0;
				this.last_eob_len = 8;
				this._InitializeBlocks();
			}

			internal void _InitializeBlocks()
			{
				for (int i = 0; i < InternalConstants.L_CODES; i++)
				{
					this.dyn_ltree[i * 2] = 0;
				}
				for (int i = 0; i < InternalConstants.D_CODES; i++)
				{
					this.dyn_dtree[i * 2] = 0;
				}
				for (int i = 0; i < InternalConstants.BL_CODES; i++)
				{
					this.bl_tree[i * 2] = 0;
				}
				this.dyn_ltree[DeflateManager.END_BLOCK * 2] = 1;
				this.opt_len = (this.static_len = 0);
				this.last_lit = (this.matches = 0);
			}

			internal void pqdownheap(short[] tree, int k)
			{
				int num = this.heap[k];
				for (int i = k << 1; i <= this.heap_len; i <<= 1)
				{
					if (i < this.heap_len && DeflateManager._IsSmaller(tree, this.heap[i + 1], this.heap[i], this.depth))
					{
						i++;
					}
					if (DeflateManager._IsSmaller(tree, num, this.heap[i], this.depth))
					{
						break;
					}
					this.heap[k] = this.heap[i];
					k = i;
				}
				this.heap[k] = num;
			}

			internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
			{
				short num = tree[n * 2];
				short num2 = tree[m * 2];
				return num < num2 || (num == num2 && depth[n] <= depth[m]);
			}

			internal void scan_tree(short[] tree, int max_code)
			{
				int num = -1;
				int num2 = (int)tree[1];
				int num3 = 0;
				int num4 = 7;
				int num5 = 4;
				if (num2 == 0)
				{
					num4 = 138;
					num5 = 3;
				}
				tree[(max_code + 1) * 2 + 1] = short.MaxValue;
				for (int i = 0; i <= max_code; i++)
				{
					int num6 = num2;
					num2 = (int)tree[(i + 1) * 2 + 1];
					if (++num3 >= num4 || num6 != num2)
					{
						if (num3 < num5)
						{
							this.bl_tree[num6 * 2] = (short)((int)this.bl_tree[num6 * 2] + num3);
						}
						else if (num6 != 0)
						{
							if (num6 != num)
							{
								short[] array = this.bl_tree;
								int num7 = num6 * 2;
								array[num7] += 1;
							}
							short[] array2 = this.bl_tree;
							int num8 = InternalConstants.REP_3_6 * 2;
							array2[num8] += 1;
						}
						else if (num3 <= 10)
						{
							short[] array3 = this.bl_tree;
							int num9 = InternalConstants.REPZ_3_10 * 2;
							array3[num9] += 1;
						}
						else
						{
							short[] array4 = this.bl_tree;
							int num10 = InternalConstants.REPZ_11_138 * 2;
							array4[num10] += 1;
						}
						num3 = 0;
						num = num6;
						if (num2 == 0)
						{
							num4 = 138;
							num5 = 3;
						}
						else if (num6 == num2)
						{
							num4 = 6;
							num5 = 3;
						}
						else
						{
							num4 = 7;
							num5 = 4;
						}
					}
				}
			}

			internal int build_bl_tree()
			{
				this.scan_tree(this.dyn_ltree, this.treeLiterals.max_code);
				this.scan_tree(this.dyn_dtree, this.treeDistances.max_code);
				this.treeBitLengths.build_tree(this);
				int i;
				for (i = InternalConstants.BL_CODES - 1; i >= 3; i--)
				{
					if (this.bl_tree[(int)(Tree.bl_order[i] * 2 + 1)] != 0)
					{
						break;
					}
				}
				this.opt_len += 3 * (i + 1) + 5 + 5 + 4;
				return i;
			}

			internal void send_all_trees(int lcodes, int dcodes, int blcodes)
			{
				this.send_bits(lcodes - 257, 5);
				this.send_bits(dcodes - 1, 5);
				this.send_bits(blcodes - 4, 4);
				for (int i = 0; i < blcodes; i++)
				{
					this.send_bits((int)this.bl_tree[(int)(Tree.bl_order[i] * 2 + 1)], 3);
				}
				this.send_tree(this.dyn_ltree, lcodes - 1);
				this.send_tree(this.dyn_dtree, dcodes - 1);
			}

			internal void send_tree(short[] tree, int max_code)
			{
				int num = -1;
				int num2 = (int)tree[1];
				int num3 = 0;
				int num4 = 7;
				int num5 = 4;
				if (num2 == 0)
				{
					num4 = 138;
					num5 = 3;
				}
				for (int i = 0; i <= max_code; i++)
				{
					int num6 = num2;
					num2 = (int)tree[(i + 1) * 2 + 1];
					if (++num3 >= num4 || num6 != num2)
					{
						if (num3 < num5)
						{
							do
							{
								this.send_code(num6, this.bl_tree);
							}
							while (--num3 != 0);
						}
						else if (num6 != 0)
						{
							if (num6 != num)
							{
								this.send_code(num6, this.bl_tree);
								num3--;
							}
							this.send_code(InternalConstants.REP_3_6, this.bl_tree);
							this.send_bits(num3 - 3, 2);
						}
						else if (num3 <= 10)
						{
							this.send_code(InternalConstants.REPZ_3_10, this.bl_tree);
							this.send_bits(num3 - 3, 3);
						}
						else
						{
							this.send_code(InternalConstants.REPZ_11_138, this.bl_tree);
							this.send_bits(num3 - 11, 7);
						}
						num3 = 0;
						num = num6;
						if (num2 == 0)
						{
							num4 = 138;
							num5 = 3;
						}
						else if (num6 == num2)
						{
							num4 = 6;
							num5 = 3;
						}
						else
						{
							num4 = 7;
							num5 = 4;
						}
					}
				}
			}

			private void put_bytes(byte[] p, int start, int len)
			{
				Array.Copy(p, start, this.pending, this.pendingCount, len);
				this.pendingCount += len;
			}

			internal void send_code(int c, short[] tree)
			{
				int num = c * 2;
				this.send_bits((int)tree[num] & 65535, (int)tree[num + 1] & 65535);
			}

			internal void send_bits(int value, int length)
			{
				if (this.bi_valid > DeflateManager.Buf_size - length)
				{
					this.bi_buf |= (short)((value << this.bi_valid) & 65535);
					this.pending[this.pendingCount++] = (byte)this.bi_buf;
					this.pending[this.pendingCount++] = (byte)(this.bi_buf >> 8);
					this.bi_buf = (short)((uint)value >> DeflateManager.Buf_size - this.bi_valid);
					this.bi_valid += length - DeflateManager.Buf_size;
				}
				else
				{
					this.bi_buf |= (short)((value << this.bi_valid) & 65535);
					this.bi_valid += length;
				}
			}

			internal void _tr_align()
			{
				this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
				this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
				this.bi_flush();
				if (1 + this.last_eob_len + 10 - this.bi_valid < 9)
				{
					this.send_bits(DeflateManager.STATIC_TREES << 1, 3);
					this.send_code(DeflateManager.END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
					this.bi_flush();
				}
				this.last_eob_len = 7;
			}

			internal bool _tr_tally(int dist, int lc)
			{
				this.pending[this._distanceOffset + this.last_lit * 2] = (byte)((uint)dist >> 8);
				this.pending[this._distanceOffset + this.last_lit * 2 + 1] = (byte)dist;
				this.pending[this._lengthOffset + this.last_lit] = (byte)lc;
				this.last_lit++;
				if (dist == 0)
				{
					short[] array = this.dyn_ltree;
					int num = lc * 2;
					array[num] += 1;
				}
				else
				{
					this.matches++;
					dist--;
					short[] array2 = this.dyn_ltree;
					int num2 = ((int)Tree.LengthCode[lc] + InternalConstants.LITERALS + 1) * 2;
					array2[num2] += 1;
					short[] array3 = this.dyn_dtree;
					int num3 = Tree.DistanceCode(dist) * 2;
					array3[num3] += 1;
				}
				if ((this.last_lit & 8191) == 0 && this.compressionLevel > CompressionLevel.Level2)
				{
					int num4 = this.last_lit << 3;
					int num5 = this.strstart - this.block_start;
					for (int i = 0; i < InternalConstants.D_CODES; i++)
					{
						num4 = (int)((long)num4 + (long)this.dyn_dtree[i * 2] * (5L + (long)Tree.ExtraDistanceBits[i]));
					}
					num4 >>= 3;
					if (this.matches < this.last_lit / 2 && num4 < num5 / 2)
					{
						return true;
					}
				}
				return this.last_lit == this.lit_bufsize - 1 || this.last_lit == this.lit_bufsize;
			}

			internal void send_compressed_block(short[] ltree, short[] dtree)
			{
				int num = 0;
				if (this.last_lit != 0)
				{
					do
					{
						int num2 = this._distanceOffset + num * 2;
						int num3 = (((int)this.pending[num2] << 8) & 65280) | (int)(this.pending[num2 + 1] & byte.MaxValue);
						int num4 = (int)(this.pending[this._lengthOffset + num] & byte.MaxValue);
						num++;
						if (num3 == 0)
						{
							this.send_code(num4, ltree);
						}
						else
						{
							int num5 = (int)Tree.LengthCode[num4];
							this.send_code(num5 + InternalConstants.LITERALS + 1, ltree);
							int num6 = Tree.ExtraLengthBits[num5];
							if (num6 != 0)
							{
								num4 -= Tree.LengthBase[num5];
								this.send_bits(num4, num6);
							}
							num3--;
							num5 = Tree.DistanceCode(num3);
							this.send_code(num5, dtree);
							num6 = Tree.ExtraDistanceBits[num5];
							if (num6 != 0)
							{
								num3 -= Tree.DistanceBase[num5];
								this.send_bits(num3, num6);
							}
						}
					}
					while (num < this.last_lit);
				}
				this.send_code(DeflateManager.END_BLOCK, ltree);
				this.last_eob_len = (int)ltree[DeflateManager.END_BLOCK * 2 + 1];
			}

			internal void set_data_type()
			{
				int i = 0;
				int num = 0;
				int num2 = 0;
				while (i < 7)
				{
					num2 += (int)this.dyn_ltree[i * 2];
					i++;
				}
				while (i < 128)
				{
					num += (int)this.dyn_ltree[i * 2];
					i++;
				}
				while (i < InternalConstants.LITERALS)
				{
					num2 += (int)this.dyn_ltree[i * 2];
					i++;
				}
				this.data_type = (sbyte)((num2 > num >> 2) ? DeflateManager.Z_BINARY : DeflateManager.Z_ASCII);
			}

			internal void bi_flush()
			{
				if (this.bi_valid == 16)
				{
					this.pending[this.pendingCount++] = (byte)this.bi_buf;
					this.pending[this.pendingCount++] = (byte)(this.bi_buf >> 8);
					this.bi_buf = 0;
					this.bi_valid = 0;
				}
				else if (this.bi_valid >= 8)
				{
					this.pending[this.pendingCount++] = (byte)this.bi_buf;
					this.bi_buf = (short)(this.bi_buf >> 8);
					this.bi_valid -= 8;
				}
			}

			internal void bi_windup()
			{
				if (this.bi_valid > 8)
				{
					this.pending[this.pendingCount++] = (byte)this.bi_buf;
					this.pending[this.pendingCount++] = (byte)(this.bi_buf >> 8);
				}
				else if (this.bi_valid > 0)
				{
					this.pending[this.pendingCount++] = (byte)this.bi_buf;
				}
				this.bi_buf = 0;
				this.bi_valid = 0;
			}

			internal void copy_block(int buf, int len, bool header)
			{
				this.bi_windup();
				this.last_eob_len = 8;
				if (header)
				{
					this.pending[this.pendingCount++] = (byte)len;
					this.pending[this.pendingCount++] = (byte)(len >> 8);
					this.pending[this.pendingCount++] = (byte)(~(byte)len);
					this.pending[this.pendingCount++] = (byte)(~len >> 8);
				}
				this.put_bytes(this.window, buf, len);
			}

			internal void flush_block_only(bool eof)
			{
				this._tr_flush_block((this.block_start >= 0) ? this.block_start : (-1), this.strstart - this.block_start, eof);
				this.block_start = this.strstart;
				this._codec.flush_pending();
			}

			internal BlockState DeflateNone(FlushType flush)
			{
				int num = 65535;
				if (num > this.pending.Length - 5)
				{
					num = this.pending.Length - 5;
				}
				for (; ; )
				{
					if (this.lookahead <= 1)
					{
						this._fillWindow();
						if (this.lookahead == 0 && flush == FlushType.None)
						{
							break;
						}
						if (this.lookahead == 0)
						{
							goto Block_5;
						}
					}
					this.strstart += this.lookahead;
					this.lookahead = 0;
					int num2 = this.block_start + num;
					if (this.strstart == 0 || this.strstart >= num2)
					{
						this.lookahead = this.strstart - num2;
						this.strstart = num2;
						this.flush_block_only(false);
						if (this._codec.AvailableBytesOut == 0)
						{
							goto Block_8;
						}
					}
					if (this.strstart - this.block_start >= this.w_size - DeflateManager.MIN_LOOKAHEAD)
					{
						this.flush_block_only(false);
						if (this._codec.AvailableBytesOut == 0)
						{
							goto Block_10;
						}
					}
				}
				return BlockState.NeedMore;
			Block_5:
				this.flush_block_only(flush == FlushType.Finish);
				if (this._codec.AvailableBytesOut == 0)
				{
					return (flush == FlushType.Finish) ? BlockState.FinishStarted : BlockState.NeedMore;
				}
				return (flush == FlushType.Finish) ? BlockState.FinishDone : BlockState.BlockDone;
			Block_8:
				return BlockState.NeedMore;
			Block_10:
				return BlockState.NeedMore;
			}

			internal void _tr_stored_block(int buf, int stored_len, bool eof)
			{
				this.send_bits((DeflateManager.STORED_BLOCK << 1) + (eof ? 1 : 0), 3);
				this.copy_block(buf, stored_len, true);
			}

			internal void _tr_flush_block(int buf, int stored_len, bool eof)
			{
				int num = 0;
				int num2;
				int num3;
				if (this.compressionLevel > CompressionLevel.None)
				{
					if ((int)this.data_type == DeflateManager.Z_UNKNOWN)
					{
						this.set_data_type();
					}
					this.treeLiterals.build_tree(this);
					this.treeDistances.build_tree(this);
					num = this.build_bl_tree();
					num2 = this.opt_len + 3 + 7 >> 3;
					num3 = this.static_len + 3 + 7 >> 3;
					if (num3 <= num2)
					{
						num2 = num3;
					}
				}
				else
				{
					num3 = (num2 = stored_len + 5);
				}
				if (stored_len + 4 <= num2 && buf != -1)
				{
					this._tr_stored_block(buf, stored_len, eof);
				}
				else if (num3 == num2)
				{
					this.send_bits((DeflateManager.STATIC_TREES << 1) + (eof ? 1 : 0), 3);
					this.send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
				}
				else
				{
					this.send_bits((DeflateManager.DYN_TREES << 1) + (eof ? 1 : 0), 3);
					this.send_all_trees(this.treeLiterals.max_code + 1, this.treeDistances.max_code + 1, num + 1);
					this.send_compressed_block(this.dyn_ltree, this.dyn_dtree);
				}
				this._InitializeBlocks();
				if (eof)
				{
					this.bi_windup();
				}
			}

			private void _fillWindow()
			{
				do
				{
					int num = this.window_size - this.lookahead - this.strstart;
					int num2;
					if (num == 0 && this.strstart == 0 && this.lookahead == 0)
					{
						num = this.w_size;
					}
					else if (num == -1)
					{
						num--;
					}
					else if (this.strstart >= this.w_size + this.w_size - DeflateManager.MIN_LOOKAHEAD)
					{
						Array.Copy(this.window, this.w_size, this.window, 0, this.w_size);
						this.match_start -= this.w_size;
						this.strstart -= this.w_size;
						this.block_start -= this.w_size;
						num2 = this.hash_size;
						int num3 = num2;
						do
						{
							int num4 = (int)this.head[--num3] & 65535;
							this.head[num3] = (short)((num4 >= this.w_size) ? (num4 - this.w_size) : 0);
						}
						while (--num2 != 0);
						num2 = this.w_size;
						num3 = num2;
						do
						{
							int num4 = (int)this.prev[--num3] & 65535;
							this.prev[num3] = (short)((num4 >= this.w_size) ? (num4 - this.w_size) : 0);
						}
						while (--num2 != 0);
						num += this.w_size;
					}
					if (this._codec.AvailableBytesIn == 0)
					{
						break;
					}
					num2 = this._codec.read_buf(this.window, this.strstart + this.lookahead, num);
					this.lookahead += num2;
					if (this.lookahead >= DeflateManager.MIN_MATCH)
					{
						this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
						this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask;
					}
				}
				while (this.lookahead < DeflateManager.MIN_LOOKAHEAD && this._codec.AvailableBytesIn != 0);
			}

			internal BlockState DeflateFast(FlushType flush)
			{
				int num = 0;
				for (; ; )
				{
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
					{
						this._fillWindow();
						if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
						{
							break;
						}
						if (this.lookahead == 0)
						{
							goto Block_4;
						}
					}
					if (this.lookahead >= DeflateManager.MIN_MATCH)
					{
						this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask;
						num = (int)this.head[this.ins_h] & 65535;
						this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
						this.head[this.ins_h] = (short)this.strstart;
					}
					if ((long)num != 0L && ((this.strstart - num) & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
					{
						if (this.compressionStrategy != CompressionStrategy.HuffmanOnly)
						{
							this.match_length = this.longest_match(num);
						}
					}
					bool flag;
					if (this.match_length >= DeflateManager.MIN_MATCH)
					{
						flag = this._tr_tally(this.strstart - this.match_start, this.match_length - DeflateManager.MIN_MATCH);
						this.lookahead -= this.match_length;
						if (this.match_length <= this.config.MaxLazy && this.lookahead >= DeflateManager.MIN_MATCH)
						{
							this.match_length--;
							do
							{
								this.strstart++;
								this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask;
								num = (int)this.head[this.ins_h] & 65535;
								this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
								this.head[this.ins_h] = (short)this.strstart;
							}
							while (--this.match_length != 0);
							this.strstart++;
						}
						else
						{
							this.strstart += this.match_length;
							this.match_length = 0;
							this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
							this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask;
						}
					}
					else
					{
						flag = this._tr_tally(0, (int)(this.window[this.strstart] & byte.MaxValue));
						this.lookahead--;
						this.strstart++;
					}
					if (flag)
					{
						this.flush_block_only(false);
						if (this._codec.AvailableBytesOut == 0)
						{
							goto Block_14;
						}
					}
				}
				return BlockState.NeedMore;
			Block_4:
				this.flush_block_only(flush == FlushType.Finish);
				if (this._codec.AvailableBytesOut != 0)
				{
					return (flush == FlushType.Finish) ? BlockState.FinishDone : BlockState.BlockDone;
				}
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			Block_14:
				return BlockState.NeedMore;
			}

			internal BlockState DeflateSlow(FlushType flush)
			{
				int num = 0;
				for (; ; )
				{
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
					{
						this._fillWindow();
						if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == FlushType.None)
						{
							break;
						}
						if (this.lookahead == 0)
						{
							goto Block_4;
						}
					}
					if (this.lookahead >= DeflateManager.MIN_MATCH)
					{
						this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask;
						num = (int)this.head[this.ins_h] & 65535;
						this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
						this.head[this.ins_h] = (short)this.strstart;
					}
					this.prev_length = this.match_length;
					this.prev_match = this.match_start;
					this.match_length = DeflateManager.MIN_MATCH - 1;
					if (num != 0 && this.prev_length < this.config.MaxLazy && ((this.strstart - num) & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
					{
						if (this.compressionStrategy != CompressionStrategy.HuffmanOnly)
						{
							this.match_length = this.longest_match(num);
						}
						if (this.match_length <= 5 && (this.compressionStrategy == CompressionStrategy.Filtered || (this.match_length == DeflateManager.MIN_MATCH && this.strstart - this.match_start > 4096)))
						{
							this.match_length = DeflateManager.MIN_MATCH - 1;
						}
					}
					if (this.prev_length >= DeflateManager.MIN_MATCH && this.match_length <= this.prev_length)
					{
						int num2 = this.strstart + this.lookahead - DeflateManager.MIN_MATCH;
						bool flag = this._tr_tally(this.strstart - 1 - this.prev_match, this.prev_length - DeflateManager.MIN_MATCH);
						this.lookahead -= this.prev_length - 1;
						this.prev_length -= 2;
						do
						{
							if (++this.strstart <= num2)
							{
								this.ins_h = ((this.ins_h << this.hash_shift) ^ (int)(this.window[this.strstart + (DeflateManager.MIN_MATCH - 1)] & byte.MaxValue)) & this.hash_mask;
								num = (int)this.head[this.ins_h] & 65535;
								this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
								this.head[this.ins_h] = (short)this.strstart;
							}
						}
						while (--this.prev_length != 0);
						this.match_available = 0;
						this.match_length = DeflateManager.MIN_MATCH - 1;
						this.strstart++;
						if (flag)
						{
							this.flush_block_only(false);
							if (this._codec.AvailableBytesOut == 0)
							{
								goto Block_19;
							}
						}
					}
					else if (this.match_available != 0)
					{
						bool flag = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
						if (flag)
						{
							this.flush_block_only(false);
						}
						this.strstart++;
						this.lookahead--;
						if (this._codec.AvailableBytesOut == 0)
						{
							goto Block_22;
						}
					}
					else
					{
						this.match_available = 1;
						this.strstart++;
						this.lookahead--;
					}
				}
				return BlockState.NeedMore;
			Block_4:
				if (this.match_available != 0)
				{
					bool flag = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
					this.match_available = 0;
				}
				this.flush_block_only(flush == FlushType.Finish);
				if (this._codec.AvailableBytesOut != 0)
				{
					return (flush == FlushType.Finish) ? BlockState.FinishDone : BlockState.BlockDone;
				}
				if (flush == FlushType.Finish)
				{
					return BlockState.FinishStarted;
				}
				return BlockState.NeedMore;
			Block_19:
				return BlockState.NeedMore;
			Block_22:
				return BlockState.NeedMore;
			}

			internal int longest_match(int cur_match)
			{
				int num = this.config.MaxChainLength;
				int num2 = this.strstart;
				int num3 = this.prev_length;
				int num4 = ((this.strstart > this.w_size - DeflateManager.MIN_LOOKAHEAD) ? (this.strstart - (this.w_size - DeflateManager.MIN_LOOKAHEAD)) : 0);
				int niceLength = this.config.NiceLength;
				int num5 = this.w_mask;
				int num6 = this.strstart + DeflateManager.MAX_MATCH;
				byte b = this.window[num2 + num3 - 1];
				byte b2 = this.window[num2 + num3];
				if (this.prev_length >= this.config.GoodLength)
				{
					num >>= 2;
				}
				if (niceLength > this.lookahead)
				{
					niceLength = this.lookahead;
				}
				do
				{
					int num7 = cur_match;
					if (this.window[num7 + num3] == b2 && this.window[num7 + num3 - 1] == b && this.window[num7] == this.window[num2] && this.window[++num7] == this.window[num2 + 1])
					{
						num2 += 2;
						num7++;
						while (this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && this.window[++num2] == this.window[++num7] && num2 < num6)
						{
						}
						int num8 = DeflateManager.MAX_MATCH - (num6 - num2);
						num2 = num6 - DeflateManager.MAX_MATCH;
						if (num8 > num3)
						{
							this.match_start = cur_match;
							num3 = num8;
							if (num8 >= niceLength)
							{
								break;
							}
							b = this.window[num2 + num3 - 1];
							b2 = this.window[num2 + num3];
						}
					}
				}
				while ((cur_match = (int)this.prev[cur_match & num5] & 65535) > num4 && --num != 0);
				int result;
				if (num3 <= this.lookahead)
				{
					result = num3;
				}
				else
				{
					result = this.lookahead;
				}
				return result;
			}

			internal bool WantRfc1950HeaderBytes
			{
				get
				{
					return this._WantRfc1950HeaderBytes;
				}
				set
				{
					this._WantRfc1950HeaderBytes = value;
				}
			}

			internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
			{
				return this.Initialize(codec, level, bits, DeflateManager.MEM_LEVEL_DEFAULT, compressionStrategy);
			}

			internal int Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
			{
				this._codec = codec;
				this._codec.Message = null;
				if (windowBits < 9 || windowBits > 15)
				{
					throw new ZlibException("windowBits must be in the range 9..15.");
				}
				if (memLevel < 1 || memLevel > DeflateManager.MEM_LEVEL_MAX)
				{
					throw new ZlibException(string.Format("memLevel must be in the range 1.. {0}", DeflateManager.MEM_LEVEL_MAX));
				}
				this._codec.dstate = this;
				this.w_bits = windowBits;
				this.w_size = 1 << this.w_bits;
				this.w_mask = this.w_size - 1;
				this.hash_bits = memLevel + 7;
				this.hash_size = 1 << this.hash_bits;
				this.hash_mask = this.hash_size - 1;
				this.hash_shift = (this.hash_bits + DeflateManager.MIN_MATCH - 1) / DeflateManager.MIN_MATCH;
				this.window = new byte[this.w_size * 2];
				this.prev = new short[this.w_size];
				this.head = new short[this.hash_size];
				this.lit_bufsize = 1 << memLevel + 6;
				this.pending = new byte[this.lit_bufsize * 4];
				this._distanceOffset = this.lit_bufsize;
				this._lengthOffset = 3 * this.lit_bufsize;
				this.compressionLevel = level;
				this.compressionStrategy = strategy;
				this.Reset();
				return 0;
			}

			internal void Reset()
			{
				this._codec.TotalBytesIn = (this._codec.TotalBytesOut = 0L);
				this._codec.Message = null;
				this.pendingCount = 0;
				this.nextPending = 0;
				this.Rfc1950BytesEmitted = false;
				this.status = (this.WantRfc1950HeaderBytes ? DeflateManager.INIT_STATE : DeflateManager.BUSY_STATE);
				this._codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
				this.last_flush = 0;
				this._InitializeTreeData();
				this._InitializeLazyMatch();
			}

			private void SetDeflater()
			{
				switch (this.config.Flavor)
				{
					case DeflateFlavor.Store:
						this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateNone);
						break;
					case DeflateFlavor.Fast:
						this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateFast);
						break;
					case DeflateFlavor.Slow:
						this.DeflateFunction = new DeflateManager.CompressFunc(this.DeflateSlow);
						break;
				}
			}

			internal int Deflate(FlushType flush)
			{
				if (this._codec.OutputBuffer == null || (this._codec.InputBuffer == null && this._codec.AvailableBytesIn != 0) || (this.status == DeflateManager.FINISH_STATE && flush != FlushType.Finish))
				{
					this._codec.Message = DeflateManager._ErrorMessage[4];
					throw new ZlibException(string.Format("Something is fishy. [{0}]", this._codec.Message));
				}
				if (this._codec.AvailableBytesOut == 0)
				{
					this._codec.Message = DeflateManager._ErrorMessage[7];
					throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
				}
				int num = this.last_flush;
				this.last_flush = (int)flush;
				if (this.status == DeflateManager.INIT_STATE)
				{
					int num2 = DeflateManager.Z_DEFLATED + (this.w_bits - 8 << 4) << 8;
					int num3 = ((this.compressionLevel - CompressionLevel.BestSpeed) & 255) >> 1;
					if (num3 > 3)
					{
						num3 = 3;
					}
					num2 |= num3 << 6;
					if (this.strstart != 0)
					{
						num2 |= DeflateManager.PRESET_DICT;
					}
					num2 += 31 - num2 % 31;
					this.status = DeflateManager.BUSY_STATE;
					this.pending[this.pendingCount++] = (byte)(num2 >> 8);
					this.pending[this.pendingCount++] = (byte)num2;
					if (this.strstart != 0)
					{
						this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 4278190080U) >> 24);
						this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 16711680U) >> 16);
						this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 65280U) >> 8);
						this.pending[this.pendingCount++] = (byte)(this._codec._Adler32 & 255U);
					}
					this._codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
				}
				if (this.pendingCount != 0)
				{
					this._codec.flush_pending();
					if (this._codec.AvailableBytesOut == 0)
					{
						this.last_flush = -1;
						return 0;
					}
				}
				else if (this._codec.AvailableBytesIn == 0 && flush <= (FlushType)num && flush != FlushType.Finish)
				{
					return 0;
				}
				if (this.status == DeflateManager.FINISH_STATE && this._codec.AvailableBytesIn != 0)
				{
					this._codec.Message = DeflateManager._ErrorMessage[7];
					throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
				}
				if (this._codec.AvailableBytesIn != 0 || this.lookahead != 0 || (flush != FlushType.None && this.status != DeflateManager.FINISH_STATE))
				{
					BlockState blockState = this.DeflateFunction(flush);
					if (blockState == BlockState.FinishStarted || blockState == BlockState.FinishDone)
					{
						this.status = DeflateManager.FINISH_STATE;
					}
					if (blockState == BlockState.NeedMore || blockState == BlockState.FinishStarted)
					{
						if (this._codec.AvailableBytesOut == 0)
						{
							this.last_flush = -1;
						}
						return 0;
					}
					if (blockState == BlockState.BlockDone)
					{
						if (flush == FlushType.Partial)
						{
							this._tr_align();
						}
						else
						{
							this._tr_stored_block(0, 0, false);
							if (flush == FlushType.Full)
							{
								for (int i = 0; i < this.hash_size; i++)
								{
									this.head[i] = 0;
								}
							}
						}
						this._codec.flush_pending();
						if (this._codec.AvailableBytesOut == 0)
						{
							this.last_flush = -1;
							return 0;
						}
					}
				}
				int result;
				if (flush != FlushType.Finish)
				{
					result = 0;
				}
				else if (!this.WantRfc1950HeaderBytes || this.Rfc1950BytesEmitted)
				{
					result = 1;
				}
				else
				{
					this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 4278190080U) >> 24);
					this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 16711680U) >> 16);
					this.pending[this.pendingCount++] = (byte)((this._codec._Adler32 & 65280U) >> 8);
					this.pending[this.pendingCount++] = (byte)(this._codec._Adler32 & 255U);
					this._codec.flush_pending();
					this.Rfc1950BytesEmitted = true;
					result = ((this.pendingCount != 0) ? 0 : 1);
				}
				return result;
			}

			private static readonly int MEM_LEVEL_MAX = 9;

			private static readonly int MEM_LEVEL_DEFAULT = 8;

			private DeflateManager.CompressFunc DeflateFunction;

			private static readonly string[] _ErrorMessage = new string[] { "need dictionary", "stream end", "", "file error", "stream error", "data error", "insufficient memory", "buffer error", "incompatible version", "" };

			private static readonly int PRESET_DICT = 32;

			private static readonly int INIT_STATE = 42;

			private static readonly int BUSY_STATE = 113;

			private static readonly int FINISH_STATE = 666;

			private static readonly int Z_DEFLATED = 8;

			private static readonly int STORED_BLOCK = 0;

			private static readonly int STATIC_TREES = 1;

			private static readonly int DYN_TREES = 2;

			private static readonly int Z_BINARY = 0;

			private static readonly int Z_ASCII = 1;

			private static readonly int Z_UNKNOWN = 2;

			private static readonly int Buf_size = 16;

			private static readonly int MIN_MATCH = 3;

			private static readonly int MAX_MATCH = 258;

			private static readonly int MIN_LOOKAHEAD = DeflateManager.MAX_MATCH + DeflateManager.MIN_MATCH + 1;

			private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

			private static readonly int END_BLOCK = 256;

			internal ZlibCodec _codec;

			internal int status;

			internal byte[] pending;

			internal int nextPending;

			internal int pendingCount;

			internal sbyte data_type;

			internal int last_flush;

			internal int w_size;

			internal int w_bits;

			internal int w_mask;

			internal byte[] window;

			internal int window_size;

			internal short[] prev;

			internal short[] head;

			internal int ins_h;

			internal int hash_size;

			internal int hash_bits;

			internal int hash_mask;

			internal int hash_shift;

			internal int block_start;

			private DeflateManager.Config config;

			internal int match_length;

			internal int prev_match;

			internal int match_available;

			internal int strstart;

			internal int match_start;

			internal int lookahead;

			internal int prev_length;

			internal CompressionLevel compressionLevel;

			internal CompressionStrategy compressionStrategy;

			internal short[] dyn_ltree;

			internal short[] dyn_dtree;

			internal short[] bl_tree;

			internal Tree treeLiterals = new Tree();

			internal Tree treeDistances = new Tree();

			internal Tree treeBitLengths = new Tree();

			internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

			internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

			internal int heap_len;

			internal int heap_max;

			internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

			internal int _lengthOffset;

			internal int lit_bufsize;

			internal int last_lit;

			internal int _distanceOffset;

			internal int opt_len;

			internal int static_len;

			internal int matches;

			internal int last_eob_len;

			internal short bi_buf;

			internal int bi_valid;

			private bool Rfc1950BytesEmitted = false;

			private bool _WantRfc1950HeaderBytes = true;

			internal delegate BlockState CompressFunc(FlushType flush);

			internal class Config
			{
				private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
				{
					this.GoodLength = goodLength;
					this.MaxLazy = maxLazy;
					this.NiceLength = niceLength;
					this.MaxChainLength = maxChainLength;
					this.Flavor = flavor;
				}

				public static DeflateManager.Config Lookup(CompressionLevel level)
				{
					return DeflateManager.Config.Table[(int)level];
				}

				internal int GoodLength;

				internal int MaxLazy;

				internal int NiceLength;

				internal int MaxChainLength;

				internal DeflateFlavor Flavor;

				private static readonly DeflateManager.Config[] Table = new DeflateManager.Config[]
				{
				new DeflateManager.Config(0, 0, 0, 0, DeflateFlavor.Store),
				new DeflateManager.Config(4, 4, 8, 4, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 5, 16, 8, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 6, 32, 32, DeflateFlavor.Fast),
				new DeflateManager.Config(4, 4, 16, 16, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 16, 32, 32, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 16, 128, 128, DeflateFlavor.Slow),
				new DeflateManager.Config(8, 32, 128, 256, DeflateFlavor.Slow),
				new DeflateManager.Config(32, 128, 258, 1024, DeflateFlavor.Slow),
				new DeflateManager.Config(32, 258, 258, 4096, DeflateFlavor.Slow)
				};
			}
		}

		public enum FlushType
		{
			None,
			Partial,
			Sync,
			Full,
			Finish
		}

		public class GZipStream : Stream
		{
			public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen, bool chimeraOmitCheckCRC = false)
			{
				this._baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen, chimeraOmitCheckCRC);
			}

			public string Comment
			{
				get
				{
					return this._Comment;
				}
				set
				{
					if (this._disposed)
					{
						throw new ObjectDisposedException("GZipStream");
					}
					this._Comment = value;
				}
			}

			public string FileName
			{
				get
				{
					return this._FileName;
				}
				set
				{
					if (this._disposed)
					{
						throw new ObjectDisposedException("GZipStream");
					}
					this._FileName = value;
					if (this._FileName != null)
					{
						if (this._FileName.IndexOf("/") != -1)
						{
							this._FileName = this._FileName.Replace("/", "\\");
						}
						if (this._FileName.EndsWith("\\"))
						{
							throw new Exception("Illegal filename");
						}
						if (this._FileName.IndexOf("\\") != -1)
						{
							this._FileName = Path.GetFileName(this._FileName);
						}
					}
				}
			}

			protected override void Dispose(bool disposing)
			{
				try
				{
					if (!this._disposed)
					{
						if (disposing && this._baseStream != null)
						{
							this._baseStream.Close();
							this._Crc32 = this._baseStream.Crc32;
						}
						this._disposed = true;
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			public override bool CanRead
			{
				get
				{
					if (this._disposed)
					{
						throw new ObjectDisposedException("GZipStream");
					}
					return this._baseStream._stream.CanRead;
				}
			}

			public override bool CanSeek
			{
				get
				{
					return false;
				}
			}

			public override bool CanWrite
			{
				get
				{
					if (this._disposed)
					{
						throw new ObjectDisposedException("GZipStream");
					}
					return this._baseStream._stream.CanWrite;
				}
			}

			public override void Flush()
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				this._baseStream.Flush();
			}

			public override long Length
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override long Position
			{
				get
				{
					long result;
					if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
					{
						result = this._baseStream._z.TotalBytesOut + (long)this._headerByteCount;
					}
					else if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
					{
						result = this._baseStream._z.TotalBytesIn + (long)this._baseStream._gzipHeaderByteCount;
					}
					else
					{
						result = 0L;
					}
					return result;
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				int result = this._baseStream.Read(buffer, offset, count);
				if (!this._firstReadDone)
				{
					this._firstReadDone = true;
					this.FileName = this._baseStream._GzipFileName;
					this.Comment = this._baseStream._GzipComment;
				}
				return result;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("GZipStream");
				}
				if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
				{
					if (!this._baseStream._wantCompress)
					{
						throw new InvalidOperationException();
					}
					this._headerByteCount = this.EmitHeader();
				}
				this._baseStream.Write(buffer, offset, count);
			}

			private int EmitHeader()
			{
				byte[] array = ((this.Comment == null) ? null : GZipStream.iso8859dash1.GetBytes(this.Comment));
				byte[] array2 = ((this.FileName == null) ? null : GZipStream.iso8859dash1.GetBytes(this.FileName));
				int num = ((this.Comment == null) ? 0 : (array.Length + 1));
				int num2 = ((this.FileName == null) ? 0 : (array2.Length + 1));
				int num3 = 10 + num + num2;
				byte[] array3 = new byte[num3];
				int num4 = 0;
				array3[num4++] = 31;
				array3[num4++] = 139;
				array3[num4++] = 8;
				byte b = 0;
				if (this.Comment != null)
				{
					b ^= 16;
				}
				if (this.FileName != null)
				{
					b ^= 8;
				}
				array3[num4++] = b;
				if (this.LastModified == null)
				{
					this.LastModified = new DateTime?(DateTime.Now);
				}
				int value = (int)(this.LastModified.Value - GZipStream._unixEpoch).TotalSeconds;
				Array.Copy(BitConverter.GetBytes(value), 0, array3, num4, 4);
				num4 += 4;
				array3[num4++] = 0;
				array3[num4++] = byte.MaxValue;
				if (num2 != 0)
				{
					Array.Copy(array2, 0, array3, num4, num2 - 1);
					num4 += num2 - 1;
					array3[num4++] = 0;
				}
				if (num != 0)
				{
					Array.Copy(array, 0, array3, num4, num - 1);
					num4 += num - 1;
					array3[num4++] = 0;
				}
				this._baseStream._stream.Write(array3, 0, array3.Length);
				return array3.Length;
			}

			public DateTime? LastModified;

			private int _headerByteCount;

			internal ZlibBaseStream _baseStream;

			private bool _disposed;

			private bool _firstReadDone;

			private string _FileName;

			private string _Comment;

			private int _Crc32;

			internal static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			internal static readonly Encoding iso8859dash1 = Encoding.GetEncoding("iso-8859-1");
		}

		internal sealed class InflateBlocks
		{
			internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
			{
				this._codec = codec;
				this.hufts = new int[4320];
				this.window = new byte[w];
				this.end = w;
				this.checkfn = checkfn;
				this.mode = InflateBlocks.InflateBlockMode.TYPE;
				this.Reset();
			}

			internal uint Reset()
			{
				uint result = this.check;
				this.mode = InflateBlocks.InflateBlockMode.TYPE;
				this.bitk = 0;
				this.bitb = 0;
				this.readAt = (this.writeAt = 0);
				if (this.checkfn != null)
				{
					this._codec._Adler32 = (this.check = Adler.Adler32(0U, null, 0, 0));
				}
				return result;
			}

			internal int Process(int r)
			{
				int num = this._codec.NextIn;
				int num2 = this._codec.AvailableBytesIn;
				int num3 = this.bitb;
				int i = this.bitk;
				int num4 = this.writeAt;
				int num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
				int num6;
				for (; ; )
				{
					int[] array;
					int[] array2;
					switch (this.mode)
					{
						case InflateBlocks.InflateBlockMode.TYPE:
							while (i < 3)
							{
								if (num2 == 0)
								{
									goto IL_AD;
								}
								r = 0;
								num2--;
								num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							num6 = num3 & 7;
							this.last = num6 & 1;
							switch ((uint)num6 >> 1)
							{
								case 0U:
									num3 >>= 3;
									i -= 3;
									num6 = i & 7;
									num3 >>= num6;
									i -= num6;
									this.mode = InflateBlocks.InflateBlockMode.LENS;
									break;
								case 1U:
									{
										array = new int[1];
										array2 = new int[1];
										int[][] array3 = new int[1][];
										int[][] array4 = new int[1][];
										InfTree.inflate_trees_fixed(array, array2, array3, array4, this._codec);
										this.codes.Init(array[0], array2[0], array3[0], 0, array4[0], 0);
										num3 >>= 3;
										i -= 3;
										this.mode = InflateBlocks.InflateBlockMode.CODES;
										break;
									}
								case 2U:
									num3 >>= 3;
									i -= 3;
									this.mode = InflateBlocks.InflateBlockMode.TABLE;
									break;
								case 3U:
									goto IL_20D;
							}
							continue;
						case InflateBlocks.InflateBlockMode.LENS:
							while (i < 32)
							{
								if (num2 == 0)
								{
									goto IL_2AB;
								}
								r = 0;
								num2--;
								num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							if (((~num3 >> 16) & 65535) != (num3 & 65535))
							{
								goto Block_8;
							}
							this.left = num3 & 65535;
							i = (num3 = 0);
							this.mode = ((this.left != 0) ? InflateBlocks.InflateBlockMode.STORED : ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE));
							continue;
						case InflateBlocks.InflateBlockMode.STORED:
							if (num2 == 0)
							{
								goto Block_11;
							}
							if (num5 == 0)
							{
								if (num4 == this.end && this.readAt != 0)
								{
									num4 = 0;
									num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
								}
								if (num5 == 0)
								{
									this.writeAt = num4;
									r = this.Flush(r);
									num4 = this.writeAt;
									num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
									if (num4 == this.end && this.readAt != 0)
									{
										num4 = 0;
										num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
									}
									if (num5 == 0)
									{
										goto Block_21;
									}
								}
							}
							r = 0;
							num6 = this.left;
							if (num6 > num2)
							{
								num6 = num2;
							}
							if (num6 > num5)
							{
								num6 = num5;
							}
							Array.Copy(this._codec.InputBuffer, num, this.window, num4, num6);
							num += num6;
							num2 -= num6;
							num4 += num6;
							num5 -= num6;
							if ((this.left -= num6) != 0)
							{
								continue;
							}
							this.mode = ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE);
							continue;
						case InflateBlocks.InflateBlockMode.TABLE:
							while (i < 14)
							{
								if (num2 == 0)
								{
									goto IL_67C;
								}
								r = 0;
								num2--;
								num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							num6 = (this.table = num3 & 16383);
							if ((num6 & 31) > 29 || ((num6 >> 5) & 31) > 29)
							{
								goto Block_29;
							}
							num6 = 258 + (num6 & 31) + ((num6 >> 5) & 31);
							if (this.blens == null || this.blens.Length < num6)
							{
								this.blens = new int[num6];
							}
							else
							{
								Array.Clear(this.blens, 0, num6);
							}
							num3 >>= 14;
							i -= 14;
							this.index = 0;
							this.mode = InflateBlocks.InflateBlockMode.BTREE;
							goto IL_826;
						case InflateBlocks.InflateBlockMode.BTREE:
							goto IL_826;
						case InflateBlocks.InflateBlockMode.DTREE:
							goto IL_A26;
						case InflateBlocks.InflateBlockMode.CODES:
							goto IL_ECF;
						case InflateBlocks.InflateBlockMode.DRY:
							goto IL_FC3;
						case InflateBlocks.InflateBlockMode.DONE:
							goto IL_107A;
						case InflateBlocks.InflateBlockMode.BAD:
							goto IL_10DA;
					}
					break;
					// continue;
				IL_ECF:
					this.bitb = num3;
					this.bitk = i;
					this._codec.AvailableBytesIn = num2;
					this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
					this._codec.NextIn = num;
					this.writeAt = num4;
					r = this.codes.Process(this, r);
					if (r != 1)
					{
						goto Block_53;
					}
					r = 0;
					num = this._codec.NextIn;
					num2 = this._codec.AvailableBytesIn;
					num3 = this.bitb;
					i = this.bitk;
					num4 = this.writeAt;
					num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
					if (this.last == 0)
					{
						this.mode = InflateBlocks.InflateBlockMode.TYPE;
						continue;
					}
					goto IL_FBA;
				IL_A26:
					for (; ; )
					{
						num6 = this.table;
						if (this.index >= 258 + (num6 & 31) + ((num6 >> 5) & 31))
						{
							break;
						}
						num6 = this.bb[0];
						while (i < num6)
						{
							if (num2 == 0)
							{
								goto IL_A79;
							}
							r = 0;
							num2--;
							num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
							i += 8;
						}
						num6 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 1];
						int num7 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 2];
						if (num7 < 16)
						{
							num3 >>= num6;
							i -= num6;
							this.blens[this.index++] = num7;
						}
						else
						{
							int num8 = ((num7 == 18) ? 7 : (num7 - 14));
							int num9 = ((num7 == 18) ? 11 : 3);
							while (i < num6 + num8)
							{
								if (num2 == 0)
								{
									goto IL_BC0;
								}
								r = 0;
								num2--;
								num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							num3 >>= num6;
							i -= num6;
							num9 += num3 & InternalInflateConstants.InflateMask[num8];
							num3 >>= num8;
							i -= num8;
							num8 = this.index;
							num6 = this.table;
							if (num8 + num9 > 258 + (num6 & 31) + ((num6 >> 5) & 31) || (num7 == 16 && num8 < 1))
							{
								goto Block_48;
							}
							num7 = ((num7 == 16) ? this.blens[num8 - 1] : 0);
							do
							{
								this.blens[num8++] = num7;
							}
							while (--num9 != 0);
							this.index = num8;
						}
					}
					this.tb[0] = -1;
					array = new int[] { 9 };
					array2 = new int[] { 6 };
					int[] array5 = new int[1];
					int[] array6 = new int[1];
					num6 = this.table;
					num6 = this.inftree.inflate_trees_dynamic(257 + (num6 & 31), 1 + ((num6 >> 5) & 31), this.blens, array, array2, array5, array6, this.hufts, this._codec);
					if (num6 != 0)
					{
						goto Block_51;
					}
					this.codes.Init(array[0], array2[0], this.hufts, array5[0], this.hufts, array6[0]);
					this.mode = InflateBlocks.InflateBlockMode.CODES;
					goto IL_ECF;
				IL_826:
					while (this.index < 4 + (this.table >> 10))
					{
						while (i < 3)
						{
							if (num2 == 0)
							{
								goto IL_844;
							}
							r = 0;
							num2--;
							num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
							i += 8;
						}
						this.blens[InflateBlocks.border[this.index++]] = num3 & 7;
						num3 >>= 3;
						i -= 3;
					}
					while (this.index < 19)
					{
						this.blens[InflateBlocks.border[this.index++]] = 0;
					}
					this.bb[0] = 7;
					num6 = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this._codec);
					if (num6 != 0)
					{
						goto Block_36;
					}
					this.index = 0;
					this.mode = InflateBlocks.InflateBlockMode.DTREE;
					goto IL_A26;
				}
				r = -2;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_AD:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_20D:
				num3 >>= 3;
				i -= 3;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
				this._codec.Message = "invalid block type";
				r = -3;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_2AB:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_8:
				this.mode = InflateBlocks.InflateBlockMode.BAD;
				this._codec.Message = "invalid stored block lengths";
				r = -3;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_11:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_21:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_67C:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_29:
				this.mode = InflateBlocks.InflateBlockMode.BAD;
				this._codec.Message = "too many length or distance symbols";
				r = -3;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_844:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_36:
				r = num6;
				if (r == -3)
				{
					this.blens = null;
					this.mode = InflateBlocks.InflateBlockMode.BAD;
				}
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_A79:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_BC0:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_48:
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
				this._codec.Message = "invalid bit length repeat";
				r = -3;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_51:
				if (num6 == -3)
				{
					this.blens = null;
					this.mode = InflateBlocks.InflateBlockMode.BAD;
				}
				r = num6;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			Block_53:
				return this.Flush(r);
			IL_FBA:
				this.mode = InflateBlocks.InflateBlockMode.DRY;
			IL_FC3:
				this.writeAt = num4;
				r = this.Flush(r);
				num4 = this.writeAt;
				int num10 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
				if (this.readAt != this.writeAt)
				{
					this.bitb = num3;
					this.bitk = i;
					this._codec.AvailableBytesIn = num2;
					this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
					this._codec.NextIn = num;
					this.writeAt = num4;
					return this.Flush(r);
				}
				this.mode = InflateBlocks.InflateBlockMode.DONE;
			IL_107A:
				r = 1;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			IL_10DA:
				r = -3;
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			}

			internal void Free()
			{
				this.Reset();
				this.window = null;
				this.hufts = null;
			}

			internal int Flush(int r)
			{
				for (int i = 0; i < 2; i++)
				{
					int num;
					if (i == 0)
					{
						num = ((this.readAt <= this.writeAt) ? this.writeAt : this.end) - this.readAt;
					}
					else
					{
						num = this.writeAt - this.readAt;
					}
					if (num == 0)
					{
						if (r == -5)
						{
							r = 0;
						}
						return r;
					}
					if (num > this._codec.AvailableBytesOut)
					{
						num = this._codec.AvailableBytesOut;
					}
					if (num != 0 && r == -5)
					{
						r = 0;
					}
					this._codec.AvailableBytesOut -= num;
					this._codec.TotalBytesOut += (long)num;
					if (this.checkfn != null)
					{
						this._codec._Adler32 = (this.check = Adler.Adler32(this.check, this.window, this.readAt, num));
					}
					Array.Copy(this.window, this.readAt, this._codec.OutputBuffer, this._codec.NextOut, num);
					this._codec.NextOut += num;
					this.readAt += num;
					if (this.readAt == this.end && i == 0)
					{
						this.readAt = 0;
						if (this.writeAt == this.end)
						{
							this.writeAt = 0;
						}
					}
					else
					{
						i++;
					}
				}
				return r;
			}

			private const int MANY = 1440;

			internal static readonly int[] border = new int[]
			{
			16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
			11, 4, 12, 3, 13, 2, 14, 1, 15
			};

			private InflateBlocks.InflateBlockMode mode;

			internal int left;

			internal int table;

			internal int index;

			internal int[] blens;

			internal int[] bb = new int[1];

			internal int[] tb = new int[1];

			internal InflateCodes codes = new InflateCodes();

			internal int last;

			internal ZlibCodec _codec;

			internal int bitk;

			internal int bitb;

			internal int[] hufts;

			internal byte[] window;

			internal int end;

			internal int readAt;

			internal int writeAt;

			internal object checkfn;

			internal uint check;

			internal InfTree inftree = new InfTree();

			private enum InflateBlockMode
			{
				TYPE,
				LENS,
				STORED,
				TABLE,
				BTREE,
				DTREE,
				CODES,
				DRY,
				DONE,
				BAD
			}
		}

		internal sealed class InflateCodes
		{
			internal InflateCodes()
			{
			}

			internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
			{
				this.mode = 0;
				this.lbits = (byte)bl;
				this.dbits = (byte)bd;
				this.ltree = tl;
				this.ltree_index = tl_index;
				this.dtree = td;
				this.dtree_index = td_index;
				this.tree = null;
			}

			internal int Process(InflateBlocks blocks, int r)
			{
				ZlibCodec codec = blocks._codec;
				int num = codec.NextIn;
				int num2 = codec.AvailableBytesIn;
				int num3 = blocks.bitb;
				int i = blocks.bitk;
				int num4 = blocks.writeAt;
				int num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
				for (; ; )
				{
					int num6;
					switch (this.mode)
					{
						case 0:
							if (num5 >= 258 && num2 >= 10)
							{
								blocks.bitb = num3;
								blocks.bitk = i;
								codec.AvailableBytesIn = num2;
								codec.TotalBytesIn += (long)(num - codec.NextIn);
								codec.NextIn = num;
								blocks.writeAt = num4;
								r = this.InflateFast((int)this.lbits, (int)this.dbits, this.ltree, this.ltree_index, this.dtree, this.dtree_index, blocks, codec);
								num = codec.NextIn;
								num2 = codec.AvailableBytesIn;
								num3 = blocks.bitb;
								i = blocks.bitk;
								num4 = blocks.writeAt;
								num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
								if (r != 0)
								{
									this.mode = ((r == 1) ? 7 : 9);
									continue;
								}
							}
							this.need = (int)this.lbits;
							this.tree = this.ltree;
							this.tree_index = this.ltree_index;
							this.mode = 1;
							goto IL_1C7;
						case 1:
							goto IL_1C7;
						case 2:
							num6 = this.bitsToGet;
							while (i < num6)
							{
								if (num2 == 0)
								{
									goto IL_3D7;
								}
								r = 0;
								num2--;
								num3 |= (int)(codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							this.len += num3 & InternalInflateConstants.InflateMask[num6];
							num3 >>= num6;
							i -= num6;
							this.need = (int)this.dbits;
							this.tree = this.dtree;
							this.tree_index = this.dtree_index;
							this.mode = 3;
							goto IL_4B1;
						case 3:
							goto IL_4B1;
						case 4:
							num6 = this.bitsToGet;
							while (i < num6)
							{
								if (num2 == 0)
								{
									goto IL_67D;
								}
								r = 0;
								num2--;
								num3 |= (int)(codec.InputBuffer[num++] & byte.MaxValue) << i;
								i += 8;
							}
							this.dist += num3 & InternalInflateConstants.InflateMask[num6];
							num3 >>= num6;
							i -= num6;
							this.mode = 5;
							goto IL_733;
						case 5:
							goto IL_733;
						case 6:
							if (num5 == 0)
							{
								if (num4 == blocks.end && blocks.readAt != 0)
								{
									num4 = 0;
									num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
								}
								if (num5 == 0)
								{
									blocks.writeAt = num4;
									r = blocks.Flush(r);
									num4 = blocks.writeAt;
									num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
									if (num4 == blocks.end && blocks.readAt != 0)
									{
										num4 = 0;
										num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
									}
									if (num5 == 0)
									{
										goto Block_44;
									}
								}
							}
							r = 0;
							blocks.window[num4++] = (byte)this.lit;
							num5--;
							this.mode = 0;
							continue;
						case 7:
							goto IL_A86;
						case 8:
							goto IL_B52;
						case 9:
							goto IL_BA5;
					}
					break;
					// continue;
				IL_1C7:
					num6 = this.need;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_1E4;
						}
						r = 0;
						num2--;
						num3 |= (int)(codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					int num7 = (this.tree_index + (num3 & InternalInflateConstants.InflateMask[num6])) * 3;
					num3 >>= this.tree[num7 + 1];
					i -= this.tree[num7 + 1];
					int num8 = this.tree[num7];
					if (num8 == 0)
					{
						this.lit = this.tree[num7 + 2];
						this.mode = 6;
						continue;
					}
					if ((num8 & 16) != 0)
					{
						this.bitsToGet = num8 & 15;
						this.len = this.tree[num7 + 2];
						this.mode = 2;
						continue;
					}
					if ((num8 & 64) == 0)
					{
						this.need = num8;
						this.tree_index = num7 / 3 + this.tree[num7 + 2];
						continue;
					}
					if ((num8 & 32) != 0)
					{
						this.mode = 7;
						continue;
					}
					goto IL_352;
				IL_4B1:
					num6 = this.need;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_4CE;
						}
						r = 0;
						num2--;
						num3 |= (int)(codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num7 = (this.tree_index + (num3 & InternalInflateConstants.InflateMask[num6])) * 3;
					num3 >>= this.tree[num7 + 1];
					i -= this.tree[num7 + 1];
					num8 = this.tree[num7];
					if ((num8 & 16) != 0)
					{
						this.bitsToGet = num8 & 15;
						this.dist = this.tree[num7 + 2];
						this.mode = 4;
						continue;
					}
					if ((num8 & 64) == 0)
					{
						this.need = num8;
						this.tree_index = num7 / 3 + this.tree[num7 + 2];
						continue;
					}
					goto IL_5F8;
				IL_733:
					int j;
					for (j = num4 - this.dist; j < 0; j += blocks.end)
					{
					}
					while (this.len != 0)
					{
						if (num5 == 0)
						{
							if (num4 == blocks.end && blocks.readAt != 0)
							{
								num4 = 0;
								num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
							}
							if (num5 == 0)
							{
								blocks.writeAt = num4;
								r = blocks.Flush(r);
								num4 = blocks.writeAt;
								num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
								if (num4 == blocks.end && blocks.readAt != 0)
								{
									num4 = 0;
									num5 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
								}
								if (num5 == 0)
								{
									goto Block_32;
								}
							}
						}
						blocks.window[num4++] = blocks.window[j++];
						num5--;
						if (j == blocks.end)
						{
							j = 0;
						}
						this.len--;
					}
					this.mode = 0;
				}
				r = -2;
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_1E4:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_352:
				this.mode = 9;
				codec.Message = "invalid literal/length code";
				r = -3;
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_3D7:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_4CE:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_5F8:
				this.mode = 9;
				codec.Message = "invalid distance code";
				r = -3;
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_67D:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			Block_32:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			Block_44:
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_A86:
				if (i > 7)
				{
					i -= 8;
					num2++;
					num--;
				}
				blocks.writeAt = num4;
				r = blocks.Flush(r);
				num4 = blocks.writeAt;
				int num9 = ((num4 < blocks.readAt) ? (blocks.readAt - num4 - 1) : (blocks.end - num4));
				if (blocks.readAt != blocks.writeAt)
				{
					blocks.bitb = num3;
					blocks.bitk = i;
					codec.AvailableBytesIn = num2;
					codec.TotalBytesIn += (long)(num - codec.NextIn);
					codec.NextIn = num;
					blocks.writeAt = num4;
					return blocks.Flush(r);
				}
				this.mode = 8;
			IL_B52:
				r = 1;
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			IL_BA5:
				r = -3;
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			}

			internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
			{
				int num = z.NextIn;
				int num2 = z.AvailableBytesIn;
				int num3 = s.bitb;
				int i = s.bitk;
				int num4 = s.writeAt;
				int num5 = ((num4 < s.readAt) ? (s.readAt - num4 - 1) : (s.end - num4));
				int num6 = InternalInflateConstants.InflateMask[bl];
				int num7 = InternalInflateConstants.InflateMask[bd];
				int num10;
				int num11;
				for (; ; )
				{
					while (i < 20)
					{
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					int num8 = num3 & num6;
					int num9 = (tl_index + num8) * 3;
					if ((num10 = tl[num9]) == 0)
					{
						num3 >>= tl[num9 + 1];
						i -= tl[num9 + 1];
						s.window[num4++] = (byte)tl[num9 + 2];
						num5--;
					}
					else
					{
						for (; ; )
						{
							num3 >>= tl[num9 + 1];
							i -= tl[num9 + 1];
							if ((num10 & 16) != 0)
							{
								goto Block_4;
							}
							if ((num10 & 64) != 0)
							{
								goto IL_58E;
							}
							num8 += tl[num9 + 2];
							num8 += num3 & InternalInflateConstants.InflateMask[num10];
							num9 = (tl_index + num8) * 3;
							if ((num10 = tl[num9]) == 0)
							{
								goto Block_20;
							}
						}
					IL_6BE:
						goto IL_6BF;
					Block_20:
						num3 >>= tl[num9 + 1];
						i -= tl[num9 + 1];
						s.window[num4++] = (byte)tl[num9 + 2];
						num5--;
						goto IL_6BE;
					Block_4:
						num10 &= 15;
						num11 = tl[num9 + 2] + (num3 & InternalInflateConstants.InflateMask[num10]);
						num3 >>= num10;
						for (i -= num10; i < 15; i += 8)
						{
							num2--;
							num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						}
						num8 = num3 & num7;
						num9 = (td_index + num8) * 3;
						num10 = td[num9];
						for (; ; )
						{
							num3 >>= td[num9 + 1];
							i -= td[num9 + 1];
							if ((num10 & 16) != 0)
							{
								break;
							}
							if ((num10 & 64) != 0)
							{
								goto IL_46D;
							}
							num8 += td[num9 + 2];
							num8 += num3 & InternalInflateConstants.InflateMask[num10];
							num9 = (td_index + num8) * 3;
							num10 = td[num9];
						}
						num10 &= 15;
						while (i < num10)
						{
							num2--;
							num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
							i += 8;
						}
						int num12 = td[num9 + 2] + (num3 & InternalInflateConstants.InflateMask[num10]);
						num3 >>= num10;
						i -= num10;
						num5 -= num11;
						int num13;
						if (num4 >= num12)
						{
							num13 = num4 - num12;
							if (num4 - num13 > 0 && 2 > num4 - num13)
							{
								s.window[num4++] = s.window[num13++];
								s.window[num4++] = s.window[num13++];
								num11 -= 2;
							}
							else
							{
								Array.Copy(s.window, num13, s.window, num4, 2);
								num4 += 2;
								num13 += 2;
								num11 -= 2;
							}
						}
						else
						{
							num13 = num4 - num12;
							do
							{
								num13 += s.end;
							}
							while (num13 < 0);
							num10 = s.end - num13;
							if (num11 > num10)
							{
								num11 -= num10;
								if (num4 - num13 > 0 && num10 > num4 - num13)
								{
									do
									{
										s.window[num4++] = s.window[num13++];
									}
									while (--num10 != 0);
								}
								else
								{
									Array.Copy(s.window, num13, s.window, num4, num10);
									num4 += num10;
									num13 += num10;
								}
								num13 = 0;
							}
						}
						if (num4 - num13 > 0 && num11 > num4 - num13)
						{
							do
							{
								s.window[num4++] = s.window[num13++];
							}
							while (--num11 != 0);
						}
						else
						{
							Array.Copy(s.window, num13, s.window, num4, num11);
							num4 += num11;
							num13 += num11;
						}
					}
				IL_6BF:
					if (num5 < 258 || num2 < 10)
					{
						goto Block_25;
					}
				}
			IL_46D:
				z.Message = "invalid distance code";
				num11 = z.AvailableBytesIn - num2;
				num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
				num2 += num11;
				num -= num11;
				i -= num11 << 3;
				s.bitb = num3;
				s.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				s.writeAt = num4;
				return -3;
			IL_58E:
				if ((num10 & 32) != 0)
				{
					num11 = z.AvailableBytesIn - num2;
					num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
					num2 += num11;
					num -= num11;
					i -= num11 << 3;
					s.bitb = num3;
					s.bitk = i;
					z.AvailableBytesIn = num2;
					z.TotalBytesIn += (long)(num - z.NextIn);
					z.NextIn = num;
					s.writeAt = num4;
					return 1;
				}
				z.Message = "invalid literal/length code";
				num11 = z.AvailableBytesIn - num2;
				num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
				num2 += num11;
				num -= num11;
				i -= num11 << 3;
				s.bitb = num3;
				s.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				s.writeAt = num4;
				return -3;
			Block_25:
				num11 = z.AvailableBytesIn - num2;
				num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
				num2 += num11;
				num -= num11;
				i -= num11 << 3;
				s.bitb = num3;
				s.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				s.writeAt = num4;
				return 0;
			}

			private const int START = 0;

			private const int LEN = 1;

			private const int LENEXT = 2;

			private const int DIST = 3;

			private const int DISTEXT = 4;

			private const int COPY = 5;

			private const int LIT = 6;

			private const int WASH = 7;

			private const int END = 8;

			private const int BADCODE = 9;

			internal int mode;

			internal int len;

			internal int[] tree;

			internal int tree_index = 0;

			internal int need;

			internal int lit;

			internal int bitsToGet;

			internal int dist;

			internal byte lbits;

			internal byte dbits;

			internal int[] ltree;

			internal int ltree_index;

			internal int[] dtree;

			internal int dtree_index;
		}

		internal sealed class InflateManager
		{
			public InflateManager(bool expectRfc1950HeaderBytes)
			{
				this._handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
			}

			internal bool HandleRfc1950HeaderBytes
			{
				get
				{
					return this._handleRfc1950HeaderBytes;
				}
			}

			internal int Reset()
			{
				this._codec.TotalBytesIn = (this._codec.TotalBytesOut = 0L);
				this._codec.Message = null;
				this.mode = (this.HandleRfc1950HeaderBytes ? InflateManager.InflateManagerMode.METHOD : InflateManager.InflateManagerMode.BLOCKS);
				this.blocks.Reset();
				return 0;
			}

			internal int End()
			{
				if (this.blocks != null)
				{
					this.blocks.Free();
				}
				this.blocks = null;
				return 0;
			}

			internal int Initialize(ZlibCodec codec, int w)
			{
				this._codec = codec;
				this._codec.Message = null;
				this.blocks = null;
				if (w < 8 || w > 15)
				{
					this.End();
					throw new ZlibException("Bad window size.");
				}
				this.wbits = w;
				this.blocks = new InflateBlocks(codec, this.HandleRfc1950HeaderBytes ? this : null, 1 << w);
				this.Reset();
				return 0;
			}

			internal int Inflate(FlushType flush)
			{
				if (this._codec.InputBuffer == null)
				{
					throw new ZlibException("InputBuffer is null. ");
				}
				int num = 0;
				int num2 = -5;
				for (; ; )
				{
					switch (this.mode)
					{
						case InflateManager.InflateManagerMode.METHOD:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_3;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							if (((this.method = (int)this._codec.InputBuffer[this._codec.NextIn++]) & 15) != 8)
							{
								this.mode = InflateManager.InflateManagerMode.BAD;
								this._codec.Message = string.Format("unknown compression method (0x{0:X2})", this.method);
								this.marker = 5;
								continue;
							}
							if ((this.method >> 4) + 8 > this.wbits)
							{
								this.mode = InflateManager.InflateManagerMode.BAD;
								this._codec.Message = string.Format("invalid window size ({0})", (this.method >> 4) + 8);
								this.marker = 5;
								continue;
							}
							this.mode = InflateManager.InflateManagerMode.FLAG;
							continue;
						case InflateManager.InflateManagerMode.FLAG:
							{
								if (this._codec.AvailableBytesIn == 0)
								{
									goto Block_6;
								}
								num2 = num;
								this._codec.AvailableBytesIn--;
								this._codec.TotalBytesIn += 1L;
								int num3 = (int)(this._codec.InputBuffer[this._codec.NextIn++] & byte.MaxValue);
								if (((this.method << 8) + num3) % 31 != 0)
								{
									this.mode = InflateManager.InflateManagerMode.BAD;
									this._codec.Message = "incorrect header check";
									this.marker = 5;
									continue;
								}
								this.mode = (((num3 & 32) == 0) ? InflateManager.InflateManagerMode.BLOCKS : InflateManager.InflateManagerMode.DICT4);
								continue;
							}
						case InflateManager.InflateManagerMode.DICT4:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_9;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck = (uint)((long)((long)this._codec.InputBuffer[this._codec.NextIn++] << 24) & unchecked((long)((ulong)(-16777216))));
							this.mode = InflateManager.InflateManagerMode.DICT3;
							continue;
						case InflateManager.InflateManagerMode.DICT3:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_10;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck += (uint)(((int)this._codec.InputBuffer[this._codec.NextIn++] << 16) & 16711680);
							this.mode = InflateManager.InflateManagerMode.DICT2;
							continue;
						case InflateManager.InflateManagerMode.DICT2:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_11;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck += (uint)(((int)this._codec.InputBuffer[this._codec.NextIn++] << 8) & 65280);
							this.mode = InflateManager.InflateManagerMode.DICT1;
							continue;
						case InflateManager.InflateManagerMode.DICT1:
							goto IL_3F6;
						case InflateManager.InflateManagerMode.DICT0:
							goto IL_493;
						case InflateManager.InflateManagerMode.BLOCKS:
							num2 = this.blocks.Process(num2);
							if (num2 == -3)
							{
								this.mode = InflateManager.InflateManagerMode.BAD;
								this.marker = 0;
								continue;
							}
							if (num2 == 0)
							{
								num2 = num;
							}
							if (num2 != 1)
							{
								goto Block_15;
							}
							num2 = num;
							this.computedCheck = this.blocks.Reset();
							if (!this.HandleRfc1950HeaderBytes)
							{
								goto Block_16;
							}
							this.mode = InflateManager.InflateManagerMode.CHECK4;
							continue;
						case InflateManager.InflateManagerMode.CHECK4:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_17;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck = (uint)((long)((long)this._codec.InputBuffer[this._codec.NextIn++] << 24) & unchecked((long)((ulong)(-16777216))));
							this.mode = InflateManager.InflateManagerMode.CHECK3;
							continue;
						case InflateManager.InflateManagerMode.CHECK3:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_18;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck += (uint)(((int)this._codec.InputBuffer[this._codec.NextIn++] << 16) & 16711680);
							this.mode = InflateManager.InflateManagerMode.CHECK2;
							continue;
						case InflateManager.InflateManagerMode.CHECK2:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_19;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck += (uint)(((int)this._codec.InputBuffer[this._codec.NextIn++] << 8) & 65280);
							this.mode = InflateManager.InflateManagerMode.CHECK1;
							continue;
						case InflateManager.InflateManagerMode.CHECK1:
							if (this._codec.AvailableBytesIn == 0)
							{
								goto Block_20;
							}
							num2 = num;
							this._codec.AvailableBytesIn--;
							this._codec.TotalBytesIn += 1L;
							this.expectedCheck += (uint)(this._codec.InputBuffer[this._codec.NextIn++] & byte.MaxValue);
							if (this.computedCheck != this.expectedCheck)
							{
								this.mode = InflateManager.InflateManagerMode.BAD;
								this._codec.Message = "incorrect data check";
								this.marker = 5;
								continue;
							}
							goto IL_79E;
						case InflateManager.InflateManagerMode.DONE:
							goto IL_7AA;
						case InflateManager.InflateManagerMode.BAD:
							goto IL_7AE;
					}
					break;
				}
				throw new ZlibException("Stream error.");
			Block_3:
				return num2;
			Block_6:
				return num2;
			Block_9:
				return num2;
			Block_10:
				return num2;
			Block_11:
				return num2;
			IL_3F6:
				if (this._codec.AvailableBytesIn == 0)
				{
					return num2;
				}
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn += 1L;
				this.expectedCheck += (uint)(this._codec.InputBuffer[this._codec.NextIn++] & byte.MaxValue);
				this._codec._Adler32 = this.expectedCheck;
				this.mode = InflateManager.InflateManagerMode.DICT0;
				return 2;
			IL_493:
				this.mode = InflateManager.InflateManagerMode.BAD;
				this._codec.Message = "need dictionary";
				this.marker = 0;
				return -2;
			Block_15:
				return num2;
			Block_16:
				this.mode = InflateManager.InflateManagerMode.DONE;
				return 1;
			Block_17:
				return num2;
			Block_18:
				return num2;
			Block_19:
				return num2;
			Block_20:
				return num2;
			IL_79E:
				this.mode = InflateManager.InflateManagerMode.DONE;
				return 1;
			IL_7AA:
				return 1;
			IL_7AE:
				throw new ZlibException(string.Format("Bad state ({0})", this._codec.Message));
			}

			private const int PRESET_DICT = 32;

			private const int Z_DEFLATED = 8;

			private InflateManager.InflateManagerMode mode;

			internal ZlibCodec _codec;

			internal int method;

			internal uint computedCheck;

			internal uint expectedCheck;

			internal int marker;

			private bool _handleRfc1950HeaderBytes = true;

			internal int wbits;

			internal InflateBlocks blocks;

			private static readonly byte[] mark = new byte[] { 0, 0, byte.MaxValue, byte.MaxValue };

			private enum InflateManagerMode
			{
				METHOD,
				FLAG,
				DICT4,
				DICT3,
				DICT2,
				DICT1,
				DICT0,
				BLOCKS,
				CHECK4,
				CHECK3,
				CHECK2,
				CHECK1,
				DONE,
				BAD
			}
		}

		internal sealed class InfTree
		{
			private int huft_build(int[] b, int bindex, int n, int s, int[] d, int[] e, int[] t, int[] m, int[] hp, int[] hn, int[] v)
			{
				int num = 0;
				int num2 = n;
				do
				{
					this.c[b[bindex + num]]++;
					num++;
					num2--;
				}
				while (num2 != 0);
				int result;
				if (this.c[0] == n)
				{
					t[0] = -1;
					m[0] = 0;
					result = 0;
				}
				else
				{
					int num3 = m[0];
					int i;
					for (i = 1; i <= 15; i++)
					{
						if (this.c[i] != 0)
						{
							break;
						}
					}
					int j = i;
					if (num3 < i)
					{
						num3 = i;
					}
					for (num2 = 15; num2 != 0; num2--)
					{
						if (this.c[num2] != 0)
						{
							break;
						}
					}
					int num4 = num2;
					if (num3 > num2)
					{
						num3 = num2;
					}
					m[0] = num3;
					int num5 = 1 << i;
					while (i < num2)
					{
						if ((num5 -= this.c[i]) < 0)
						{
							return -3;
						}
						i++;
						num5 <<= 1;
					}
					if ((num5 -= this.c[num2]) < 0)
					{
						result = -3;
					}
					else
					{
						this.c[num2] += num5;
						i = (this.x[1] = 0);
						num = 1;
						int num6 = 2;
						while (--num2 != 0)
						{
							i = (this.x[num6] = i + this.c[num]);
							num6++;
							num++;
						}
						num2 = 0;
						num = 0;
						do
						{
							if ((i = b[bindex + num]) != 0)
							{
								v[this.x[i]++] = num2;
							}
							num++;
						}
						while (++num2 < n);
						n = this.x[num4];
						num2 = (this.x[0] = 0);
						num = 0;
						int num7 = -1;
						int num8 = -num3;
						this.u[0] = 0;
						int num9 = 0;
						int num10 = 0;
						while (j <= num4)
						{
							int num11 = this.c[j];
							while (num11-- != 0)
							{
								int num12;
								while (j > num8 + num3)
								{
									num7++;
									num8 += num3;
									num10 = num4 - num8;
									num10 = ((num10 > num3) ? num3 : num10);
									if ((num12 = 1 << ((i = j - num8) & 31)) > num11 + 1)
									{
										num12 -= num11 + 1;
										num6 = j;
										if (i < num10)
										{
											while (++i < num10)
											{
												if ((num12 <<= 1) <= this.c[++num6])
												{
													break;
												}
												num12 -= this.c[num6];
											}
										}
									}
									num10 = 1 << i;
									if (hn[0] + num10 > 1440)
									{
										return -3;
									}
									num9 = (this.u[num7] = hn[0]);
									hn[0] += num10;
									if (num7 != 0)
									{
										this.x[num7] = num2;
										this.r[0] = (int)((sbyte)i);
										this.r[1] = (int)((sbyte)num3);
										i = SharedUtils.URShift(num2, num8 - num3);
										this.r[2] = num9 - this.u[num7 - 1] - i;
										Array.Copy(this.r, 0, hp, (this.u[num7 - 1] + i) * 3, 3);
									}
									else
									{
										t[0] = num9;
									}
								}
								this.r[1] = (int)((sbyte)(j - num8));
								if (num >= n)
								{
									this.r[0] = 192;
								}
								else if (v[num] < s)
								{
									this.r[0] = (int)((v[num] < 256) ? 0 : 96);
									this.r[2] = v[num++];
								}
								else
								{
									this.r[0] = (int)((sbyte)(e[v[num] - s] + 16 + 64));
									this.r[2] = d[v[num++] - s];
								}
								num12 = 1 << j - num8;
								for (i = SharedUtils.URShift(num2, num8); i < num10; i += num12)
								{
									Array.Copy(this.r, 0, hp, (num9 + i) * 3, 3);
								}
								i = 1 << j - 1;
								while ((num2 & i) != 0)
								{
									num2 ^= i;
									i = SharedUtils.URShift(i, 1);
								}
								num2 ^= i;
								int num13 = (1 << num8) - 1;
								while ((num2 & num13) != this.x[num7])
								{
									num7--;
									num8 -= num3;
									num13 = (1 << num8) - 1;
								}
							}
							j++;
						}
						result = ((num5 != 0 && num4 != 1) ? (-5) : 0);
					}
				}
				return result;
			}

			internal int inflate_trees_bits(int[] c, int[] bb, int[] tb, int[] hp, ZlibCodec z)
			{
				this.initWorkArea(19);
				this.hn[0] = 0;
				int num = this.huft_build(c, 0, 19, 19, null, null, tb, bb, hp, this.hn, this.v);
				if (num == -3)
				{
					z.Message = "oversubscribed dynamic bit lengths tree";
				}
				else if (num == -5 || bb[0] == 0)
				{
					z.Message = "incomplete dynamic bit lengths tree";
					num = -3;
				}
				return num;
			}

			internal int inflate_trees_dynamic(int nl, int nd, int[] c, int[] bl, int[] bd, int[] tl, int[] td, int[] hp, ZlibCodec z)
			{
				this.initWorkArea(288);
				this.hn[0] = 0;
				int num = this.huft_build(c, 0, nl, 257, InfTree.cplens, InfTree.cplext, tl, bl, hp, this.hn, this.v);
				int result;
				if (num != 0 || bl[0] == 0)
				{
					if (num == -3)
					{
						z.Message = "oversubscribed literal/length tree";
					}
					else if (num != -4)
					{
						z.Message = "incomplete literal/length tree";
						num = -3;
					}
					result = num;
				}
				else
				{
					this.initWorkArea(288);
					num = this.huft_build(c, nl, nd, 0, InfTree.cpdist, InfTree.cpdext, td, bd, hp, this.hn, this.v);
					if (num != 0 || (bd[0] == 0 && nl > 257))
					{
						if (num == -3)
						{
							z.Message = "oversubscribed distance tree";
						}
						else if (num == -5)
						{
							z.Message = "incomplete distance tree";
							num = -3;
						}
						else if (num != -4)
						{
							z.Message = "empty distance tree with lengths";
							num = -3;
						}
						result = num;
					}
					else
					{
						result = 0;
					}
				}
				return result;
			}

			internal static int inflate_trees_fixed(int[] bl, int[] bd, int[][] tl, int[][] td, ZlibCodec z)
			{
				bl[0] = 9;
				bd[0] = 5;
				tl[0] = InfTree.fixed_tl;
				td[0] = InfTree.fixed_td;
				return 0;
			}

			private void initWorkArea(int vsize)
			{
				if (this.hn == null)
				{
					this.hn = new int[1];
					this.v = new int[vsize];
					this.c = new int[16];
					this.r = new int[3];
					this.u = new int[15];
					this.x = new int[16];
				}
				else
				{
					if (this.v.Length < vsize)
					{
						this.v = new int[vsize];
					}
					Array.Clear(this.v, 0, vsize);
					Array.Clear(this.c, 0, 16);
					this.r[0] = 0;
					this.r[1] = 0;
					this.r[2] = 0;
					Array.Clear(this.u, 0, 15);
					Array.Clear(this.x, 0, 16);
				}
			}

			private const int MANY = 1440;

			private const int Z_OK = 0;

			private const int Z_STREAM_END = 1;

			private const int Z_NEED_DICT = 2;

			private const int Z_ERRNO = -1;

			private const int Z_STREAM_ERROR = -2;

			private const int Z_DATA_ERROR = -3;

			private const int Z_MEM_ERROR = -4;

			private const int Z_BUF_ERROR = -5;

			private const int Z_VERSION_ERROR = -6;

			internal const int fixed_bl = 9;

			internal const int fixed_bd = 5;

			internal const int BMAX = 15;

			internal static readonly int[] fixed_tl = new int[]
			{
			96, 7, 256, 0, 8, 80, 0, 8, 16, 84,
			8, 115, 82, 7, 31, 0, 8, 112, 0, 8,
			48, 0, 9, 192, 80, 7, 10, 0, 8, 96,
			0, 8, 32, 0, 9, 160, 0, 8, 0, 0,
			8, 128, 0, 8, 64, 0, 9, 224, 80, 7,
			6, 0, 8, 88, 0, 8, 24, 0, 9, 144,
			83, 7, 59, 0, 8, 120, 0, 8, 56, 0,
			9, 208, 81, 7, 17, 0, 8, 104, 0, 8,
			40, 0, 9, 176, 0, 8, 8, 0, 8, 136,
			0, 8, 72, 0, 9, 240, 80, 7, 4, 0,
			8, 84, 0, 8, 20, 85, 8, 227, 83, 7,
			43, 0, 8, 116, 0, 8, 52, 0, 9, 200,
			81, 7, 13, 0, 8, 100, 0, 8, 36, 0,
			9, 168, 0, 8, 4, 0, 8, 132, 0, 8,
			68, 0, 9, 232, 80, 7, 8, 0, 8, 92,
			0, 8, 28, 0, 9, 152, 84, 7, 83, 0,
			8, 124, 0, 8, 60, 0, 9, 216, 82, 7,
			23, 0, 8, 108, 0, 8, 44, 0, 9, 184,
			0, 8, 12, 0, 8, 140, 0, 8, 76, 0,
			9, 248, 80, 7, 3, 0, 8, 82, 0, 8,
			18, 85, 8, 163, 83, 7, 35, 0, 8, 114,
			0, 8, 50, 0, 9, 196, 81, 7, 11, 0,
			8, 98, 0, 8, 34, 0, 9, 164, 0, 8,
			2, 0, 8, 130, 0, 8, 66, 0, 9, 228,
			80, 7, 7, 0, 8, 90, 0, 8, 26, 0,
			9, 148, 84, 7, 67, 0, 8, 122, 0, 8,
			58, 0, 9, 212, 82, 7, 19, 0, 8, 106,
			0, 8, 42, 0, 9, 180, 0, 8, 10, 0,
			8, 138, 0, 8, 74, 0, 9, 244, 80, 7,
			5, 0, 8, 86, 0, 8, 22, 192, 8, 0,
			83, 7, 51, 0, 8, 118, 0, 8, 54, 0,
			9, 204, 81, 7, 15, 0, 8, 102, 0, 8,
			38, 0, 9, 172, 0, 8, 6, 0, 8, 134,
			0, 8, 70, 0, 9, 236, 80, 7, 9, 0,
			8, 94, 0, 8, 30, 0, 9, 156, 84, 7,
			99, 0, 8, 126, 0, 8, 62, 0, 9, 220,
			82, 7, 27, 0, 8, 110, 0, 8, 46, 0,
			9, 188, 0, 8, 14, 0, 8, 142, 0, 8,
			78, 0, 9, 252, 96, 7, 256, 0, 8, 81,
			0, 8, 17, 85, 8, 131, 82, 7, 31, 0,
			8, 113, 0, 8, 49, 0, 9, 194, 80, 7,
			10, 0, 8, 97, 0, 8, 33, 0, 9, 162,
			0, 8, 1, 0, 8, 129, 0, 8, 65, 0,
			9, 226, 80, 7, 6, 0, 8, 89, 0, 8,
			25, 0, 9, 146, 83, 7, 59, 0, 8, 121,
			0, 8, 57, 0, 9, 210, 81, 7, 17, 0,
			8, 105, 0, 8, 41, 0, 9, 178, 0, 8,
			9, 0, 8, 137, 0, 8, 73, 0, 9, 242,
			80, 7, 4, 0, 8, 85, 0, 8, 21, 80,
			8, 258, 83, 7, 43, 0, 8, 117, 0, 8,
			53, 0, 9, 202, 81, 7, 13, 0, 8, 101,
			0, 8, 37, 0, 9, 170, 0, 8, 5, 0,
			8, 133, 0, 8, 69, 0, 9, 234, 80, 7,
			8, 0, 8, 93, 0, 8, 29, 0, 9, 154,
			84, 7, 83, 0, 8, 125, 0, 8, 61, 0,
			9, 218, 82, 7, 23, 0, 8, 109, 0, 8,
			45, 0, 9, 186, 0, 8, 13, 0, 8, 141,
			0, 8, 77, 0, 9, 250, 80, 7, 3, 0,
			8, 83, 0, 8, 19, 85, 8, 195, 83, 7,
			35, 0, 8, 115, 0, 8, 51, 0, 9, 198,
			81, 7, 11, 0, 8, 99, 0, 8, 35, 0,
			9, 166, 0, 8, 3, 0, 8, 131, 0, 8,
			67, 0, 9, 230, 80, 7, 7, 0, 8, 91,
			0, 8, 27, 0, 9, 150, 84, 7, 67, 0,
			8, 123, 0, 8, 59, 0, 9, 214, 82, 7,
			19, 0, 8, 107, 0, 8, 43, 0, 9, 182,
			0, 8, 11, 0, 8, 139, 0, 8, 75, 0,
			9, 246, 80, 7, 5, 0, 8, 87, 0, 8,
			23, 192, 8, 0, 83, 7, 51, 0, 8, 119,
			0, 8, 55, 0, 9, 206, 81, 7, 15, 0,
			8, 103, 0, 8, 39, 0, 9, 174, 0, 8,
			7, 0, 8, 135, 0, 8, 71, 0, 9, 238,
			80, 7, 9, 0, 8, 95, 0, 8, 31, 0,
			9, 158, 84, 7, 99, 0, 8, 127, 0, 8,
			63, 0, 9, 222, 82, 7, 27, 0, 8, 111,
			0, 8, 47, 0, 9, 190, 0, 8, 15, 0,
			8, 143, 0, 8, 79, 0, 9, 254, 96, 7,
			256, 0, 8, 80, 0, 8, 16, 84, 8, 115,
			82, 7, 31, 0, 8, 112, 0, 8, 48, 0,
			9, 193, 80, 7, 10, 0, 8, 96, 0, 8,
			32, 0, 9, 161, 0, 8, 0, 0, 8, 128,
			0, 8, 64, 0, 9, 225, 80, 7, 6, 0,
			8, 88, 0, 8, 24, 0, 9, 145, 83, 7,
			59, 0, 8, 120, 0, 8, 56, 0, 9, 209,
			81, 7, 17, 0, 8, 104, 0, 8, 40, 0,
			9, 177, 0, 8, 8, 0, 8, 136, 0, 8,
			72, 0, 9, 241, 80, 7, 4, 0, 8, 84,
			0, 8, 20, 85, 8, 227, 83, 7, 43, 0,
			8, 116, 0, 8, 52, 0, 9, 201, 81, 7,
			13, 0, 8, 100, 0, 8, 36, 0, 9, 169,
			0, 8, 4, 0, 8, 132, 0, 8, 68, 0,
			9, 233, 80, 7, 8, 0, 8, 92, 0, 8,
			28, 0, 9, 153, 84, 7, 83, 0, 8, 124,
			0, 8, 60, 0, 9, 217, 82, 7, 23, 0,
			8, 108, 0, 8, 44, 0, 9, 185, 0, 8,
			12, 0, 8, 140, 0, 8, 76, 0, 9, 249,
			80, 7, 3, 0, 8, 82, 0, 8, 18, 85,
			8, 163, 83, 7, 35, 0, 8, 114, 0, 8,
			50, 0, 9, 197, 81, 7, 11, 0, 8, 98,
			0, 8, 34, 0, 9, 165, 0, 8, 2, 0,
			8, 130, 0, 8, 66, 0, 9, 229, 80, 7,
			7, 0, 8, 90, 0, 8, 26, 0, 9, 149,
			84, 7, 67, 0, 8, 122, 0, 8, 58, 0,
			9, 213, 82, 7, 19, 0, 8, 106, 0, 8,
			42, 0, 9, 181, 0, 8, 10, 0, 8, 138,
			0, 8, 74, 0, 9, 245, 80, 7, 5, 0,
			8, 86, 0, 8, 22, 192, 8, 0, 83, 7,
			51, 0, 8, 118, 0, 8, 54, 0, 9, 205,
			81, 7, 15, 0, 8, 102, 0, 8, 38, 0,
			9, 173, 0, 8, 6, 0, 8, 134, 0, 8,
			70, 0, 9, 237, 80, 7, 9, 0, 8, 94,
			0, 8, 30, 0, 9, 157, 84, 7, 99, 0,
			8, 126, 0, 8, 62, 0, 9, 221, 82, 7,
			27, 0, 8, 110, 0, 8, 46, 0, 9, 189,
			0, 8, 14, 0, 8, 142, 0, 8, 78, 0,
			9, 253, 96, 7, 256, 0, 8, 81, 0, 8,
			17, 85, 8, 131, 82, 7, 31, 0, 8, 113,
			0, 8, 49, 0, 9, 195, 80, 7, 10, 0,
			8, 97, 0, 8, 33, 0, 9, 163, 0, 8,
			1, 0, 8, 129, 0, 8, 65, 0, 9, 227,
			80, 7, 6, 0, 8, 89, 0, 8, 25, 0,
			9, 147, 83, 7, 59, 0, 8, 121, 0, 8,
			57, 0, 9, 211, 81, 7, 17, 0, 8, 105,
			0, 8, 41, 0, 9, 179, 0, 8, 9, 0,
			8, 137, 0, 8, 73, 0, 9, 243, 80, 7,
			4, 0, 8, 85, 0, 8, 21, 80, 8, 258,
			83, 7, 43, 0, 8, 117, 0, 8, 53, 0,
			9, 203, 81, 7, 13, 0, 8, 101, 0, 8,
			37, 0, 9, 171, 0, 8, 5, 0, 8, 133,
			0, 8, 69, 0, 9, 235, 80, 7, 8, 0,
			8, 93, 0, 8, 29, 0, 9, 155, 84, 7,
			83, 0, 8, 125, 0, 8, 61, 0, 9, 219,
			82, 7, 23, 0, 8, 109, 0, 8, 45, 0,
			9, 187, 0, 8, 13, 0, 8, 141, 0, 8,
			77, 0, 9, 251, 80, 7, 3, 0, 8, 83,
			0, 8, 19, 85, 8, 195, 83, 7, 35, 0,
			8, 115, 0, 8, 51, 0, 9, 199, 81, 7,
			11, 0, 8, 99, 0, 8, 35, 0, 9, 167,
			0, 8, 3, 0, 8, 131, 0, 8, 67, 0,
			9, 231, 80, 7, 7, 0, 8, 91, 0, 8,
			27, 0, 9, 151, 84, 7, 67, 0, 8, 123,
			0, 8, 59, 0, 9, 215, 82, 7, 19, 0,
			8, 107, 0, 8, 43, 0, 9, 183, 0, 8,
			11, 0, 8, 139, 0, 8, 75, 0, 9, 247,
			80, 7, 5, 0, 8, 87, 0, 8, 23, 192,
			8, 0, 83, 7, 51, 0, 8, 119, 0, 8,
			55, 0, 9, 207, 81, 7, 15, 0, 8, 103,
			0, 8, 39, 0, 9, 175, 0, 8, 7, 0,
			8, 135, 0, 8, 71, 0, 9, 239, 80, 7,
			9, 0, 8, 95, 0, 8, 31, 0, 9, 159,
			84, 7, 99, 0, 8, 127, 0, 8, 63, 0,
			9, 223, 82, 7, 27, 0, 8, 111, 0, 8,
			47, 0, 9, 191, 0, 8, 15, 0, 8, 143,
			0, 8, 79, 0, 9, 255
			};

			internal static readonly int[] fixed_td = new int[]
			{
			80, 5, 1, 87, 5, 257, 83, 5, 17, 91,
			5, 4097, 81, 5, 5, 89, 5, 1025, 85, 5,
			65, 93, 5, 16385, 80, 5, 3, 88, 5, 513,
			84, 5, 33, 92, 5, 8193, 82, 5, 9, 90,
			5, 2049, 86, 5, 129, 192, 5, 24577, 80, 5,
			2, 87, 5, 385, 83, 5, 25, 91, 5, 6145,
			81, 5, 7, 89, 5, 1537, 85, 5, 97, 93,
			5, 24577, 80, 5, 4, 88, 5, 769, 84, 5,
			49, 92, 5, 12289, 82, 5, 13, 90, 5, 3073,
			86, 5, 193, 192, 5, 24577
			};

			internal static readonly int[] cplens = new int[]
			{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
			15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
			67, 83, 99, 115, 131, 163, 195, 227, 258, 0,
			0
			};

			internal static readonly int[] cplext = new int[]
			{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
			4, 4, 4, 4, 5, 5, 5, 5, 0, 112,
			112
			};

			internal static readonly int[] cpdist = new int[]
			{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
			33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
			1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
			};

			internal static readonly int[] cpdext = new int[]
			{
			0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
			4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
			9, 9, 10, 10, 11, 11, 12, 12, 13, 13
			};

			internal int[] hn = null;

			internal int[] v = null;

			internal int[] c = null;

			internal int[] r = null;

			internal int[] u = null;

			internal int[] x = null;
		}

		internal static class InternalConstants
		{
			internal static readonly int MAX_BITS = 15;

			internal static readonly int BL_CODES = 19;

			internal static readonly int D_CODES = 30;

			internal static readonly int LITERALS = 256;

			internal static readonly int LENGTH_CODES = 29;

			internal static readonly int L_CODES = InternalConstants.LITERALS + 1 + InternalConstants.LENGTH_CODES;

			internal static readonly int MAX_BL_BITS = 7;

			internal static readonly int REP_3_6 = 16;

			internal static readonly int REPZ_3_10 = 17;

			internal static readonly int REPZ_11_138 = 18;
		}

		internal static class InternalInflateConstants
		{
			internal static readonly int[] InflateMask = new int[]
			{
			0, 1, 3, 7, 15, 31, 63, 127, 255, 511,
			1023, 2047, 4095, 8191, 16383, 32767, 65535
			};
		}

		internal class SharedUtils
		{
			public static int URShift(int number, int bits)
			{
				return (int)((uint)number >> bits);
			}
		}

		internal sealed class StaticTree
		{
			private StaticTree(short[] treeCodes, int[] extraBits, int extraBase, int elems, int maxLength)
			{
				this.treeCodes = treeCodes;
				this.extraBits = extraBits;
				this.extraBase = extraBase;
				this.elems = elems;
				this.maxLength = maxLength;
			}

			internal static readonly short[] lengthAndLiteralsTreeCodes = new short[]
			{
			12, 8, 140, 8, 76, 8, 204, 8, 44, 8,
			172, 8, 108, 8, 236, 8, 28, 8, 156, 8,
			92, 8, 220, 8, 60, 8, 188, 8, 124, 8,
			252, 8, 2, 8, 130, 8, 66, 8, 194, 8,
			34, 8, 162, 8, 98, 8, 226, 8, 18, 8,
			146, 8, 82, 8, 210, 8, 50, 8, 178, 8,
			114, 8, 242, 8, 10, 8, 138, 8, 74, 8,
			202, 8, 42, 8, 170, 8, 106, 8, 234, 8,
			26, 8, 154, 8, 90, 8, 218, 8, 58, 8,
			186, 8, 122, 8, 250, 8, 6, 8, 134, 8,
			70, 8, 198, 8, 38, 8, 166, 8, 102, 8,
			230, 8, 22, 8, 150, 8, 86, 8, 214, 8,
			54, 8, 182, 8, 118, 8, 246, 8, 14, 8,
			142, 8, 78, 8, 206, 8, 46, 8, 174, 8,
			110, 8, 238, 8, 30, 8, 158, 8, 94, 8,
			222, 8, 62, 8, 190, 8, 126, 8, 254, 8,
			1, 8, 129, 8, 65, 8, 193, 8, 33, 8,
			161, 8, 97, 8, 225, 8, 17, 8, 145, 8,
			81, 8, 209, 8, 49, 8, 177, 8, 113, 8,
			241, 8, 9, 8, 137, 8, 73, 8, 201, 8,
			41, 8, 169, 8, 105, 8, 233, 8, 25, 8,
			153, 8, 89, 8, 217, 8, 57, 8, 185, 8,
			121, 8, 249, 8, 5, 8, 133, 8, 69, 8,
			197, 8, 37, 8, 165, 8, 101, 8, 229, 8,
			21, 8, 149, 8, 85, 8, 213, 8, 53, 8,
			181, 8, 117, 8, 245, 8, 13, 8, 141, 8,
			77, 8, 205, 8, 45, 8, 173, 8, 109, 8,
			237, 8, 29, 8, 157, 8, 93, 8, 221, 8,
			61, 8, 189, 8, 125, 8, 253, 8, 19, 9,
			275, 9, 147, 9, 403, 9, 83, 9, 339, 9,
			211, 9, 467, 9, 51, 9, 307, 9, 179, 9,
			435, 9, 115, 9, 371, 9, 243, 9, 499, 9,
			11, 9, 267, 9, 139, 9, 395, 9, 75, 9,
			331, 9, 203, 9, 459, 9, 43, 9, 299, 9,
			171, 9, 427, 9, 107, 9, 363, 9, 235, 9,
			491, 9, 27, 9, 283, 9, 155, 9, 411, 9,
			91, 9, 347, 9, 219, 9, 475, 9, 59, 9,
			315, 9, 187, 9, 443, 9, 123, 9, 379, 9,
			251, 9, 507, 9, 7, 9, 263, 9, 135, 9,
			391, 9, 71, 9, 327, 9, 199, 9, 455, 9,
			39, 9, 295, 9, 167, 9, 423, 9, 103, 9,
			359, 9, 231, 9, 487, 9, 23, 9, 279, 9,
			151, 9, 407, 9, 87, 9, 343, 9, 215, 9,
			471, 9, 55, 9, 311, 9, 183, 9, 439, 9,
			119, 9, 375, 9, 247, 9, 503, 9, 15, 9,
			271, 9, 143, 9, 399, 9, 79, 9, 335, 9,
			207, 9, 463, 9, 47, 9, 303, 9, 175, 9,
			431, 9, 111, 9, 367, 9, 239, 9, 495, 9,
			31, 9, 287, 9, 159, 9, 415, 9, 95, 9,
			351, 9, 223, 9, 479, 9, 63, 9, 319, 9,
			191, 9, 447, 9, 127, 9, 383, 9, 255, 9,
			511, 9, 0, 7, 64, 7, 32, 7, 96, 7,
			16, 7, 80, 7, 48, 7, 112, 7, 8, 7,
			72, 7, 40, 7, 104, 7, 24, 7, 88, 7,
			56, 7, 120, 7, 4, 7, 68, 7, 36, 7,
			100, 7, 20, 7, 84, 7, 52, 7, 116, 7,
			3, 8, 131, 8, 67, 8, 195, 8, 35, 8,
			163, 8, 99, 8, 227, 8
			};

			internal static readonly short[] distTreeCodes = new short[]
			{
			0, 5, 16, 5, 8, 5, 24, 5, 4, 5,
			20, 5, 12, 5, 28, 5, 2, 5, 18, 5,
			10, 5, 26, 5, 6, 5, 22, 5, 14, 5,
			30, 5, 1, 5, 17, 5, 9, 5, 25, 5,
			5, 5, 21, 5, 13, 5, 29, 5, 3, 5,
			19, 5, 11, 5, 27, 5, 7, 5, 23, 5
			};

			internal static readonly StaticTree Literals = new StaticTree(StaticTree.lengthAndLiteralsTreeCodes, Tree.ExtraLengthBits, InternalConstants.LITERALS + 1, InternalConstants.L_CODES, InternalConstants.MAX_BITS);

			internal static readonly StaticTree Distances = new StaticTree(StaticTree.distTreeCodes, Tree.ExtraDistanceBits, 0, InternalConstants.D_CODES, InternalConstants.MAX_BITS);

			internal static readonly StaticTree BitLengths = new StaticTree(null, Tree.extra_blbits, 0, InternalConstants.BL_CODES, InternalConstants.MAX_BL_BITS);

			internal short[] treeCodes;

			internal int[] extraBits;

			internal int extraBase;

			internal int elems;

			internal int maxLength;
		}

		internal sealed class Tree
		{
			internal static int DistanceCode(int dist)
			{
				return (int)((dist < 256) ? Tree._dist_code[dist] : Tree._dist_code[256 + SharedUtils.URShift(dist, 7)]);
			}

			internal void gen_bitlen(DeflateManager s)
			{
				short[] array = this.dyn_tree;
				short[] treeCodes = this.staticTree.treeCodes;
				int[] extraBits = this.staticTree.extraBits;
				int extraBase = this.staticTree.extraBase;
				int maxLength = this.staticTree.maxLength;
				int num = 0;
				for (int i = 0; i <= InternalConstants.MAX_BITS; i++)
				{
					s.bl_count[i] = 0;
				}
				array[s.heap[s.heap_max] * 2 + 1] = 0;
				int j;
				for (j = s.heap_max + 1; j < Tree.HEAP_SIZE; j++)
				{
					int num2 = s.heap[j];
					int i = (int)(array[(int)(array[num2 * 2 + 1] * 2 + 1)] + 1);
					if (i > maxLength)
					{
						i = maxLength;
						num++;
					}
					array[num2 * 2 + 1] = (short)i;
					if (num2 <= this.max_code)
					{
						short[] bl_count = s.bl_count;
						int num3 = i;
						bl_count[num3] += 1;
						int num4 = 0;
						if (num2 >= extraBase)
						{
							num4 = extraBits[num2 - extraBase];
						}
						short num5 = array[num2 * 2];
						s.opt_len += (int)num5 * (i + num4);
						if (treeCodes != null)
						{
							s.static_len += (int)num5 * ((int)treeCodes[num2 * 2 + 1] + num4);
						}
					}
				}
				if (num != 0)
				{
					do
					{
						int i = maxLength - 1;
						while (s.bl_count[i] == 0)
						{
							i--;
						}
						short[] bl_count2 = s.bl_count;
						int num6 = i;
						bl_count2[num6] -= 1;
						s.bl_count[i + 1] = (short)(s.bl_count[i + 1] + 2);
						short[] bl_count3 = s.bl_count;
						int num7 = maxLength;
						bl_count3[num7] -= 1;
						num -= 2;
					}
					while (num > 0);
					for (int i = maxLength; i != 0; i--)
					{
						int num2 = (int)s.bl_count[i];
						while (num2 != 0)
						{
							int num8 = s.heap[--j];
							if (num8 <= this.max_code)
							{
								if ((int)array[num8 * 2 + 1] != i)
								{
									s.opt_len = (int)((long)s.opt_len + ((long)i - (long)array[num8 * 2 + 1]) * (long)array[num8 * 2]);
									array[num8 * 2 + 1] = (short)i;
								}
								num2--;
							}
						}
					}
				}
			}

			internal void build_tree(DeflateManager s)
			{
				short[] array = this.dyn_tree;
				short[] treeCodes = this.staticTree.treeCodes;
				int elems = this.staticTree.elems;
				int num = -1;
				s.heap_len = 0;
				s.heap_max = Tree.HEAP_SIZE;
				for (int i = 0; i < elems; i++)
				{
					if (array[i * 2] != 0)
					{
						num = (s.heap[++s.heap_len] = i);
						s.depth[i] = 0;
					}
					else
					{
						array[i * 2 + 1] = 0;
					}
				}
				int num2;
				while (s.heap_len < 2)
				{
					num2 = (s.heap[++s.heap_len] = ((num < 2) ? (++num) : 0));
					array[num2 * 2] = 1;
					s.depth[num2] = 0;
					s.opt_len--;
					if (treeCodes != null)
					{
						s.static_len -= (int)treeCodes[num2 * 2 + 1];
					}
				}
				this.max_code = num;
				for (int i = s.heap_len / 2; i >= 1; i--)
				{
					s.pqdownheap(array, i);
				}
				num2 = elems;
				do
				{
					int i = s.heap[1];
					s.heap[1] = s.heap[s.heap_len--];
					s.pqdownheap(array, 1);
					int num3 = s.heap[1];
					s.heap[--s.heap_max] = i;
					s.heap[--s.heap_max] = num3;
					array[num2 * 2] = (short)(array[i * 2] + array[num3 * 2]);
					s.depth[num2] = (sbyte)(Math.Max((byte)s.depth[i], (byte)s.depth[num3]) + 1);
					array[i * 2 + 1] = (array[num3 * 2 + 1] = (short)num2);
					s.heap[1] = num2++;
					s.pqdownheap(array, 1);
				}
				while (s.heap_len >= 2);
				s.heap[--s.heap_max] = s.heap[1];
				this.gen_bitlen(s);
				Tree.gen_codes(array, num, s.bl_count);
			}

			internal static void gen_codes(short[] tree, int max_code, short[] bl_count)
			{
				short[] array = new short[InternalConstants.MAX_BITS + 1];
				short num = 0;
				for (int i = 1; i <= InternalConstants.MAX_BITS; i++)
				{
					num = (array[i] = (short)(num + bl_count[i - 1] << 1));
				}
				for (int j = 0; j <= max_code; j++)
				{
					int num2 = (int)tree[j * 2 + 1];
					if (num2 != 0)
					{
						int num3 = j * 2;
						short[] array2 = array;
						int num4 = num2;
						short code;
						array2[num4] = (short)((code = array2[num4]) + 1);
						tree[num3] = (short)Tree.bi_reverse((int)code, num2);
					}
				}
			}

			internal static int bi_reverse(int code, int len)
			{
				int num = 0;
				do
				{
					num |= code & 1;
					code >>= 1;
					num <<= 1;
				}
				while (--len > 0);
				return num >> 1;
			}

			internal const int Buf_size = 16;

			private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

			internal static readonly int[] ExtraLengthBits = new int[]
			{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
			4, 4, 4, 4, 5, 5, 5, 5, 0
			};

			internal static readonly int[] ExtraDistanceBits = new int[]
			{
			0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
			4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
			9, 9, 10, 10, 11, 11, 12, 12, 13, 13
			};

			internal static readonly int[] extra_blbits = new int[]
			{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 2, 3, 7
			};

			internal static readonly sbyte[] bl_order = new sbyte[]
			{
			16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
			11, 4, 12, 3, 13, 2, 14, 1, 15
			};

			private static readonly sbyte[] _dist_code = new sbyte[]
			{
			0, 1, 2, 3, 4, 4, 5, 5, 6, 6,
			6, 6, 7, 7, 7, 7, 8, 8, 8, 8,
			8, 8, 8, 8, 9, 9, 9, 9, 9, 9,
			9, 9, 10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10, 11, 11,
			11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
			11, 11, 11, 11, 12, 12, 12, 12, 12, 12,
			12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
			12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
			12, 12, 12, 12, 12, 12, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
			14, 14, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 0, 0, 16, 17,
			18, 18, 19, 19, 20, 20, 20, 20, 21, 21,
			21, 21, 22, 22, 22, 22, 22, 22, 22, 22,
			23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 25, 25, 25, 25, 25, 25,
			25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
			26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 27, 27, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
			28, 28, 28, 28, 28, 28, 28, 28, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
			29, 29
			};

			internal static readonly sbyte[] LengthCode = new sbyte[]
			{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 8,
			9, 9, 10, 10, 11, 11, 12, 12, 12, 12,
			13, 13, 13, 13, 14, 14, 14, 14, 15, 15,
			15, 15, 16, 16, 16, 16, 16, 16, 16, 16,
			17, 17, 17, 17, 17, 17, 17, 17, 18, 18,
			18, 18, 18, 18, 18, 18, 19, 19, 19, 19,
			19, 19, 19, 19, 20, 20, 20, 20, 20, 20,
			20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
			21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
			21, 21, 21, 21, 21, 21, 22, 22, 22, 22,
			22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
			22, 22, 23, 23, 23, 23, 23, 23, 23, 23,
			23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
			25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
			25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
			25, 25, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
			26, 26, 26, 26, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
			27, 27, 27, 27, 27, 28
			};

			internal static readonly int[] LengthBase = new int[]
			{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 10,
			12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
			64, 80, 96, 112, 128, 160, 192, 224, 0
			};

			internal static readonly int[] DistanceBase = new int[]
			{
			0, 1, 2, 3, 4, 6, 8, 12, 16, 24,
			32, 48, 64, 96, 128, 192, 256, 384, 512, 768,
			1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576
			};

			internal short[] dyn_tree;

			internal int max_code;

			internal StaticTree staticTree;
		}

		internal class ZlibBaseStream : Stream
		{
			public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen, bool chimeraOmitCrcCheck)
			{
				this._flushMode = FlushType.None;
				this._stream = stream;
				this._leaveOpen = leaveOpen;
				this._compressionMode = compressionMode;
				this._flavor = flavor;
				this._level = level;
				this.m_chimera_omitTheCrcCheck = chimeraOmitCrcCheck;
				if (flavor == ZlibStreamFlavor.GZIP)
				{
					this.crc = new CRC32();
				}
			}

			internal int Crc32
			{
				get
				{
					int result;
					if (this.crc == null)
					{
						result = 0;
					}
					else
					{
						result = this.crc.Crc32Result;
					}
					return result;
				}
			}

			protected internal bool _wantCompress
			{
				get
				{
					return this._compressionMode == CompressionMode.Compress;
				}
			}

			private ZlibCodec z
			{
				get
				{
					if (this._z == null)
					{
						bool flag = this._flavor == ZlibStreamFlavor.ZLIB;
						this._z = new ZlibCodec();
						if (this._compressionMode == CompressionMode.Decompress)
						{
							this._z.InitializeInflate(flag);
						}
						else
						{
							this._z.Strategy = this.Strategy;
							this._z.InitializeDeflate(this._level, flag);
						}
					}
					return this._z;
				}
			}

			private byte[] workingBuffer
			{
				get
				{
					if (this._workingBuffer == null)
					{
						this._workingBuffer = new byte[this._bufferSize];
					}
					return this._workingBuffer;
				}
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (this.crc != null)
				{
					this.crc.SlurpBlock(buffer, offset, count);
				}
				if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
				{
					this._streamMode = ZlibBaseStream.StreamMode.Writer;
				}
				else if (this._streamMode != ZlibBaseStream.StreamMode.Writer)
				{
					throw new ZlibException("Cannot Write after Reading.");
				}
				if (count != 0)
				{
					this.z.InputBuffer = buffer;
					this._z.NextIn = offset;
					this._z.AvailableBytesIn = count;
					for (; ; )
					{
						this._z.OutputBuffer = this.workingBuffer;
						this._z.NextOut = 0;
						this._z.AvailableBytesOut = this._workingBuffer.Length;
						int num = (this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode));
						if (num != 0 && num != 1)
						{
							break;
						}
						this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
						bool flag = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
						if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
						{
							flag = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
						}
						if (flag)
						{
							return;
						}
					}
					throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
				}
			}

			private void finish()
			{
				if (this._z != null)
				{
					if (this._streamMode == ZlibBaseStream.StreamMode.Writer)
					{
						int num;
						for (; ; )
						{
							this._z.OutputBuffer = this.workingBuffer;
							this._z.NextOut = 0;
							this._z.AvailableBytesOut = this._workingBuffer.Length;
							num = (this._wantCompress ? this._z.Deflate(FlushType.Finish) : this._z.Inflate(FlushType.Finish));
							if (num != 1 && num != 0)
							{
								break;
							}
							if (this._workingBuffer.Length - this._z.AvailableBytesOut > 0)
							{
								this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
							}
							bool flag = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
							if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
							{
								flag = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
							}
							if (flag)
							{
								goto Block_12;
							}
						}
						string text = (this._wantCompress ? "de" : "in") + "flating";
						if (this._z.Message == null)
						{
							throw new ZlibException(string.Format("{0}: (rc = {1})", text, num));
						}
						throw new ZlibException(text + ": " + this._z.Message);
					Block_12:
						this.Flush();
						if (this._flavor == ZlibStreamFlavor.GZIP)
						{
							if (!this._wantCompress)
							{
								throw new ZlibException("Writing with decompression is not supported.");
							}
							int crc32Result = this.crc.Crc32Result;
							this._stream.Write(BitConverter.GetBytes(crc32Result), 0, 4);
							int value = (int)(this.crc.TotalBytesRead & unchecked((long)((ulong)(-1))));
							this._stream.Write(BitConverter.GetBytes(value), 0, 4);
						}
					}
					else if (this._streamMode == ZlibBaseStream.StreamMode.Reader)
					{
						if (this._flavor == ZlibStreamFlavor.GZIP)
						{
							if (this._wantCompress)
							{
								throw new ZlibException("Reading with compression is not supported.");
							}
							if (this._z.TotalBytesOut != 0L)
							{
								byte[] array = new byte[8];
								if (this._z.AvailableBytesIn < 8)
								{
									Array.Copy(this._z.InputBuffer, this._z.NextIn, array, 0, this._z.AvailableBytesIn);
									int num2 = 8 - this._z.AvailableBytesIn;
									int num3 = this._stream.Read(array, this._z.AvailableBytesIn, num2);
									if (num2 != num3)
									{
										throw new ZlibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", this._z.AvailableBytesIn + num3));
									}
								}
								else
								{
									Array.Copy(this._z.InputBuffer, this._z.NextIn, array, 0, array.Length);
								}
								int num4 = BitConverter.ToInt32(array, 0);
								int crc32Result2 = this.crc.Crc32Result;
								int num5 = BitConverter.ToInt32(array, 4);
								int num6 = (int)(this._z.TotalBytesOut & unchecked((long)((ulong)(-1))));
								if (crc32Result2 != num4 && !this.m_chimera_omitTheCrcCheck)
								{
									throw new ZlibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32Result2, num4));
								}
								if (num6 != num5 && !this.m_chimera_omitTheCrcCheck)
								{
									throw new ZlibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", num6, num5));
								}
							}
						}
					}
				}
			}

			private void end()
			{
				if (this.z != null)
				{
					if (this._wantCompress)
					{
						this._z.EndDeflate();
					}
					else
					{
						this._z.EndInflate();
					}
					this._z = null;
				}
			}

			public override void Close()
			{
				if (this._stream != null)
				{
					try
					{
						this.finish();
					}
					finally
					{
						this.end();
						if (!this._leaveOpen)
						{
							this._stream.Close();
						}
						this._stream = null;
					}
				}
			}

			public override void Flush()
			{
				this._stream.Flush();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				this._stream.SetLength(value);
			}

			private string ReadZeroTerminatedString()
			{
				List<byte> list = new List<byte>();
				bool flag = false;
				for (; ; )
				{
					int num = this._stream.Read(this._buf1, 0, 1);
					if (num != 1)
					{
						break;
					}
					if (this._buf1[0] == 0)
					{
						flag = true;
					}
					else
					{
						list.Add(this._buf1[0]);
					}
					if (flag)
					{
						goto Block_3;
					}
				}
				throw new ZlibException("Unexpected EOF reading GZIP header.");
			Block_3:
				byte[] array = list.ToArray();
				return GZipStream.iso8859dash1.GetString(array, 0, array.Length);
			}

			private int _ReadAndValidateGzipHeader()
			{
				int num = 0;
				byte[] array = new byte[10];
				int num2 = this._stream.Read(array, 0, array.Length);
				int result;
				if (num2 == 0)
				{
					result = 0;
				}
				else
				{
					if (num2 != 10)
					{
						throw new ZlibException("Not a valid GZIP stream.");
					}
					if (array[0] != 31 || array[1] != 139 || array[2] != 8)
					{
						throw new ZlibException("Bad GZIP header.");
					}
					int num3 = BitConverter.ToInt32(array, 4);
					this._GzipMtime = GZipStream._unixEpoch.AddSeconds((double)num3);
					num += num2;
					if ((array[3] & 4) == 4)
					{
						num2 = this._stream.Read(array, 0, 2);
						num += num2;
						short num4 = (short)((int)array[0] + (int)array[1] * 256);
						byte[] array2 = new byte[(int)num4];
						num2 = this._stream.Read(array2, 0, array2.Length);
						if (num2 != (int)num4)
						{
							throw new ZlibException("Unexpected end-of-file reading GZIP header.");
						}
						num += num2;
					}
					if ((array[3] & 8) == 8)
					{
						this._GzipFileName = this.ReadZeroTerminatedString();
					}
					if ((array[3] & 16) == 16)
					{
						this._GzipComment = this.ReadZeroTerminatedString();
					}
					if ((array[3] & 2) == 2)
					{
						this.Read(this._buf1, 0, 1);
					}
					result = num;
				}
				return result;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
				{
					if (!this._stream.CanRead)
					{
						throw new ZlibException("The stream is not readable.");
					}
					this._streamMode = ZlibBaseStream.StreamMode.Reader;
					this.z.AvailableBytesIn = 0;
					if (this._flavor == ZlibStreamFlavor.GZIP)
					{
						this._gzipHeaderByteCount = this._ReadAndValidateGzipHeader();
						if (this._gzipHeaderByteCount == 0)
						{
							return 0;
						}
					}
				}
				if (this._streamMode != ZlibBaseStream.StreamMode.Reader)
				{
					throw new ZlibException("Cannot Read after Writing.");
				}
				int result;
				if (count == 0)
				{
					result = 0;
				}
				else if (this.nomoreinput && this._wantCompress)
				{
					result = 0;
				}
				else
				{
					if (buffer == null)
					{
						throw new ArgumentNullException("buffer");
					}
					if (count < 0)
					{
						throw new ArgumentOutOfRangeException("count");
					}
					if (offset < buffer.GetLowerBound(0))
					{
						throw new ArgumentOutOfRangeException("offset");
					}
					if (offset + count > buffer.GetLength(0))
					{
						throw new ArgumentOutOfRangeException("count");
					}
					this._z.OutputBuffer = buffer;
					this._z.NextOut = offset;
					this._z.AvailableBytesOut = count;
					this._z.InputBuffer = this.workingBuffer;
					int num;
					for (; ; )
					{
						if (this._z.AvailableBytesIn == 0 && !this.nomoreinput)
						{
							this._z.NextIn = 0;
							this._z.AvailableBytesIn = this._stream.Read(this._workingBuffer, 0, this._workingBuffer.Length);
							if (this._z.AvailableBytesIn == 0)
							{
								this.nomoreinput = true;
							}
						}
						num = (this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode));
						if (this.nomoreinput && num == -5)
						{
							break;
						}
						if (num != 0 && num != 1)
						{
							goto Block_20;
						}
						if ((this.nomoreinput || num == 1) && this._z.AvailableBytesOut == count)
						{
							goto Block_23;
						}
						if (this._z.AvailableBytesOut <= 0 || this.nomoreinput || num != 0)
						{
							goto IL_2AA;
						}
					}
					return 0;
				Block_20:
					throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", this._wantCompress ? "de" : "in", num, this._z.Message));
				Block_23:
				IL_2AA:
					if (this._z.AvailableBytesOut > 0)
					{
						if (num == 0 && this._z.AvailableBytesIn == 0)
						{
						}
						if (this.nomoreinput)
						{
							if (this._wantCompress)
							{
								num = this._z.Deflate(FlushType.Finish);
								if (num != 0 && num != 1)
								{
									throw new ZlibException(string.Format("Deflating:  rc={0}  msg={1}", num, this._z.Message));
								}
							}
						}
					}
					num = count - this._z.AvailableBytesOut;
					if (this.crc != null)
					{
						this.crc.SlurpBlock(buffer, offset, num);
					}
					result = num;
				}
				return result;
			}

			public override bool CanRead
			{
				get
				{
					return this._stream.CanRead;
				}
			}

			public override bool CanSeek
			{
				get
				{
					return this._stream.CanSeek;
				}
			}

			public override bool CanWrite
			{
				get
				{
					return this._stream.CanWrite;
				}
			}

			public override long Length
			{
				get
				{
					return this._stream.Length;
				}
			}

			public override long Position
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			protected internal ZlibCodec _z = null;

			protected internal ZlibBaseStream.StreamMode _streamMode = ZlibBaseStream.StreamMode.Undefined;

			protected internal FlushType _flushMode;

			protected internal ZlibStreamFlavor _flavor;

			protected internal CompressionMode _compressionMode;

			protected internal CompressionLevel _level;

			protected internal bool _leaveOpen;

			protected internal byte[] _workingBuffer;

			protected internal int _bufferSize = 16384;

			protected internal byte[] _buf1 = new byte[1];

			protected internal Stream _stream;

			protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

			private readonly bool m_chimera_omitTheCrcCheck;

			private CRC32 crc;

			protected internal string _GzipFileName;

			protected internal string _GzipComment;

			protected internal DateTime _GzipMtime;

			protected internal int _gzipHeaderByteCount;

			private bool nomoreinput = false;

			internal enum StreamMode
			{
				Writer,
				Reader,
				Undefined
			}
		}

		[ComVisible(true)]
		// [ClassInterface(ClassInterfaceType.AutoDispatch)]
		[Guid("ebc25cf6-9120-4283-b972-0e5520d0000D")]
		public sealed class ZlibCodec
		{
			public int InitializeInflate(bool expectRfc1950Header)
			{
				return this.InitializeInflate(this.WindowBits, expectRfc1950Header);
			}

			public int InitializeInflate(int windowBits, bool expectRfc1950Header)
			{
				this.WindowBits = windowBits;
				if (this.dstate != null)
				{
					throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
				}
				this.istate = new InflateManager(expectRfc1950Header);
				return this.istate.Initialize(this, windowBits);
			}

			public int Inflate(FlushType flush)
			{
				if (this.istate == null)
				{
					throw new ZlibException("No Inflate State!");
				}
				return this.istate.Inflate(flush);
			}

			public int EndInflate()
			{
				if (this.istate == null)
				{
					throw new ZlibException("No Inflate State!");
				}
				int result = this.istate.End();
				this.istate = null;
				return result;
			}

			public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
			{
				this.CompressLevel = level;
				return this._InternalInitializeDeflate(wantRfc1950Header);
			}

			private int _InternalInitializeDeflate(bool wantRfc1950Header)
			{
				if (this.istate != null)
				{
					throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
				}
				this.dstate = new DeflateManager();
				this.dstate.WantRfc1950HeaderBytes = wantRfc1950Header;
				return this.dstate.Initialize(this, this.CompressLevel, this.WindowBits, this.Strategy);
			}

			public int Deflate(FlushType flush)
			{
				if (this.dstate == null)
				{
					throw new ZlibException("No Deflate State!");
				}
				return this.dstate.Deflate(flush);
			}

			public int EndDeflate()
			{
				if (this.dstate == null)
				{
					throw new ZlibException("No Deflate State!");
				}
				this.dstate = null;
				return 0;
			}

			internal void flush_pending()
			{
				int num = this.dstate.pendingCount;
				if (num > this.AvailableBytesOut)
				{
					num = this.AvailableBytesOut;
				}
				if (num != 0)
				{
					if (this.dstate.pending.Length <= this.dstate.nextPending || this.OutputBuffer.Length <= this.NextOut || this.dstate.pending.Length < this.dstate.nextPending + num || this.OutputBuffer.Length < this.NextOut + num)
					{
						throw new ZlibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", this.dstate.pending.Length, this.dstate.pendingCount));
					}
					Array.Copy(this.dstate.pending, this.dstate.nextPending, this.OutputBuffer, this.NextOut, num);
					this.NextOut += num;
					this.dstate.nextPending += num;
					this.TotalBytesOut += (long)num;
					this.AvailableBytesOut -= num;
					this.dstate.pendingCount -= num;
					if (this.dstate.pendingCount == 0)
					{
						this.dstate.nextPending = 0;
					}
				}
			}

			internal int read_buf(byte[] buf, int start, int size)
			{
				int num = this.AvailableBytesIn;
				if (num > size)
				{
					num = size;
				}
				int result;
				if (num == 0)
				{
					result = 0;
				}
				else
				{
					this.AvailableBytesIn -= num;
					if (this.dstate.WantRfc1950HeaderBytes)
					{
						this._Adler32 = Adler.Adler32(this._Adler32, this.InputBuffer, this.NextIn, num);
					}
					Array.Copy(this.InputBuffer, this.NextIn, buf, start, num);
					this.NextIn += num;
					this.TotalBytesIn += (long)num;
					result = num;
				}
				return result;
			}

			public byte[] InputBuffer;

			public int NextIn;

			public int AvailableBytesIn;

			public long TotalBytesIn;

			public byte[] OutputBuffer;

			public int NextOut;

			public int AvailableBytesOut;

			public long TotalBytesOut;

			public string Message;

			internal DeflateManager dstate;

			internal InflateManager istate;

			internal uint _Adler32;

			public CompressionLevel CompressLevel = CompressionLevel.Default;

			public int WindowBits = 15;

			public CompressionStrategy Strategy = CompressionStrategy.Default;
		}

		[Guid("ebc25cf6-9120-4283-b972-0e5520d0000E")]
		public class ZlibException : Exception
		{
			public ZlibException(string s)
				: base(s)
			{
			}
		}

		internal enum ZlibStreamFlavor
		{
			ZLIB = 1950,
			DEFLATE,
			GZIP
		}

		public class CompressionDotNetZipImpl : ICompressionService
		{
			public byte[] Compress(byte[] data)
			{
				byte[] result;
				using (MemoryStream memoryStream = new MemoryStream(data))
				{
					using (MemoryStream memoryStream2 = new MemoryStream())
					{
						using (Stream stream = new GZipStream(memoryStream2, CompressionMode.Compress, CompressionLevel.Default, false, false))
						{
							byte[] array = new byte[data.Length / 2];
							int count;
							while ((count = memoryStream.Read(array, 0, array.Length)) != 0)
							{
								stream.Write(array, 0, count);
							}
							stream.Flush();
							stream.Close();
							byte[] array2 = memoryStream2.ToArray();
							result = array2;
						}
					}
				}
				return result;
			}

			public byte[] Decompress(byte[] data)
			{
				byte[] result;
				using (MemoryStream memoryStream = new MemoryStream(data))
				{
					byte[] array = new byte[data.Length / 2];
					int num = 1;
					using (Stream stream = new GZipStream(memoryStream, CompressionMode.Decompress, CompressionLevel.Default, true, false))
					{
						using (MemoryStream memoryStream2 = new MemoryStream())
						{
							while (num != 0)
							{
								num = stream.Read(array, 0, array.Length);
								if (num > 0)
								{
									memoryStream2.Write(array, 0, num);
								}
							}
							byte[] array2 = memoryStream2.ToArray();
							result = array2;
						}
					}
				}
				return result;
			}

			public byte[] DecompressIfNecessary(byte[] data)
			{
				byte[] result;
				try
				{
					result = this.Decompress(data);
				}
				catch (Exception ex)
				{
					if (!ex.Message.ToLower().Contains("bad gzip header"))
					{
						throw ex;
					}
					result = data;
				}
				return result;
			}
		}

		public interface ICompressionService
		{
			byte[] Compress(byte[] data);

			byte[] Decompress(byte[] data);

			byte[] DecompressIfNecessary(byte[] data);
		}

		private static ICompressionService m_compressionService;

		public static ICompressionService GetCompressionService()
		{
			ICompressionService result;
			if ((result = m_compressionService) == null)
			{
				result = (m_compressionService = new CompressionDotNetZipImpl());
			}
			return result;
		}
	}
}
