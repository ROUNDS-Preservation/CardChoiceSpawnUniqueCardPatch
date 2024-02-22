using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CardChoiceSpawnUniqueCardPatch {
    [Serializable]
    [HarmonyPatch(typeof(CardChoice), "SpawnUniqueCard")]
    public class CardChoicePatchSpawnUniqueCard {
        //To be overwriten by modding utils if present.
        public static Func<Player, CardInfo> DrawRandom = (player) => {
            CardInfo result = null;
            var cards = CardChoice.instance.cards.Where(c=> ModifiedBaseCondition(player,c) && PlayerIsAllowedCard(player,c)).ToArray();
            float num = 0f;
            for(int i = 0; i < cards.Length; i++) {
                if(cards[i].rarity == CardInfo.Rarity.Common) {
                    num += 10f;
                }
                if(cards[i].rarity == CardInfo.Rarity.Uncommon) {
                    num += 4f;
                }
                if(cards[i].rarity == CardInfo.Rarity.Rare) {
                    num += 1f;
                }
            }

            float num2 = UnityEngine.Random.Range(0f, num);
            for(int j = 0; j < cards.Length; j++) {
                if(cards[j].rarity == CardInfo.Rarity.Common) {
                    num2 -= 10f;
                }
                if(cards[j].rarity == CardInfo.Rarity.Uncommon) {
                    num2 -= 4f;
                }
                if(cards[j].rarity == CardInfo.Rarity.Rare) {
                    num2 -= 1f;
                }
                if(num2 <= 0f) {
                    result = cards[j];
                    break;
                }
            }

            return result;
        };

        private static bool Prefix(ref GameObject __result, CardChoice __instance, Vector3 pos, Quaternion rot) {
            Player player;
            if((PickerType)Traverse.Create(__instance).Field("pickerType").GetValue() == PickerType.Team) {
                player = PlayerManager.instance.GetPlayersInTeam(__instance.pickrID)[0];
            } else {
                player = PlayerManager.instance.players[__instance.pickrID];
            }

            CardInfo validCard = null;
            if(CardChoice.instance.cards.Length > 0) {
                validCard = DrawRandom(player);
            }

            if(validCard != null) {
                GameObject gameObject = (GameObject)typeof(CardChoice).InvokeMember("Spawn",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, __instance, new object[] { validCard.gameObject, pos, rot });
                gameObject.GetComponent<CardInfo>().sourceCard = validCard;
                gameObject.GetComponentInChildren<DamagableEvent>().GetComponent<Collider2D>().enabled = false;

                __result = gameObject;
            } else {
                // there are no valid cards left - this is an extremely unlikely scenario, only achievable if most of the cards have been disabled

                // return a blank card
                GameObject gameObject = (GameObject)typeof(CardChoice).InvokeMember("Spawn",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, __instance, new object[] { CardChoiceSpawnUniqueCardPatch.NullCard.gameObject, pos, rot });
                gameObject.GetComponent<CardInfo>().sourceCard = CardChoiceSpawnUniqueCardPatch.NullCard;
                gameObject.GetComponentInChildren<DamagableEvent>().GetComponent<Collider2D>().enabled = false;
                __result = gameObject;
            }

            return false;
        }

        public static bool ModifiedBaseCondition(Player player, CardInfo card) {

            List<GameObject> spawnedCards = (List<GameObject>)Traverse.Create(CardChoice.instance).Field("spawnedCards").GetValue();

            //check if gun is locked to default
            if(CardChoice.instance.pickrID != -1) {
                Holdable holdable = player.data.GetComponent<Holding>().holdable;
                if(holdable) {
                    Gun component2 = holdable.GetComponent<Gun>();
                    Gun component3 = card.GetComponent<Gun>();
                    if(component3 && component2 && component3.lockGunToDefault && component2.lockGunToDefault) {
                        return false;
                    }
                }
            }

            // slightly modified condition that if the card has the CanDrawMultipleCategory, then its okay that its a duplicate
            if(card.categories.Contains(CustomCategories.CustomCardCategories.CanDrawMultipleCategory)) {
                return true;
            } else {
                return !spawnedCards.Any(spawnedCard => spawnedCard.name.Replace("(Clone)", "") == card.gameObject.name);
            }

        }

        internal static bool PlayerIsAllowedCard(Player player, CardInfo card) 
        {
            for(int i = 0; i < player.data.currentCards.Count; i++) {
                CardInfo playerCard = player.data.currentCards[i];
                if(playerCard.blacklistedCategories.Intersect(card.categories).Any() || (!card.allowMultiple && playerCard.name == card.name))
                    return false;
            }
            return true;
        }
    }

}
