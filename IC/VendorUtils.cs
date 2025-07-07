using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using ItemChanger.Util;

namespace VendorRando {
    //everything here is taken straight from ItemChanger with minor tweaks for scoping and compatibility
    public class VendorUtils {
        public static void EditShopRegion(PlayMakerFSM fsm) {
            FsmState checkRelics = fsm.GetState("Check Relics");
            bool hasBeenEdited = checkRelics.GetActionsOfType<DelegateBoolTest>().Any();
            if(hasBeenEdited) {
                return;
            }

            // spoof having relics if any items are available
            bool ItemsInStock() {
                ShopMenuStock shop = fsm.FsmVariables.FindFsmGameObject("Shop Object").Value.GetComponent<ShopMenuStock>();
                return shop.StockLeft();
            }
            IntCompare relicComparison = checkRelics.GetFirstActionOfType<IntCompare>();
            checkRelics.ReplaceAction(new DelegateBoolTest(ItemsInStock, "HAS RELIC", "NO RELIC"),
                checkRelics.Actions.IndexOf(relicComparison));
        }

        public static void EditShopControl(PlayMakerFSM fsm, AbstractPlacement placement, string name, DefaultShopItems defaultShopItems, string requiredBool) {
            ShopMenuStock shop = fsm.gameObject.GetComponent<ShopMenuStock>();
            GameObject itemPrefab = ObjectCache.ShopItem;
            shop.stock = GetNewStock(placement, name, defaultShopItems, shop.stock, itemPrefab, requiredBool);
            if(shop.stockAlt != null) {
                shop.stockAlt = GetNewAltStock(name, defaultShopItems, shop.stock, shop.stockAlt);
            }
            FsmState chooseNoStockConvo = fsm.GetState("Choose Convo");
            bool hasBeenEdited = chooseNoStockConvo.GetTransition(4) != null;
            if(hasBeenEdited) {
                return;
            }
            chooseNoStockConvo.AddTransition("FINISHED", "Relic Dealer");
            FsmState checkRelics = fsm.GetState("Check Relics");
            IntCompare relicComparison = checkRelics.GetFirstActionOfType<IntCompare>();
            checkRelics.ReplaceAction(new Lambda(() => fsm.SendEvent("HAS RELIC")),
                checkRelics.Actions.IndexOf(relicComparison));
        }

        public static GameObject[] GetNewStock(AbstractPlacement placement, string name, DefaultShopItems defaultShopItems, GameObject[] oldStock, GameObject shopPrefab, string requiredBool) {
            List<GameObject> stock = new(oldStock.Length + placement.Items.Count);
            void AddShopItem(AbstractItem item) {
                GameObject shopItem = Object.Instantiate(shopPrefab);
                shopItem.SetActive(false);
                ApplyItemDef(placement, name, shopItem.GetComponent<ShopItemStats>(), item, item.GetTag<CostTag>()?.Cost, requiredBool);
                stock.Add(shopItem);
            }
            foreach(var item in placement.Items.Where(i => !i.WasEverObtained()))
                AddShopItem(item);
            foreach(var item in placement.Items.Where(i => i.WasEverObtained()))
                AddShopItem(item);
            string sceneName = name switch {
                Consts.Sly => SceneNames.Room_shop,
                Consts.Salubra => SceneNames.Room_Charm_Shop,
                Consts.Iselda => SceneNames.Room_mapper,
                Consts.LegEater => SceneNames.Fungus2_26,
                Consts.Lemm => SceneNames.Ruins1_05b,
                _ => null
            };
            stock.AddRange(oldStock.Where(g => KeepOldItem(g.GetComponent<ShopItemStats>(), sceneName, defaultShopItems)));
            return stock.ToArray();
        }

