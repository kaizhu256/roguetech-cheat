using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RoguetechCheat
{
    public class
    Dict2 : SortedList<string, string>
    {
        public string
        getItem(string key)
        {
            TryGetValue(key, out string val);
            return val ?? "";
        }

        public void
        setItem(string key, object val)
        {
            var val2 = (
                val == null
                ? ""
                : val is string tstring
                ? (string)val
                : val is bool tbool
                ? (
                    (bool)val
                    ? "1"
                    : ""
                )
                : val.ToString()
            ).Trim();
            val2 = Regex.Replace(val2, "^true$", "1", RegexOptions.IgnoreCase);
            val2 = Regex.Replace(
                val2,
                "^false$",
                "",
                RegexOptions.IgnoreCase
            );
            this[key.ToLower()] = val2;
        }
    }

    public class
    Local
    {
        public static bool
        cheat_enginevalidation_off;

        public static bool
        cheat_mechcomponentsize_1;

        public static bool
        cheat_pilotabilitycooldown_0;

        public static bool
        cheat_shopnuke_on;

        public static int
        countJsonParse = 0;

        public static Newtonsoft.Json.JsonSerializerSettings
        jsonSerializerSettings;

        public static Dict2
        state;

        public static object
        debugInline(string name, object obj)
        {
            /*
             * this function will inline-debug <obj> to file ./debug.log
             */
            return debugLog(name, obj);
        }

        public static object
        debugLog(string name, object obj)
        {
            /*
             * this function will inline-debug <obj> to file ./debug.log
             * example usage
             * Local.debugLog("trace", System.Environment.StackTrace);
             */
            FileLog.Log("\ndebugLog " + name + " " + (
                obj is string tt
                ? (string)obj
                : Local.jsonStringify(obj)
            ));
            return obj;
        }

        public static void
        debugStack(string name)
        {
            /*
             * this function will log stack-trace
             */
            Local.debugLog(name, System.Environment.StackTrace);
        }

        public static Dict2
        jsonParseDict2(string json)
        {
            /*
             * this function will parse json into List<Dict2>
             */
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dict2>(
                json,
                Local.jsonSerializerSettings
            );
        }

        public static string
        jsonStringify(object obj)
        {
            /*
             * this function will stringify val
             */
            return Newtonsoft.Json.JsonConvert.SerializeObject(
                obj,
                Local.jsonSerializerSettings
            );
        }

        public static void
        stateChangedAfter()
        {
            /*
             * this function will run after state has changed
             */
            Local.debugLog("stateChangedAfter", Local.state);
        }

        public static void
        Init(string cwd, string settingsJson)
        {
            // init logging
            FileLog.logPath = System.IO.Path.Combine(cwd, "debug.log");
            System.IO.File.Delete(FileLog.logPath);
            // init jsonSerializerSettings
            Local.jsonSerializerSettings = new Newtonsoft.Json
            .JsonSerializerSettings
            {
                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            // init state from README.md
            Local.state = Local.jsonParseDict2(
                new Regex(@"```\w*?\n([\S\s]*?)\n```").Match(
                    System.IO.File.ReadAllText(
                        System.IO.Path.Combine(cwd, "README.md")
                    )
                ).Groups[1].ToString()
            );
            Local.state.setItem("cwd", cwd);
            Local.cheat_enginevalidation_off = (
                state["cheat_enginevalidation_off"] != ""
            );
            Local.cheat_mechcomponentsize_1 = (
                state["cheat_mechcomponentsize_1"] != ""
            );
            Local.cheat_pilotabilitycooldown_0 = (
                state["cheat_pilotabilitycooldown_0"] != ""
            );
            Local.cheat_shopnuke_on = (
                state["cheat_shopnuke_on"] != ""
            );
            // init settings.json
            try
            {
                foreach (var item in Local.jsonParseDict2(
                    System.IO.File.ReadAllText(
                        System.IO.Path.Combine(cwd, "settings.json")
                    )
                ))
                {
                    Local.state.setItem(item.Key, item.Value);
                }
            }
            catch (Exception err)
            {
                Local.debugLog("settings.json", err);
            }
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(cwd, "settings.json"),
                Local.jsonStringify(Local.state)
            );
            // init shopitem.csv
            Local.state.setItem(
                "shopitem.csv",
                System.IO.Path.Combine(cwd, "shopitem.csv")
            );
            Local.stateChangedAfter();
            // init harmony
            var harmony = HarmonyInstance.Create(
                "com.github.kaizhu256.RoguetechCheat"
            );
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // cheat_enginevalidation_off
            if (Local.state.getItem("cheat_enginevalidation_off") != "")
            {
                object engineSettings;
                engineSettings = typeof(MechEngineer.Control).GetField(
                    "settings",
                    BindingFlags.NonPublic | BindingFlags.Static
                ).GetValue(null);
                // EngineFeature.settings => Control.settings.Engine
                engineSettings = engineSettings.GetType().GetField(
                    "Engine",
                    BindingFlags.Instance | BindingFlags.Public
                ).GetValue(engineSettings);
                // set AllowMixingHeatSinkTypes
                Traverse.Create(engineSettings).Field(
                    "AllowMixingHeatSinkTypes"
                ).SetValue(true);
                // set EnforceRulesForAdditionalInternalHeatSinks
                Traverse.Create(engineSettings).Field(
                    "EnforceRulesForAdditionalInternalHeatSinks"
                ).SetValue(false);
                // set LimitEngineCoresToTonnage
                Traverse.Create(engineSettings).Field(
                    "LimitEngineCoresToTonnage"
                ).SetValue(false);
            }
        }
    }

    [HarmonyPatch(typeof(HBS.Util.JSONSerializationUtility))]
    [HarmonyPatch("RehydrateObjectFromDictionary")]
    [HarmonyPatch(new Type[] {
        typeof(object),
        typeof(Dictionary<string, object>),
        typeof(string),
        typeof(HBS.Stopwatch),
        typeof(HBS.Stopwatch),
        typeof(HBS.Util.JSONSerializationUtility.RehydrationFilteringMode),
        typeof(Func<string, bool>[])
    })]
    public class
    Patch_JSONSerializationUtility_RehydrateObjectFromDictionary
    {
        public static void
        Postfix(object target)
        {
            /*
            Local.countJsonParse += 1;
            if (Local.countJsonParse % 10000 == 0)
            {
                Local.debugLog("countJsonParse", Local.countJsonParse);
                Local.debugInline(
                    "countJsonParse",
                    System.Environment.StackTrace
                );
            }
            */
            // cheat_enginevalidation_off
            if (Local.cheat_enginevalidation_off)
            {
                var obj = (
                    target as CustomComponents.TagRestrictions
                );
                if (obj != null)
                {
                    obj.IncompatibleTags = new string[0];
                }
            }
            // cheat_mechcomponentsize_1
            if (Local.cheat_mechcomponentsize_1)
            {
                var obj = target as MechComponentDef;
                if (obj != null && obj.InventorySize > 1)
                {
                    Traverse.Create(obj).Property("InventorySize").SetValue(1);
                }
            }
            if (Local.cheat_mechcomponentsize_1)
            {
                var obj = (
                    target as MechEngineer.Features.DynamicSlots.DynamicSlots
                );
                if (obj != null)
                {
                    obj.ReservedSlots = 0;
                }
            }
            // cheat_pilotabilitycooldown_0
            if (Local.cheat_pilotabilitycooldown_0)
            {
                var obj = target as AbilityDef;
                if (obj != null && obj.ActivationCooldown > 1)
                {
                    Traverse.Create(obj).Property(
                        "ActivationCooldown"
                    ).SetValue(1);
                }
            }
        }
        /*
        public static bool
        Prefix(object target, Dictionary<string, object> values)
        {
            return true;
        }
        */
    }

    // patch - cheat_ammoboxcapacity_infinite
    [HarmonyPatch(typeof(AmmunitionBoxDef))]
    [HarmonyPatch("FromJSON")]
    public class
    Patch_AmmunitionBoxDef_FromJSON
    {
        public static void
        Postfix(AmmunitionBoxDef __instance)
        {
            if (Local.state.getItem("cheat_ammoboxcapacity_infinite") != "")
            {
                Traverse.Create(__instance).Property(
                    "Capacity"
                ).SetValue(5000);
            }
        }
    }

    // patch - cheat_armorinstall_free

    // patch - cheat_contractban_off

    // patch - cheat_contractreputationloss_cheap

    // patch - cheat_enginevalidation_off
    /*
    [HarmonyPatch(typeof(CustomComponents.DefaultHelper))]
    [HarmonyPatch("IsModuleFixed")]
    public class
    Patch_CustomComponents_DefaultHelper_IsModuleFixed
    {
        public static void
        Postfix(ref bool __result)
        {
            if (Local.state.getItem("cheat_enginevalidation_off") != "")
            {
                __result = false;
            }
        }
    }
    [HarmonyPatch(typeof(
        MechEngineer.Features.ArmActuators.ArmActuatorSupport
    ))]
    [HarmonyPatch("GetLimit")]
    public class
    Patch_MechEngineer_Features_ArmActuators_ArmActuatorSupport_GetLimit
    {
        public static void
        Postfix(ref MechEngineer.Features.ArmActuators.ArmActuatorSlot __result)
        {
            if (Local.state.getItem("cheat_enginevalidation_off") != "")
            {
                __result = (
                    MechEngineer.Features.ArmActuators.ArmActuatorSlot.Hand
                );
            }
        }
    }
    */

    // patch - cheat_introskip_on
    [HarmonyPatch(typeof(IntroCinematicLauncher))]
    [HarmonyPatch("Init")]
    public class
    Patch_IntroCinematicLauncher_Init
    {
        public static void
        Postfix(IntroCinematicLauncher __instance)
        {
            if (Local.state.getItem("cheat_introskip_on") != "")
            {
                Traverse.Create(__instance).Field("state").SetValue(3);
            }
        }
    }
    [HarmonyPatch(typeof(SplashLauncher))]
    [HarmonyPatch("OnStart")]
    public class
    Patch_SplashLauncher_OnStart
    {
        public static bool
        Prefix()
        {
            return Local.state.getItem("cheat_introskip_on") == "";
        }
    }
    [HarmonyPatch(typeof(SplashLauncher))]
    [HarmonyPatch("OnStep")]
    public class
    Patch_SplashLauncher_OnStep
    {
        public static bool
        Prefix()
        {
            return Local.state.getItem("cheat_introskip_on") == "";
        }
    }
    [HarmonyPatch(typeof(SplashLauncher))]
    [HarmonyPatch("Start")]
    public class
    Patch_SplashLauncher_Start
    {
        public static bool
        Prefix(SplashLauncher __instance)
        {
            if (Local.state.getItem("cheat_introskip_on") == "")
            {
                return true;
            }
            Traverse.Create(__instance).Field("currentState").SetValue(3);
            Traverse.Create(__instance)
                .Field("activate")
                .GetValue<ActivateAfterInit>()
                .enabled = true;
            return false;
        }
    }
    [HarmonyPatch(typeof(SplashLauncher))]
    [HarmonyPatch("Update")]
    public class
    Patch_SplashLauncher_Update
    {
        public static bool
        Prefix()
        {
            return Local.state.getItem("cheat_introskip_on") == "";
        }
    }

    // patch - cheat_mechcomponentsize_1
    /*
    [HarmonyPatch(typeof(MechComponentDef), MethodType.Constructor)]
    [HarmonyPatch(new Type[] {
        typeof(ComponentType),
        typeof(MechComponentType),
        typeof(EffectData[]),
        typeof(TagSet),
        typeof(DescriptionDef),
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(int),
        typeof(float),
        typeof(ChassisLocations),
        typeof(ChassisLocations),
        typeof(bool)
    })]
    public class
    Patch_MechComponentDef_constructor
    {
        public static bool
        Prefix(ref int InventorySize)
        {
            InventorySize = 1;
            return true;
        }
        public static void
        Postfix(MechComponentDef __instance)
        {
            Traverse.Create(__instance).Property(
                "InventorySize"
            ).SetValue(1);
        }
    }
    [HarmonyPatch(typeof(MechComponentDef), MethodType.Constructor)]
    [HarmonyPatch(new Type[] {
        typeof(MechComponentDef)
    })]
    public class
    Patch_MechComponentDef_constructor2
    {
        public static void
        Postfix(MechComponentDef __instance)
        {
            Traverse.Create(__instance).Property(
                "InventorySize"
            ).SetValue(1);
        }
    }
    */

    // patch - cheat_mechweightlimit_off
    [HarmonyPatch(typeof(MechValidationRules))]
    [HarmonyPatch("ValidateMechTonnage")]
    public class
    Patch_MechValidationRules_ValidateMechTonnage
    {
        public static void
        Postfix(
            ref Dictionary<MechValidationType, List<Localize.Text>>
            errorMessages
        )
        {
            if (Local.state.getItem("cheat_mechweightlimit_off") != "")
            {
                errorMessages[MechValidationType.Overweight].Clear();
            }
        }
    }

    // patch - cheat_pilotabilitycooldown_0

    // patch - cheat_pilotskill_reset
    [HarmonyPatch(typeof(SGBarracksMWDetailPanel))]
    [HarmonyPatch("OnSkillsSectionClicked")]
    public class
    Patch_SGBarracksMWDetailPanel_OnSkillsSectionClicked
    {
        public static bool
        Prefix(SGBarracksMWDetailPanel __instance, Pilot ___curPilot)
        {
            if (
                Local.state.getItem("cheat_pilotskill_reset") == ""
                || !(
                    Input.GetKey(KeyCode.LeftShift)
                    || Input.GetKey(KeyCode.RightShift)
                )
            )
            {
                return true;
            }
            GenericPopupBuilder
                .Create(
                    "Pilot Reskill",
                    "This will set skills to 1 and refund all XP."
                )
                .AddButton("Cancel")
                .AddButton("Pilot Reskill", () =>
                {
                    PilotReskill(__instance, ___curPilot);
                })
                .CancelOnEscape()
                .AddFader(
                    HBS.LazySingletonBehavior<UIManager>
                        .Instance
                        .UILookAndColorConstants
                        .PopupBackfill
                )
                .Render();
            return false;
        }
        public static void
        PilotReskill(SGBarracksMWDetailPanel __instance, Pilot ___curPilot)
        {
            var sim = UnityGameInstance.BattleTechGame.Simulation;
            var pilotDef = ___curPilot.pilotDef.CopyToSim();
            foreach (var val in sim.Constants.Story.CampaignCommanderUpdateTags)
            {
                if (!sim.CompanyTags.Contains(val))
                {
                    sim.CompanyTags.Add(val);
                }
            }
            // save xpUsed
            var xpUsed = (
                sim.GetLevelRangeCost(1, pilotDef.SkillPiloting - 1)
                + sim.GetLevelRangeCost(1, pilotDef.SkillGunnery - 1)
                + sim.GetLevelRangeCost(1, pilotDef.SkillGuts - 1)
                + sim.GetLevelRangeCost(1, pilotDef.SkillTactics - 1)
            );
            // handle xpUsed overflow
            if (xpUsed < 0)
            {
                xpUsed = 0x40000000;
            }
            // reset ___curPilot
            Traverse.Create(pilotDef).Property("BasePiloting").SetValue(1);
            Traverse.Create(pilotDef).Property("BaseGunnery").SetValue(1);
            Traverse.Create(pilotDef).Property("BaseGuts").SetValue(1);
            Traverse.Create(pilotDef).Property("BaseTactics").SetValue(1);
            Traverse.Create(pilotDef).Property("BonusPiloting").SetValue(1);
            Traverse.Create(pilotDef).Property("BonusGunnery").SetValue(1);
            Traverse.Create(pilotDef).Property("BonusGuts").SetValue(1);
            Traverse.Create(pilotDef).Property("BonusTactics").SetValue(1);
            pilotDef.abilityDefNames.Clear();
            pilotDef.SetSpentExperience(0);
            pilotDef.ForceRefreshAbilityDefs();
            pilotDef.ResetBonusStats();
            ___curPilot.FromPilotDef(pilotDef);
            // reset xpUsed
            ___curPilot.AddExperience(0, "reset", xpUsed);
            // ___curPilot.AddExperience(0, "reset", 1234567);
            __instance.DisplayPilot(___curPilot);
        }
    }

    // patch - cheat_pilotskillcost_low

    // patch - cheat_pilotxpnag_off
    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("ShowMechWarriorTrainingNotif")]
    public class
    Patch_SimGameState_ShowMechWarriorTrainingNotif
    {
        public static bool
        Prefix(SimGameState __instance)
        {
            if (Local.state.getItem("cheat_pilotxpnag_off") == "")
            {
                return true;
            }
            return false;
        }
    }

    // patch - cheat_salvagefullmech_on
    [HarmonyPatch(
        typeof(CustomSalvage.ChassisHandler.AssemblyChancesResult),
        MethodType.Constructor
    )]
    [HarmonyPatch(new Type[] {
        typeof(MechDef),
        typeof(SimGameState),
        typeof(int)
    })]
    public class
    Patch_AssemblyChancesResult_constructor
    {
        public static void
        Postfix(CustomSalvage.ChassisHandler.AssemblyChancesResult __instance)
        {
            if (Local.state.getItem("cheat_salvagefullmech_on") != "")
            {
                Traverse.Create(__instance).Property(
                    "LimbChance"
                ).SetValue(Math.Max(__instance.LimbChance, 1.0f));
                Traverse.Create(__instance).Property(
                    "CompFChance"
                ).SetValue(Math.Max(__instance.CompFChance, 1.0f));
                Traverse.Create(__instance).Property(
                    "CompNFChance"
                ).SetValue(Math.Max(__instance.CompNFChance, 1.0f));
            }
        }
    }
    [HarmonyPatch(typeof(CustomSalvage.PartsNumCalculations))]
    [HarmonyPatch("PartDestroyed")]
    public class
    Patch_CustomSalvage_PartsNumCalculations_PartDestroyed
    {
        public static void
        Postfix(ref int __result)
        {
            if (Local.state.getItem("cheat_salvagemechparts_all") != "")
            {
                __result = 2;
            }
        }
    }

    // patch - cheat_salvagetotal_300

    // patch - cheat_shopnuke_on
    [HarmonyPatch(typeof(NaturalStringComparer))]
    [HarmonyPatch("Compare")]
    public class
    Patch_NaturalStringComparer_Compare
    {
        public static Regex _re = new Regex(
            "(?<=\\D)(?=\\d)|(?<=\\d)(?=\\D)",
            RegexOptions.Compiled
        );

        public static int PartCompare(string x, string y)
        {
            int num;
            int value;
            if (int.TryParse(x, out num) && int.TryParse(y, out value))
            {
                return num.CompareTo(value);
            }
            return x.CompareTo(y);
        }

        public static int Compare(InventoryDataObject_BASE a, InventoryDataObject_BASE b)
        {
            string text = a.GetItemType().ToString() + '.' + a.GetId();
            string text2 = b.GetItemType().ToString() + '.' + b.GetId();
            if (text == null)
            {
                text = "";
            }
            if (text2 == null)
            {
                text2 = "";
            }
            text = text.ToLower();
            text2 = text2.ToLower();
            if (string.Compare(text, 0, text2, 0, Math.Min(text.Length, text2.Length)) != 0)
            {
                string[] array = _re.Split(text);
                string[] array2 = _re.Split(text2);
                int num = 0;
                int num2;
                for (; ; )
                {
                    num2 = PartCompare(array[num], array2[num]);
                    if (num2 != 0)
                    {
                        break;
                    }
                    num++;
                }
                return num2;
            }
            if (text.Length == text2.Length)
            {
                return 0;
            }
            if (text.Length >= text2.Length)
            {
                return 1;
            }
            return -1;
        }

        public static void
        Postfix(
            InventoryDataObject_BASE a,
            InventoryDataObject_BASE b,
            ref int __result
        )
        {
            if (Local.cheat_shopnuke_on)
            {
                __result = Compare(a, b);
                return;
            }
        }
    }
    [HarmonyPatch(typeof(SG_Shop_Screen))]
    [HarmonyPatch("AddShopInventory")]
    public class
    Patch_SG_Shop_Screen_AddShopInventory
    {
        public static void
        Postfix(
            MechLabInventoryWidget_ListView ___inventoryWidget,
            SG_Shop_Screen __instance,
            StarSystem ___theSystem,
            bool ___isInBuyingState,
            Shop shop
        )
        {
            if (!Local.cheat_shopnuke_on)
            {
                return;
            }
            if (
                !___isInBuyingState
                || shop != ___theSystem.SystemShop
            )
            {
                return;
            }
            foreach (
                var item in
                System.IO.File.ReadAllText(Local.state.getItem("shopitem.csv"))
                .Replace("\r", "")
                .Trim()
                .Split('\n')
            )
            {
                string shopItemType;
                shopItemType = item.Split(',')[0];
                __instance.AddShopItemToWidget(
                    new ShopDefItem(
                        item.Split(',')[1], // string ID
                        (
                            shopItemType == "AmmunitionBox"
                            ? ShopItemType.AmmunitionBox
                            : shopItemType == "HeatSink"
                            ? ShopItemType.HeatSink
                            : shopItemType == "JumpJet"
                            ? ShopItemType.JumpJet
                            : shopItemType == "Mech"
                            ? ShopItemType.Mech
                            : shopItemType == "MechPart"
                            ? ShopItemType.MechPart
                            : shopItemType == "Reference"
                            ? ShopItemType.Reference
                            : shopItemType == "Upgrade"
                            ? ShopItemType.Upgrade
                            : shopItemType == "Weapon"
                            ? ShopItemType.Weapon
                            : ShopItemType.None
                        ), // ShopItemType Type
                        1.0f, // float DiscountModifier
                        0, // int Count
                        true, // bool IsInfinite
                        false, // bool IsDamaged
                        0 // int SellCost
                    ),
                    shop,
                    ___inventoryWidget
                );
            }
        }
    }

    // patch - difficulty_settings
    [HarmonyPatch(typeof(SimGameDifficultySettingList))]
    [HarmonyPatch("FromJSON")]
    public class
    Patch_SimGameDifficultySettingList_FromJSON
    {
        public static bool
        Prefix(ref string json)
        {
            var ii = json.LastIndexOf("]");
            json = json.Substring(0, ii) + @",
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_armorinstall_free"",
    ""Name"": ""cheat_armorinstall_free"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""ArmorInstallCost"",
                    ""ConstantType"": ""MechLab"",
                    ""ConstantValue"": ""0""
                },
                {
                    ""ConstantName"": ""ArmorInstallTechPoints"",
                    ""ConstantType"": ""MechLab"",
                    ""ConstantValue"": ""0""
                }
            ],
            ""ID"": ""cheat_armorinstall_free_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_armorinstall_free"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
},
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_contractreputationloss_low"",
    ""Name"": ""cheat_contractreputationloss_low"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""TargetRepGoodFaithMod"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""0""
                },
                {
                    ""ConstantName"": ""TargetRepSuccessMod"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""-0.25""
                },
                {
                    ""ConstantName"": ""TargetRepGoodFaithMod"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""0""
                },
                {
                    ""ConstantName"": ""TargetRepSuccessMod"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""-0.25""
                }
            ],
            ""ID"": ""cheat_contractreputationloss_low_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_contractreputationloss_low"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
},
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_contractban_off"",
    ""Name"": ""cheat_contractban_off"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""DislikedMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""FriendlyMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""HatedMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""HonoredMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""IndifferentMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""LikedMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""LoathedMaxContractDifficulty"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""DislikedMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""FriendlyMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""HatedMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""HonoredMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""IndifferentMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""LikedMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                },
                {
                    ""ConstantName"": ""LoathedMaxContractDifficulty"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""10""
                }
            ],
            ""ID"": ""cheat_contractban_off_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_contractban_off"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
},
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_pilotskillcost_low"",
    ""Name"": ""cheat_pilotskillcost_low"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""PilotLevelCostExponent"",
                    ""ConstantType"": ""Pilot"",
                    ""ConstantValue"": ""1.1""
                }
            ],
            ""ID"": ""cheat_pilotskillcost_low_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_pilotskillcost_low"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
},
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_salvagefullmech_on"",
    ""Name"": ""cheat_salvagefullmech_on"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""DefaultMechPartMax"",
                    ""ConstantType"": ""CareerMode"",
                    ""ConstantValue"": ""1""
                },
                {
                    ""ConstantName"": ""DefaultMechPartMax"",
                    ""ConstantType"": ""Story"",
                    ""ConstantValue"": ""1""
                }
            ],
            ""ID"": ""cheat_salvagefullmech_on_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_salvagefullmech_on"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
},
{
    ""DefaultIndex"": 0,
    ""Enabled"": true,
    ""ID"": ""cheat_salvagetotal_300"",
    ""Name"": ""cheat_salvagetotal_300"",
    ""Options"": [
        {
            ""DifficultyConstants"": [
                {
                    ""ConstantName"": ""ContractFloorSalvageBonus"",
                    ""ConstantType"": ""Finances"",
                    ""ConstantValue"": ""300""
                },
                {
                    ""ConstantName"": ""PrioritySalvageModifier"",
                    ""ConstantType"": ""Salvage"",
                    ""ConstantValue"": ""0""
                }
            ],
            ""ID"": ""cheat_salvagetotal_300_on"",
            ""Name"": ""On"",
            ""TelemetryEventDesc"": ""1""
        }
    ],
    ""StartOnly"": false,
    ""TelemetryEventName"": ""cheat_salvagetotal_300"",
    ""Toggle"": false,
    ""Tooltip"": """",
    ""UIOrder"": 1000,
    ""Visible"": false
}
            " + json.Substring(ii);
            return true;
        }
    }
    [HarmonyPatch(typeof(SimGameConstantOverride))]
    [HarmonyPatch("AddOverride")]
    [HarmonyPatch(new Type[] {
        typeof(SimGameDifficulty.DifficultySetting),
        typeof(int),
        typeof(bool)
    })]
    public class
    Patch_SimGameConstantOverride_AddOverride1
    {
        public static bool
        Prefix(SimGameDifficulty.DifficultySetting setting)
        {
            /*
             * this function will enable/disable difficulty-settings-cheats
             * based on settings.json
             */
            if (
                setting.ID.IndexOf("cheat_") == 0
                && Local.state.getItem(setting.ID) == ""
            )
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(SimGameConstantOverride))]
    [HarmonyPatch("AddOverride")]
    [HarmonyPatch(new Type[] {
        typeof(string),
        typeof(string),
        typeof(object),
        typeof(bool)
    })]
    public class
    Patch_SimGameConstantOverride_AddOverride2
    {
        public static bool
        Prefix(string constantType, ref string key, object value)
        {
            /*
             * this function will save difficulty-settings to Local.state
             */
            Local.state.setItem(constantType + "_" + key, value);
            return true;
        }
    }
    [HarmonyPatch(typeof(SimGameDifficulty))]
    [HarmonyPatch("ApplyAllSettings")]
    public class
    Patch_SimGameDifficulty_ApplyAllSettings
    {
        public static void
        Postfix()
        {
            Local.stateChangedAfter();
        }
    }
    [HarmonyPatch(typeof(SimGameDifficultySettingsModule))]
    [HarmonyPatch("SaveSettings")]
    public class
    Patch_SimGameDifficultySettingsModule_SaveSettings
    {
        public static void
        Postfix()
        {
            Local.stateChangedAfter();
        }
    }
}
