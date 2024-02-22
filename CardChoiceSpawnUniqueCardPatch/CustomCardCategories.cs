using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnboundLib.Utils;

namespace CardChoiceSpawnUniqueCardPatch.CustomCategories {
    public class CustomCardCategories {
        // custom card class for cards that can be drawn multiple times in a single hand
        public static CardCategory CanDrawMultipleCategory => CustomCategories.CustomCardCategories.instance.CardCategory("__CanDrawMultiple__");

        // singleton design, so that the categories are only created once
        public static readonly CustomCardCategories instance = new CustomCardCategories();

        private List<CardCategory> cardCategories = new List<CardCategory>() { };

        private CustomCardCategories() {
            CustomCardCategories instance = this;

            CardInfo[] vanilla = Resources.LoadAll<GameObject>("0 Cards/").Where(obj => obj.GetComponent<CardInfo>()).Select(obj2 => obj2.GetComponent<CardInfo>()).ToArray();

            foreach(CardInfo card in vanilla) {
                UpdateAndPullCategoriesFromCard(card);
            }

            foreach(CardInfo card in CardManager.cards.Values.Select(c => c.cardInfo)) {
                UpdateAndPullCategoriesFromCard(card);
            }

            CardManager.AddAllCardsCallback(FirstStartAction);
        }

        private void FirstStartAction(CardInfo[] cards) {
            foreach(CardInfo card in cards) {
                UpdateAndPullCategoriesFromCard(card);
            }
        }

        public void UpdateAndPullCategoriesFromCard(CardInfo card) {
            List<CardCategory> goodCategories = new List<CardCategory>();
            for(int i = 0; i < card.categories.Length; i++) {
                CardCategory category = card.categories[i];

                if(category == null) {
                    continue;
                }

                if(!this.cardCategories.Contains(category)) {
                    var storedCategory = GetCategoryWithName(category.name);

                    if(storedCategory != null) {
                        card.categories[i] = storedCategory;
                        category = card.categories[i];
                    } else {
                        this.cardCategories.Add(category);
                    }
                }
                goodCategories.Add(category);
            }
            card.categories = goodCategories.ToArray();
            goodCategories = new List<CardCategory>();
            for(int i = 0; i < card.blacklistedCategories.Length; i++) {
                CardCategory category = card.blacklistedCategories[i];

                if(category == null) {
                    continue;
                }

                if(!this.cardCategories.Contains(category)) {
                    var storedCategory = GetCategoryWithName(category.name);

                    if(storedCategory != null) {
                        card.blacklistedCategories[i] = storedCategory;
                        category = card.blacklistedCategories[i];
                    } else {
                        this.cardCategories.Add(category);
                    }
                }
                goodCategories.Add(category);
            }
            card.blacklistedCategories = goodCategories.ToArray();
        }

        private CardCategory GetCategoryWithName(string categoryName) {

            foreach(CardCategory category in this.cardCategories) {
                // not case-sensitive
                if(category != null && category.name != null && category.name.ToLower() == categoryName.ToLower()) {
                    return category;
                }
            }

            return null;

        }

        public CardCategory CardCategory(string categoryName) {
            CardCategory category = this.GetCategoryWithName(categoryName);

            if(category == null) {
                CardCategory newCategory = ScriptableObject.CreateInstance<CardCategory>();
                newCategory.name = categoryName.ToLower();
                this.cardCategories.Add(newCategory);

                category = newCategory;
            }

            return category;
        }

        public void MakeCardsExclusive(CardInfo card1, CardInfo card2) {
            CardCategory category1 = this.CardCategory(card1.name);
            CardCategory category2 = this.CardCategory(card2.name);
            card1.AddCatagory(category1);
            card2.AddCatagory(category2);
            card1.BlacklistCatagory(category2);
            card2.BlacklistCatagory(category1);
        }

    }
    public static class CardInfoExtention {

        public static CardInfo DrawMultiple(this CardInfo card) {
            return card.AddCatagory(CustomCardCategories.CanDrawMultipleCategory);
        }
        public static CardInfo AddCatagory(this CardInfo card, CardCategory cardCategory) {
            return card.AddCatagories(new CardCategory[] { cardCategory });
        }
        public static CardInfo AddCatagories(this CardInfo card, CardCategory[] cardCategories) {

            card.categories = card.categories.Concat(cardCategories).ToArray();
            return card;
        }
        public static CardInfo BlacklistCatagory(this CardInfo card, CardCategory cardCategory) {
            return card.BlacklistCatagories(new CardCategory[] { cardCategory });
        }
        public static CardInfo BlacklistCatagories(this CardInfo card, CardCategory[] cardCategories) {

            card.blacklistedCategories = card.blacklistedCategories.Concat(cardCategories).ToArray();
            return card;
        }
    }
}