        public static GameObject[] GetNewAltStock(string name, DefaultShopItems defaultShopItems, GameObject[] newStock, GameObject[] altStock) {
            return newStock.Union(altStock.Where(g => KeepOldItem(g.GetComponent<ShopItemStats>(), name, defaultShopItems))).ToArray();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static void ApplyItemDef(AbstractPlacement placement, string name, ShopItemStats stats, AbstractItem item, Cost? cost, string requiredBool) {
            foreach(var m in stats.gameObject.GetComponents<ModShopItemStats>())
                Object.Destroy(m); // Probably not necessary

            CostDisplayer costDisplayer = new GeoCostDisplayer();

            var mod = stats.gameObject.AddComponent<ModShopItemStats>();
            mod.item = item;
            mod.cost = cost;
            mod.costDisplayer = costDisplayer;
            mod.placement = placement;

            // Apply all the stored values
            stats.playerDataBoolName = string.Empty;
            stats.nameConvo = string.Empty;
            stats.descConvo = string.Empty;
            stats.requiredPlayerDataBool = requiredBool;
            stats.removalPlayerDataBool = string.Empty;
            stats.dungDiscount = name == Consts.LegEater;
            stats.notchCostBool = string.Empty;


            // Need to set all these to make sure the item doesn't break in one of various ways
            stats.priceConvo = string.Empty;
            stats.specialType = 0;
            stats.charmsRequired = 0;
            stats.relic = false;
            stats.relicNumber = 0;
            stats.relicPDInt = string.Empty;

            // Apply the sprite for the UI
            stats.transform.Find("Item Sprite").gameObject.GetComponent<SpriteRenderer>().sprite = item.GetResolvedUIDef(placement)!.GetSprite();

            ISprite ispritecranberry = costDisplayer.CustomCostSprite;
            if(ispritecranberry == null) {
                return;
            }
            Sprite costSprite = ispritecranberry.Value;
            if(costSprite != null) {
                stats.transform.Find("Geo Sprite").gameObject.GetComponent<SpriteRenderer>().sprite = costSprite;
            }
        }

        public static bool KeepOldItem(ShopItemStats stats, string name, DefaultShopItems defaultShopItems) {
            DefaultShopItems? itemType = ShopUtil.GetVanillaShopItemType(name, stats);
            if(itemType == null)
                return true; // unrecognized items are kept by default
            if(itemType == DefaultShopItems.SalubraBlessing && (defaultShopItems & DefaultShopItems.SalubraNotches) == 0) {
                stats.requiredPlayerDataBool = string.Empty; // vanilla blessing appears immediately when notches are not vanilla
            }
            return (itemType & defaultShopItems) == itemType;
        }

        public static void EditItemListControl(PlayMakerFSM fsm) {
            FsmState init = fsm.GetState("Init");

            bool hasBeenEdited = init.GetActionsOfType<Lambda>().Any(); // for cases like sly, sly key, only one placement needs to edit the shop functionality
            if(hasBeenEdited) {
                return;
            }

            FsmState getDetailsInit = fsm.GetState("Get Details Init");
            FsmState getDetails = fsm.GetState("Get Details");
            FsmState charmsRequiredInit = fsm.GetState("Charms Required? Init");
            FsmState charmsRequired = fsm.GetState("Charms Required?");
            FsmState notchDisplayInit = fsm.GetState("Notch Display Init");
            FsmState notchDisplay = fsm.GetState("Notch Display?");
            FsmState checkCanBuy = fsm.GetState("Check Can Buy");
            FsmState activateConfirm = fsm.GetState("Activate confirm");
            FsmState activateUI = fsm.GetState("Activate UI");

            var textSetters = getDetails.GetActionsOfType<SetTextMeshProText>();

            void SetName() {
                int index = fsm.FsmVariables.FindFsmInt("Current Item").Value;
                GameObject shopItem = fsm.gameObject.GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                string name;
                if(mod && mod.item != null) {
                    name = mod.GetPreviewName();
                }
                else {
                    name = Language.Language.Get(shopItem.GetComponent<ShopItemStats>().GetNameConvo(), "UI");
                }

                fsm.FsmVariables.FindFsmGameObject("Item name").Value.GetComponent<TextMeshPro>().text = name;
            }

            void ResetSprites() {
                foreach(GameObject shopItem in fsm.gameObject.GetComponent<ShopMenuStock>().stockInv) {
                    var mod = shopItem.GetComponent<ModShopItemStats>();
                    if(!mod || mod.item == null)
                        continue;
                    shopItem.transform.Find("Item Sprite").gameObject.GetComponent<SpriteRenderer>().sprite = mod.GetSprite();
                }
            }

            void SetDesc() {
                int index = fsm.FsmVariables.FindFsmInt("Current Item").Value;
                GameObject shopItem = fsm.gameObject.GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                string desc;
                if(mod && mod.item != null) {
                    desc = mod.GetShopDesc();
                    if(mod.cost is not null && !mod.cost.Paid) {
                        string? costText = mod.GetShopCostText();
                        if(!string.IsNullOrEmpty(costText)) {
                            desc += $"\n\n<#888888>{costText}";
                        }
                    }
                }
                else {
                    int charmsRequired = shopItem.GetComponent<ShopItemStats>().GetCharmsRequired();
                    if(charmsRequired > 0) {
                        charmsRequired -= PlayerData.instance.GetInt(nameof(PlayerData.charmsOwned));
                    }
                    if(charmsRequired > 0) {
                        desc = string.Concat(Language.Language.Get(shopItem.GetComponent<ShopItemStats>().GetDescConvo() + "_NE", "UI").Replace("<br>", "\n"), " ", charmsRequired.ToString(), " ",
                            Language.Language.Get("CHARMS_REMAINING", "UI"));
                    }
                    else {
                        desc = Language.Language.Get(shopItem.GetComponent<ShopItemStats>().GetDescConvo(), "UI").Replace("<br>", "\n");
                    }
                }

                fsm.FsmVariables.FindFsmGameObject("Item desc").Value.GetComponent<TextMeshPro>()
                    .text = desc;
            }

            void GetNotchCost() {
                int index = fsm.FsmVariables.FindFsmInt("Current Item").Value;
                GameObject shopItem = fsm.gameObject.GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                var stats = shopItem.GetComponent<ShopItemStats>();
                int notchCost = 0;
                if(mod && !mod.IsSecretItem() && mod.item is AbstractItem item) {
                    if(item.GetTag<IShopNotchCostTag>() is IShopNotchCostTag notchCostTag) {
                        notchCost = notchCostTag.GetNotchCost(item);
                    }
                    else if(item is ItemChanger.Items.CharmItem charm) {
                        notchCost = PlayerData.instance.GetInt($"charmCost_{charm.charmNum}");
                    }
                }
                else {
                    notchCost = stats.GetNotchCost();
                }

                fsm.FsmVariables.FindFsmInt("Notch Cost").Value = notchCost;
            }

            bool CanBuy() {
                int index = fsm.FsmVariables.FindFsmInt("Current Item").Value;
                GameObject shopItem = fsm.gameObject.GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                if(mod) {
                    Cost? cost = mod.cost;
                    return cost == null || cost.Paid || cost.CanPay();
                }
                else {
                    return fsm.gameObject.GetComponent<ShopMenuStock>().CanBuy(index);
                }
            }

            void SetConfirmName() {
                int index = fsm.FsmVariables.FindFsmInt("Current Item").Value;
                GameObject shopItem = fsm.gameObject.GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                string name;
                if(mod && mod.item != null) {
                    name = mod.GetPreviewName();
                }
                else {
                    name = Language.Language.Get(shopItem.GetComponent<ShopItemStats>().GetNameConvo(), "UI");
                }

                fsm.FsmVariables.FindFsmGameObject("Confirm").Value.transform.Find("Item name").GetComponent<TextMeshPro>()
                    .text = name;
            }

            void AddIntToConfirm() {
                GameObject uiList = fsm.FsmVariables.FindFsmGameObject("UI List").Value;
                PlayMakerFSM confirmControl = uiList.LocateMyFSM("Confirm Control");
                FsmInt itemIndex = confirmControl.FsmVariables.FindFsmInt("Item Index");
                if(itemIndex == null) {
                    int length = confirmControl.FsmVariables.IntVariables.Length;
                    FsmInt[] fsmInts = new FsmInt[length + 1];
                    confirmControl.FsmVariables.IntVariables.CopyTo(fsmInts, 0);
                    itemIndex = fsmInts[length] = new FsmInt {
                        Name = "Item Index",
                    };
                    confirmControl.FsmVariables.IntVariables = fsmInts;
                }
                itemIndex.Value = fsm.FsmVariables.FindFsmInt("Current Item").Value;
            }

            Lambda resetSprites = new(ResetSprites);
            Lambda setName = new(SetName);
            Lambda setSprite = new(ResetSprites);
            Lambda setDesc = new(SetDesc);
            Lambda getNotchCost = new(GetNotchCost);
            DelegateBoolTest canBuy = new(CanBuy, checkCanBuy.GetFirstActionOfType<BoolTest>());
            Lambda setConfirmName = new(SetConfirmName);
            Lambda addIntToConfirm = new(AddIntToConfirm);

            init.AddLastAction(resetSprites);
            getDetailsInit.SetActions(
                setName,
                setSprite,
                // 7-8 Activate detail pane
                getDetailsInit.Actions[7],
                getDetailsInit.Actions[8]
            );
            getDetails.SetActions(setName);
            charmsRequiredInit.SetActions(setDesc);
            charmsRequired.SetActions(setDesc);
            notchDisplayInit.AddFirstAction(getNotchCost);
            notchDisplay.AddFirstAction(getNotchCost);
            checkCanBuy.SetActions(canBuy);
            activateConfirm.SetActions(
                // Find Children
                activateConfirm.Actions[0],
                activateConfirm.Actions[1],
                activateConfirm.Actions[2],
                // 3-4 Set Confirm Name -- replace
                setConfirmName,
                // 5-6 Set Confirm Cost
                activateConfirm.Actions[5],
                activateConfirm.Actions[6],
                // 7-10 Set and adjust sprite
                activateConfirm.Actions[7],
                activateConfirm.Actions[8],
                activateConfirm.Actions[9],
                activateConfirm.Actions[10],
                // 11 Set relic number
                activateConfirm.Actions[11],
                // 12-15 Activate and send events
                activateConfirm.Actions[12],
                activateConfirm.Actions[13],
                activateConfirm.Actions[14],
                activateConfirm.Actions[15]
            );
            activateUI.AddLastAction(addIntToConfirm);
        }

        public static void EditConfirmControl(PlayMakerFSM fsm) {
            FsmState deductSet = fsm.GetState("Deduct Geo and set PD");
            if(deductSet.GetActionsOfType<Lambda>().Any()) {
                return; // Fsm has already been edited
            }

            bool ShouldSell() {
                int index = fsm.FsmVariables.FindFsmInt("Item Index").Value;
                GameObject shopItem = fsm.transform.parent.parent.Find("Item List").GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();

                // only vanilla items in a selling shop are eligible for sale
                return !mod && fsm.FsmVariables.FindFsmBool("Selling Shop").Value;
            }

            FsmState trinketCheck = fsm.GetState("Trinket?");
            BoolTest sellTest = trinketCheck.GetFirstActionOfType<BoolTest>();
            trinketCheck.ReplaceAction(new DelegateBoolTest(ShouldSell, sellTest),
                trinketCheck.Actions.IndexOf(sellTest));

            void Give() {
                int index = fsm.FsmVariables.FindFsmInt("Item Index").Value;
                GameObject shopItem = fsm.transform.parent.parent.Find("Item List").GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();

                if(mod) {
                    mod.item.Give(mod.placement, new GiveInfo {
                        Container = mod.placement?.MainContainerType,
                        FlingType = FlingType.DirectDeposit,
                        MessageType = MessageType.Corner,
                        Transform = GameObject.Find(fsm.gameObject.name)?.transform,
                    });
                }
                else {
                    string boolName = shopItem.GetComponent<ShopItemStats>().GetPlayerDataBoolName();
                    PlayerData.instance.SetBool(boolName, true);
                }
            }

            void Pay() {
                int index = fsm.FsmVariables.FindFsmInt("Item Index").Value;
                GameObject shopItem = fsm.transform.parent.parent.Find("Item List").GetComponent<ShopMenuStock>().stockInv[index];
                var mod = shopItem.GetComponent<ModShopItemStats>();
                var stats = shopItem.GetComponent<ShopItemStats>();

                if(mod) {
                    Cost? cost = mod.cost;
                    if(cost is null || cost.Paid)
                        return;
                    cost.Pay();
                }
                else {
                    int cost = stats.GetCost();
                    if(cost > 0) {
                        HeroController.instance.TakeGeo(cost);
                    }
                }
            }

            Lambda give = new(Give);
            Lambda pay = new(Pay);

            deductSet.SetActions(give, pay);
        }

        public static void HastenItemListControl(PlayMakerFSM fsm) {
            FsmState menuDown = fsm.GetState("Menu Down");
            FsmState blankName = fsm.GetState("Blank Name and Desc");
            FsmState activateConfirm = fsm.GetState("Activate confirm");

            void ReduceFadeOutTime() {
                var fade = fsm.FsmVariables.FindFsmGameObject("Parent").Value.GetComponent<FadeGroup>();
                fade.fadeOutTimeFast = fade.fadeOutTime = 0.01f;
            }
            menuDown.AddFirstAction(new Lambda(ReduceFadeOutTime));
            menuDown.GetFirstActionOfType<Wait>().time = 0.01f;
            foreach(var a in menuDown.GetActionsOfType<SendEventByName>()) {
                if(a.sendEvent.Value == "DOWN") {
                    a.sendEvent.Value = "DOWN INSTANT";
                }
            }

            void ReduceFadeInTime() {
                var fade = fsm.FsmVariables.FindFsmGameObject("Confirm").Value.GetComponent<FadeGroup>();
                fade.fadeInTime = 0.01f;
            }

            blankName.AddLastAction(new Lambda(ReduceFadeInTime));
            activateConfirm.GetFirstActionOfType<Wait>().time = 0.01f;
        }

        public static void HastenConfirmControl(PlayMakerFSM fsm) {
            FsmState particles = fsm.GetState("Particles");
            particles.GetFirstActionOfType<Wait>().time = 0.2f;
            FsmState bob = fsm.GetState("Bob");
            bob.SetActions(bob.Actions[0], bob.Actions[1]);
            FsmState specialType = fsm.GetState("Special Type?");
            bob.Transitions[0].SetToState(specialType);

            //FsmState thankFade = fsm.GetState("Thank Fade");
            //thankFade.GetFirstActionOfType<SendEventByName>().sendEvent.Value = "DOWN INSTANT";
            //thankFade.GetFirstActionOfType<Wait>().time = 0.01f;
        }

        public static void HastenUIList(PlayMakerFSM fsm) {
            FsmState selectionMade = fsm.GetState("Selection Made");
            FsmState selectionMadeCancel = fsm.GetState("Selection Made Cancel");
            selectionMade.GetFirstActionOfType<Wait>().time = 0.01f;
            selectionMadeCancel.GetFirstActionOfType<Wait>().time = 0.01f;
        }

        public static void HastenUIListGetInput(PlayMakerFSM fsm) {
            FsmState confirm = fsm.GetState("Confirm");
            FsmState cancel = fsm.GetState("Cancel");
            confirm.GetFirstActionOfType<Wait>().time = 0.01f;
            cancel.GetFirstActionOfType<Wait>().time = 0.01f;

            FsmState stillUp = fsm.GetState("Still Up?");
            FsmState stillLeft = fsm.GetState("Still Left?");
            FsmState stillRight = fsm.GetState("Still Right?");
            FsmState stillDown = fsm.GetState("Still Down?");
            stillUp.GetFirstActionOfType<Wait>().time = 0.15f;
            stillLeft.GetFirstActionOfType<Wait>().time = 0.15f;
            stillRight.GetFirstActionOfType<Wait>().time = 0.15f;
            stillDown.GetFirstActionOfType<Wait>().time = 0.15f;

            FsmState repeatUp = fsm.GetState("Repeat Up");
            FsmState repeatLeft = fsm.GetState("Repeat Left");
            FsmState repeatRight = fsm.GetState("Repeat Right");
            FsmState repeatDown = fsm.GetState("Repeat Down");
            repeatUp.GetFirstActionOfType<Wait>().time = 0.1f;
            repeatLeft.GetFirstActionOfType<Wait>().time = 0.1f;
            repeatRight.GetFirstActionOfType<Wait>().time = 0.1f;
            repeatDown.GetFirstActionOfType<Wait>().time = 0.1f;
        }

        public static void HastenUIListButtonListen(PlayMakerFSM fsm) {
            FsmState selectPressed = fsm.GetState("Select Pressed");
            FsmState cancelPressed = fsm.GetState("Cancel Pressed");

            selectPressed.GetFirstActionOfType<Wait>().time = 0.1f;
            cancelPressed.GetFirstActionOfType<Wait>().time = 0.1f;
        }
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.