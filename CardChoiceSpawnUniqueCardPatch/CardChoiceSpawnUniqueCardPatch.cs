using BepInEx;
using HarmonyLib;
using UnboundLib.Cards;

namespace CardChoiceSpawnUniqueCardPatch {
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, "0.2.0")]
    [BepInProcess("Rounds.exe")]
    public class CardChoiceSpawnUniqueCardPatch:BaseUnityPlugin {
        private void Awake() {
            new Harmony(ModId).PatchAll();
            var _ = CustomCategories.CustomCardCategories.instance;
        }
        private void Start() {
            CustomCard.BuildCard<NullCard>(cardInfo => NullCard = cardInfo);
        }

        private const string ModId = "rounds.paches.cardchoicespawnuniquecardpatch";

        private const string ModName = "CardChoiceSpawnUniqueCardPatch";

        public static CardInfo NullCard;
    }

}