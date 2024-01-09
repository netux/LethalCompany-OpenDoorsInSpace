using HarmonyLib;
using System.Collections.Generic;

namespace OpenDoorsInSpace.Patches
{
	[HarmonyPatch]
	public class Patches
	{
        [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Update))]
        [HarmonyPostfix]
        public static void HanagerShipDoor_Update_Postfix(HangarShipDoor __instance)
        {
            if (!__instance.triggerScript.interactable && StartOfRound.Instance.inShipPhase)
            {
                __instance.triggerScript.interactable = true;
            }
        }

        [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.PlayDoorAnimation))]
        [HarmonyPrefix]
        public static bool HangarShipDoor_PlayDoorAnimation_Prefix(bool closed)
        {
            Plugin.Log.LogDebug($"HangarShipDoor.PlayDoorAnimation(closed: {closed}) Prefix");
            if (!closed && StartOfRound.Instance.inShipPhase)
            {
                Plugin.Manager.EjectDueToNegligance();
            }
            return true;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndPlayersFiredSequenceClientRpc))]
        [HarmonyPrefix]
        public static void StartOfRound_EndPlayersFiredSequenceClientRpc_Prefix()
        {
            Plugin.Log.LogDebug($"StartOfRound.EndPlayersFiredSequenceClientRpc() Prefix");
            Plugin.Manager.ResetEjectingDueToNegligance();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.playersFiredGameOver), MethodType.Enumerator)]
        [HarmonyPrefix]
        public static void StartOfRound_playersFiredGameOver_Enumerator_Prefix(IEnumerator<object> __instance)
        {
            var stateField = AccessTools.DeclaredField(__instance.GetType(), "<>1__state");
            var currentField = AccessTools.DeclaredField(__instance.GetType(), "<>2__current");

            Plugin.Log.LogDebug($"stateField: {stateField.GetValue(__instance)}");
            Plugin.Log.LogDebug($"currentField: {currentField.GetValue(__instance)}");

            if (Plugin.Manager != null && Plugin.Manager.IsEjectingDueToNegligence && ((int)stateField.GetValue(__instance)) < 2)
            {
                // Skip 5s wait, alarm and fired speech sfx, and another ~9s wait before opening the doors
                stateField.SetValue(__instance, 2);
            }
        }
    }
}
