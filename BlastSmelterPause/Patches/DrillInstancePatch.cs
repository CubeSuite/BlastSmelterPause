using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastSmelterPause.Patches
{
    internal class DrillInstancePatch
    {
        [HarmonyPatch(typeof(DrillInstance), "ExplosivesReady")]
        [HarmonyPrefix]
        static bool ReplaceExplosivesReady(ref DrillInstance __instance, ref bool __result) {
            if (__instance.GetOutputInventory().myStacks[0].isEmpty) return true;
            
            ResourceInfo minedResource = __instance.GetOutputInventory().myStacks[0].info;
            int maxStack = __instance.GetOutputInventory().myStacks[0].maxStack;
            int have = __instance.GetOutputInventory().GetResourceCount(minedResource.uniqueId);

            if (have == maxStack) {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
