using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LowCountProducts
{
    [BepInPlugin("SupermarketTogether.plugins.lowcountproducts", "LowCountProducts", "0.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        public static Plugin Instance;
        
        // ==== Plugin info
        private const string PluginGuid = "SupermarketTogether.plugins.lowcountproducts";
        private const string PluginName = "LowCountProducts";
        private const string PluginVersion = "0.0.2";
        
        // ==== Keyboard shortcut
        public static string KeyboardShortcutLowCountProductsKey = "LowCountProducts Toggle";
        public static ConfigEntry<KeyboardShortcut> KeyboardShortcutLowCountProducts;
        public static string KeyboardShortcutAddLowCountProductsKey = "AddLowCountProducts Key";
        public static ConfigEntry<KeyboardShortcut> KeyboardShortcutAddLowCountProducts;

        // ==== Notification stuff
        public static bool Notify = false;
        public static string NotificationType;
        
        // ==== Other stuff
        private static readonly Harmony Harmony = new Harmony("SupermarketTogether.plugins.lowcountproducts");
        public static ManualLogSource Log = new ManualLogSource("LowCountProducts");
        public static bool LowCountProducts = true;
        public static bool AddLowCountProducts = false;

        private void Awake()
        {
            Instance = this;
            
            KeyboardShortcutLowCountProducts = ((BaseUnityPlugin)this).Config.Bind<KeyboardShortcut>("General", KeyboardShortcutLowCountProductsKey, new KeyboardShortcut((KeyCode)98, Array.Empty<KeyCode>()), (ConfigDescription)null);
            KeyboardShortcutAddLowCountProducts = ((BaseUnityPlugin)this).Config.Bind<KeyboardShortcut>("General", KeyboardShortcutAddLowCountProductsKey, new KeyboardShortcut((KeyCode)98, (KeyCode[])(object)new KeyCode[1] { (KeyCode)306 }), (ConfigDescription)null);
            
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo((object)$"{PluginName}: {PluginGuid}, Version: {PluginVersion} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo((object)$"{PluginName}: {PluginGuid}, Version: {PluginVersion} is loaded.");
            Log = Logger;
        }
        
        private void Update()
        {
            if (KeyboardShortcutLowCountProducts.Value.IsDown())
            {
                LowCountProducts = !LowCountProducts;
                NotificationType = "lowCountToggle";
                Notify = true;
                return;
            }

            if (KeyboardShortcutAddLowCountProducts.Value.IsDown())
            {
                if (!AddLowCountProducts)
                {
                    // Logger.LogInfo($"{PluginName}: Adding low count products to cart.");
                    AddLowCountProducts = true;
                }
            }
        }
    }
}

namespace LowCountProducts.Patches
{
    [HarmonyPatch(typeof(ManagerBlackboard))]
    internal class LowCountProductsManagerBlackboardPatch
    {
        [HarmonyPatch("FixedUpdate")]
        [HarmonyPostfix]
        public static void LowCountProductsPostfix(ManagerBlackboard __instance)
        {
            if (LowCountProducts.Plugin.AddLowCountProducts)
            {
                LowCountProducts.Plugin.AddLowCountProducts = false;

                Dictionary<int, Dictionary<string, object>> lowProductList = new Dictionary<int, Dictionary<string, object>>();

                ProductListing productListing = __instance.GetComponent<ProductListing>();
                if (productListing == null)
                {
                    LowCountProducts.Plugin.Logger.LogError("ProductListing component not found on ManagerBlackboard.");
                    return;
                }

                // Iterate over unlocked products
                foreach (int productID in productListing.availableProducts)
                {
                    GameObject productPrefab = productListing.productPrefabs[productID];
                    if (productPrefab != null)
                    {
                        int[] quantities = GetProductsExistences(__instance, productID);

                        int shelvesQuantity = quantities[0];
                        int storageQuantity = quantities[1];
                        int boxesQuantity = quantities[2];

                        if (shelvesQuantity <= 10 && storageQuantity == 0 && boxesQuantity == 0)
                        {
                            if (!lowProductList.ContainsKey(productID))
                            {
                                string price = GetProductPrice(__instance, productListing, productID);
                                Dictionary<string, object> productInfo = new Dictionary<string, object>
                                {
                                    { "ID", productID },
                                    { "price", price }
                                };

                                lowProductList[productID] = productInfo;
                            }
                        }
                    }
                }

                // Add low count products to cart
                AddProductsToCart(__instance, lowProductList);

                if (lowProductList.Count > 0)
                {
                    LowCountProducts.Plugin.NotificationType = "lowCountAddToCart";
                    LowCountProducts.Plugin.Notify = true;
                }
            }
        }
        
        private static int[] GetProductsExistences(ManagerBlackboard instance, int productIDToCompare)
        {
            // Use Harmony's AccessTools to access the private method
            MethodInfo method = AccessTools.Method(typeof(ManagerBlackboard), "GetProductsExistences");
            if (method != null)
            {
                object result = method.Invoke(instance, new object[] { productIDToCompare });
                return (int[])result;
            }
            else
            {
                LowCountProducts.Plugin.Logger.LogError("Could not find method GetProductsExistences");
                return new int[3];
            }
        }
        
        private static string GetProductPrice(ManagerBlackboard instance, ProductListing productListing, int productID)
        {
            GameObject productPrefab = productListing.productPrefabs[productID];

            float basePricePerUnit = productPrefab.GetComponent<Data_Product>().basePricePerUnit;
            int productTier = productPrefab.GetComponent<Data_Product>().productTier;

            float inflationFactor = productListing.tierInflation[productTier];
            float pricePerUnit = Mathf.Round(basePricePerUnit * inflationFactor * 100f) / 100f;

            int maxItemsPerBox = productPrefab.GetComponent<Data_Product>().maxItemsPerBox;

            float boxPrice = Mathf.Round(pricePerUnit * maxItemsPerBox * 100f) / 100f;

            return "$" + boxPrice.ToString("F2", CultureInfo.InvariantCulture);
        }
        
        private static void AddProductsToCart(ManagerBlackboard manager, Dictionary<int, Dictionary<string, object>> lowProductList)
        {
            foreach (var product in lowProductList)
            {
                var productInfo = product.Value;
                int productID = (int)productInfo["ID"];
                
                // Check if product is already in shopping list
                if (IsProductInShoppingList(manager, productID))
                {
                    // Skip adding this product
                    continue;
                }
                
                string productPriceText = productInfo["price"].ToString().Replace("$", "").Replace(",", ".");
                if (float.TryParse(productPriceText, NumberStyles.Float, CultureInfo.InvariantCulture, out float finalProductPrice))
                {
                    manager.AddShoppingListProduct(productID, finalProductPrice);
                }
            }
        }

        private static bool IsProductInShoppingList(ManagerBlackboard manager, int productID)
        {
            foreach (Transform item in manager.shoppingListParent.transform)
            {
                InteractableData data = item.GetComponent<InteractableData>();
                if (data != null && data.thisSkillIndex == productID)
                {
                    // Product is already in the shopping list
                    return true;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GameCanvas))]
    internal class NotificationHandler
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void NotificationHandler_Postfix(GameCanvas __instance, ref bool ___inCooldown)
        {
            if (LowCountProducts.Plugin.Notify)
            {
                ___inCooldown = false;
                LowCountProducts.Plugin.Notify = false;
                string text = "`";
                switch (LowCountProducts.Plugin.NotificationType)
                {
                    case "lowCountToggle":
                        text = text + "Low Count Products: " + (LowCountProducts.Plugin.LowCountProducts ? "ON" : "OFF");
                        break;
                    case "lowCountAddToCart":
                        text = text + "Low Count Products: Added almost out of stock products to cart.";
                        break;
                }

                __instance.CreateCanvasNotification(text);
            }
        }
    }
    
    [HarmonyPatch(typeof(LocalizationManager))]
    internal class LocalizationHandler
    {
        [HarmonyPatch("GetLocalizationString")]
        [HarmonyPrefix]
        public static bool noLocalization_Prefix(ref string key, ref string __result)
        {
            if (key[0] == '`')
            {
                __result = key.Substring(1);
                return false;
            }
            return true;
        }
    }
}