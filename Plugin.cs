using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenDoorsInSpace
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ManualLogSource Log;
        internal static OpenDoorsInSpacePluginManager Manager;

        public Plugin()
        {
            Instance = this;
            Log = base.Logger;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SampleSceneRelay" && mode == LoadSceneMode.Single)
            {
                if (Manager?.gameObject == null)
                {
                    var gameObject = new GameObject("OpenDoorsInSpace", [typeof(OpenDoorsInSpacePluginManager)]);
                    Instantiate(
                        gameObject,
                        gameObject.transform.position,
                        gameObject.transform.rotation,
                        scene.GetRootGameObjects()[0].transform
                    );
                    Manager = gameObject.GetComponent<OpenDoorsInSpacePluginManager>();
                }
            }
        }
    }

    public class OpenDoorsInSpacePluginManager : MonoBehaviour
    {
        public bool IsEjectingDueToNegligence { get; internal set; }

        private TextMeshProUGUI youAreFiredDueTopNegliganceSubtextTMP;

        void Start()
        {
            Plugin.Log.LogDebug("OpenDoorsInSpacePluginManager Start");

            youAreFiredDueTopNegliganceSubtextTMP = CreateYouAreFiredDueToNegliganceSubtext();
        }

        private GameObject GetYouAreFiredOriginalReasonSubtextGameObject()
        {
            return GameObject.Find("/Systems/UI/Canvas/GameOverScreen/MaskImage/HeaderText (1)");
        }

        private TextMeshProUGUI CreateYouAreFiredDueToNegliganceSubtext()
        {
            var youAreFiredOriginalReasonSubtextGameObject = this.GetYouAreFiredOriginalReasonSubtextGameObject();
            var youAreFiredNegliganceReasonSubtextGameObject = Instantiate(youAreFiredOriginalReasonSubtextGameObject, youAreFiredOriginalReasonSubtextGameObject.transform.parent);
            youAreFiredNegliganceReasonSubtextGameObject.name = youAreFiredNegliganceReasonSubtextGameObject.name + " (OpenDoorsInSpace fired subtext)";
            youAreFiredNegliganceReasonSubtextGameObject.SetActive(false);

            youAreFiredDueTopNegliganceSubtextTMP = youAreFiredNegliganceReasonSubtextGameObject.GetComponent<TextMeshProUGUI>();
            youAreFiredDueTopNegliganceSubtextTMP.text = "Someone opened the hangar door while in space.";

            return youAreFiredDueTopNegliganceSubtextTMP;
        }

        public void EjectDueToNegligance()
        {
            if (IsEjectingDueToNegligence)
            {
                return;
            }

            IsEjectingDueToNegligence = true;

            StartOfRound.Instance.ManuallyEjectPlayersServerRpc();

            if (youAreFiredDueTopNegliganceSubtextTMP == null)
            {
                youAreFiredDueTopNegliganceSubtextTMP = CreateYouAreFiredDueToNegliganceSubtext();
            }
            this.GetYouAreFiredOriginalReasonSubtextGameObject()?.SetActive(false);
            youAreFiredDueTopNegliganceSubtextTMP.gameObject.SetActive(true);

        }

        public void ResetEjectingDueToNegligance()
        {
            // No need to do this as by this point the scene was already reloaded
            //this.GetYouAreFiredOriginalReasonSubtextGameObject()?.SetActive(true);
            //youAreFiredNegliganceReasonSubtextTMP?.gameObject.SetActive(false);

            IsEjectingDueToNegligence = false;
        }
    }

    [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Update))]
    public class HangarDoorUpdatePatch
    {
        public static void Postfix(HangarShipDoor __instance)
        {
            if (!__instance.triggerScript.interactable && StartOfRound.Instance.inShipPhase)
            {
                __instance.triggerScript.interactable = true;
            }
        }
    }

    [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.PlayDoorAnimation))]
    public class HangarDoorPlayDoorAnimationPatch
    {
        public static bool Prefix(bool closed)
        {
            Plugin.Log.LogDebug($"HangarShipDoor.PlayDoorAnimation(closed: {closed}) Prefix");
            if (!closed && StartOfRound.Instance.inShipPhase)
            {
                Plugin.Manager.EjectDueToNegligance();
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.EndPlayersFiredSequenceClientRpc))]
    public class StartOfRoundEndPlayersFiredSequenceClientRpcPatch
    {
        public static void Prefix()
        {
            Plugin.Log.LogDebug($"StartOfRound.EndPlayersFiredSequenceClientRpc() Prefix");
            Plugin.Manager.ResetEjectingDueToNegligance();
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.playersFiredGameOver), MethodType.Enumerator)]
    public class StartOfRoundPlayersFiredGameOverEnumeratorPatch
    {
        public static void Prefix(IEnumerator<object> __instance)
        {
            var stateField = AccessTools.DeclaredField(__instance.GetType(), "<>1__state");
            var currentField = AccessTools.DeclaredField(__instance.GetType(), "<>2__current");

            Plugin.Log.LogDebug($"stateField: {stateField.GetValue(__instance)}");
            Plugin.Log.LogDebug($"currentField: {currentField.GetValue(__instance)}");

            if (Plugin.Manager != null && Plugin.Manager.IsEjectingDueToNegligence && ((int) stateField.GetValue(__instance)) < 2)
            {
                // Skip 5s wait, alarm and fired speech sfx, and another ~9s wait before opening the doors
                stateField.SetValue(__instance, 2);
            }
        }
    }
}
