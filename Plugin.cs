using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenDoorsInSpace
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static OpenDoorsInSpacePluginManager Manager;

        public Plugin()
        {
            Log = base.Logger;
        }

        private void Awake()
        {
            var gameObject = new GameObject("OpenDoorsInSpace");
            gameObject.AddComponent<OpenDoorsInSpacePluginManager>();
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            Manager = gameObject.GetComponent<OpenDoorsInSpacePluginManager>();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }

    public class OpenDoorsInSpacePluginManager : MonoBehaviour
    {
        public bool IsEjectingDueToNegligence { get; internal set; }

        private TextMeshProUGUI youAreFiredDueToNegliganceSubtextTMP;

        void Start()
        {
            Plugin.Log.LogDebug("OpenDoorsInSpacePluginManager Start");

            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        void HandleActiveSceneChanged(Scene prevScene, Scene activeScene)
        {
            // Handle cases when players disconnect while the sequence is ongoing, etc.
            ResetEjectingDueToNegligance();
        }

        private GameObject FindYouAreFiredOriginalSubtextGameObject()
        {
            return GameObject.Find("/Systems/UI/Canvas/GameOverScreen/MaskImage/HeaderText (1)");
        }

        private TextMeshProUGUI CreateYouAreFiredDueToNegliganceSubtext()
        {
            var youAreFiredOriginalReasonSubtextGameObject = FindYouAreFiredOriginalSubtextGameObject();
            var youAreFiredNegliganceReasonSubtextGameObject = Instantiate(youAreFiredOriginalReasonSubtextGameObject, youAreFiredOriginalReasonSubtextGameObject.transform.parent);
            youAreFiredNegliganceReasonSubtextGameObject.name = youAreFiredNegliganceReasonSubtextGameObject.name + " (OpenDoorsInSpace fired subtext)";
            youAreFiredNegliganceReasonSubtextGameObject.SetActive(false);

            youAreFiredDueToNegliganceSubtextTMP = youAreFiredNegliganceReasonSubtextGameObject.GetComponent<TextMeshProUGUI>();
            youAreFiredDueToNegliganceSubtextTMP.text = "Someone opened the hangar door while in space.";

            return youAreFiredDueToNegliganceSubtextTMP;
        }

        public void EjectDueToNegligance()
        {
            if (StartOfRound.Instance.firingPlayersCutsceneRunning || IsEjectingDueToNegligence)
            {
                return;
            }

            IsEjectingDueToNegligence = true;

            StartOfRound.Instance.ManuallyEjectPlayersServerRpc();

            if (youAreFiredDueToNegliganceSubtextTMP == null)
            {
                youAreFiredDueToNegliganceSubtextTMP = CreateYouAreFiredDueToNegliganceSubtext();
            }
            FindYouAreFiredOriginalSubtextGameObject().SetActive(false);
            youAreFiredDueToNegliganceSubtextTMP.gameObject.SetActive(true);

        }

        public void ResetEjectingDueToNegligance()
        {
            FindYouAreFiredOriginalSubtextGameObject()?.SetActive(true);
            if (youAreFiredDueToNegliganceSubtextTMP != null && youAreFiredDueToNegliganceSubtextTMP.gameObject != null)
            {
                youAreFiredDueToNegliganceSubtextTMP.gameObject.SetActive(false);
            }

            IsEjectingDueToNegligence = false;
        }
    }
}
