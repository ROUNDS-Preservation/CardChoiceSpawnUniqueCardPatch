using System.Collections.Generic;
using System.Linq;
using Unbound.Cards;
using UnityEngine;

namespace CardChoiceSpawnUniqueCardPatch {
    public class NullCard:CustomCard {
        public const string cardName = "  ";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers) {
            cardInfo.sourceCard = CardChoiceSpawnUniqueCardPatch.NullCard;
            cardInfo.categories = cardInfo.categories.ToList().Concat(new List<CardCategory>() { CustomCategories.CustomCardCategories.CanDrawMultipleCategory }).ToArray();
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats) {
            this.gameObject.GetComponent<CardInfo>().sourceCard = CardChoiceSpawnUniqueCardPatch.NullCard;
        }
        public override void OnRemoveCard() {
        }

        protected override string GetTitle() {
            return cardName;
        }
        protected override string GetDescription() {
            return "Rounds Has Run Out Of Valid Cards.";
        }

        protected override GameObject GetCardArt() {
            return null;
        }

        protected override CardInfo.Rarity GetRarity() {
            return CardInfo.Rarity.Common;
        }

        protected override CardInfoStat[] GetStats() {
            return null;
        }
        protected override CardThemeColor.CardThemeColorType GetTheme() {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override bool GetEnabled() {
            return false;
        }
        public override string GetModName() {
            return "BLANK CARD";
        }
    }
}
