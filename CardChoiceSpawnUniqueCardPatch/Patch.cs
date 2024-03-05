using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unbound.Core;
using UnityEngine;

namespace CardChoiceSpawnUniqueCardPatch {
    [Serializable]
    [HarmonyPatch(typeof(CardChoice), "SpawnUniqueCard")]
    public class CardChoicePatchSpawnUniqueCard {
        public static Func<CardInfo[], CardInfo> DrawRandom = (cards) => {
            CardInfo result = null;
            float num = 0f;
            for(int i = 0; i < cards.Length; i++) {
                num += GetWeight(cards[i]);
            }

            float num2 = UnityEngine.Random.Range(0f, num);

            for(int j = 0; j < cards.Length; j++) {
                num2 -= GetWeight(cards[j]);
                if(num2 <= 0f) {
                    result = cards[j];
                    break;
                }
            }

            return result;
        };

        public static Func<CardInfo, float> GetWeight = (card) => {
            return card.rarity switch {
                CardInfo.Rarity.Common => 10f,
                CardInfo.Rarity.Uncommon => 4f,
                CardInfo.Rarity.Rare => 1f,
                _ => 0f
            };
        };

        public static Func<CardInfo, bool> CanCardSpawn = (card) => {

            List<GameObject> spawnedCards = (List<GameObject>)Traverse.Create(CardChoice.instance).Field("spawnedCards").GetValue();


            if(!card.categories.Contains(CustomCategories.CustomCardCategories.CanDrawMultipleCategory)) 
                return !spawnedCards.Any(spawnedCard => spawnedCard.name.Replace("(Clone)", "") == card.gameObject.name);
            return true;

        };

        public static Func<Player, CardInfo, bool> PlayerIsAllowedCard = (player, card) => {
            for(int i = 0; i < player.data.currentCards.Count; i++) {
                CardInfo playerCard = player.data.currentCards[i];
                if(playerCard.blacklistedCategories.Intersect(card.categories).Any() || (!card.allowMultiple && playerCard.name == card.name))
                    return false;
            }
            Holdable holdable = player.data.GetComponent<Holding>().holdable;
            if(holdable) {
                Gun component2 = holdable.GetComponent<Gun>();
                Gun component3 = card.GetComponent<Gun>();
                if(component3 && component2 && component3.lockGunToDefault && component2.lockGunToDefault) {
                    return false;
                }
            }
            return true;
        };

        internal static bool Validator(Player player, CardInfo card){ 
            return CanCardSpawn(card) && PlayerIsAllowedCard(player, card);
        }

        private static bool Prefix(ref GameObject __result, CardChoice __instance, Vector3 pos, Quaternion rot) {
            Player player = GetCurentPicker();

            CardInfo randomCard = DrawRandom(CardChoice.instance.cards.Where(c => Validator(player, c)).ToArray());

            if(randomCard == null) randomCard = CardChoiceSpawnUniqueCardPatch.NullCard;

            __result = (GameObject)__instance.InvokeMethod("Spawn", randomCard.gameObject, pos, rot);
            __result.GetComponent<CardInfo>().sourceCard = randomCard;
            __result.GetComponentInChildren<DamagableEvent>().GetComponent<Collider2D>().enabled = false;

            return false;
        }

        private static Player GetCurentPicker() {
            if(CardChoice.instance.IsPicking)
                return (PickerType)CardChoice.instance.GetFieldValue("pickerType") == PickerType.Team
                    ? PlayerManager.instance.GetPlayersInTeam(CardChoice.instance.pickrID)[0] 
                    : PlayerManager.instance.players[CardChoice.instance.pickrID];
            return null;
        }
    }

}
