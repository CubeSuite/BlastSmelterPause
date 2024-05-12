using FIMSpace;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlastSmelterPause.Patches
{
    internal class BlastSmelterInstancePatch
    {
        [HarmonyPatch(typeof(BlastSmelterInstance), "UpdateCrafting")]
        [HarmonyPrefix]
        static bool ReplaceUpdateCrafting(ref BlastSmelterInstance __instance, float dt) {
            __instance.cooldown -= dt / __instance.myDef.runtimeSettings.cooldown;

            ref Inventory outputInventory = ref __instance.GetOutputInventory();
            ref Inventory inputInventory = ref __instance.GetInputInventory();

            if(__instance.targetRecipe != null) {
                if (outputInventory.GetResourceCount(__instance.targetRecipe.outputTypes[0].uniqueId) == __instance.targetRecipe.outputTypes[0].maxStackCount) return false;
            }

            MethodInfo hasResourcesToCraftInfo = __instance.GetType().GetMethod("HasResourcesToCraft", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo refreshHasFuelInfo = __instance.GetType().GetMethod("RefreshHasFuel", BindingFlags.NonPublic | BindingFlags.Instance);
            refreshHasFuelInfo.Invoke(__instance, new object[0]);

            MethodInfo consumeFuelInfo = __instance.GetType().GetMethod("ConsumeFuel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (__instance.targetRecipe != null && TechTreeState.instance.IsRecipeKnown(__instance.targetRecipe) && __instance.cooldown <= 0f && (bool)hasResourcesToCraftInfo.Invoke(__instance, new object[0]) && (bool)consumeFuelInfo.Invoke(__instance, new object[0])) {
                __instance.cooldown = 0f;
                refreshHasFuelInfo.Invoke(__instance, new object[0]);

                if (__instance.targetRecipe != null && __instance.targetRecipe.craftingMethod == CraftingMethod.BlastSmelter) {
                    __instance.lastOutputAmount = 0;
                    __instance.lastOutputResource = __instance.targetRecipe.outputTypes[0].uniqueId;
                    int blastSmelterIterations = GameState.instance.blastSmelterIterations;
                    int remaining = __instance.targetRecipe.outputTypes[0].maxStackCount - outputInventory.GetResourceCount(__instance.lastOutputResource);
                    if (blastSmelterIterations > remaining) blastSmelterIterations = remaining / __instance.targetRecipe.outputQuantities[0];

                    for (int i = 0; i < blastSmelterIterations; i++) {
                        if ((bool)hasResourcesToCraftInfo.Invoke(__instance, new object[0])) {

                            MethodInfo takeResourcesInfo = __instance.GetType().GetMethod("TakeResourcesToCraft", BindingFlags.NonPublic | BindingFlags.Instance);
                            takeResourcesInfo.Invoke(__instance, new object[] { false });

                            int num = 0;
                            while (num < __instance.targetRecipe.outputQuantities.Length && num < __instance.targetRecipe.outputTypes.Length) {
                                if (outputInventory.CanAddResources(__instance.targetRecipe.outputTypes[num].uniqueId, __instance.targetRecipe.outputQuantities[num], null)) {
                                    outputInventory.AddResources(__instance.targetRecipe.outputTypes[num], __instance.targetRecipe.outputQuantities[num], true);
                                    __instance.lastOutputAmount += __instance.targetRecipe.outputQuantities[num];
                                }
                                if (__instance.commonInfo.protectionStatus == ProtectionStatus.None) {
                                    PlayerQuestSystem.instance.OnSmelterCraft(__instance.targetRecipe.outputTypes[num], __instance.targetRecipe.outputQuantities[num]);
                                }
                                num++;
                            }
                        }
                    }
                }
                __instance.cooldown = 1f;
                __instance.queuedBlastAnim = true;
            }
            if (inputInventory.isEmpty && outputInventory.isEmpty) {
                __instance.targetRecipe = null;
            }

            return false;
        }
    }
}
