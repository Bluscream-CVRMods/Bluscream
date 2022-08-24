using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using Bluscream;
using MelonLoader;
using UnityEngine;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;

[assembly: MelonInfo(typeof(Bluscream.Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace Bluscream;
public static class Guh {
    public const string Name = "Bluscream";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}
public class Main : MelonMod {
    public MelonPreferences_Entry ExampleSetting;

    public override void OnPreferencesLoaded(string filepath) {
    }
    public override void OnPreferencesSaved(string filepath) {
    }

    public override void OnPreSupportModule() {
    }
    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        ExampleSetting = cat.CreateEntry<bool>("Test 1", false);

        ButtonAPI.OnInit += ButtonAPI_OnInit;
        ButtonAPI.OnPlayerJoin += OnPlayerJoin;
        ButtonAPI.OnPlayerLeave += OnPlayerLeave;
        ButtonAPI.OnAvatarInstantiated_Pre_E += OnAvatarInstantiated_Pre_E;
        ButtonAPI.OnAvatarInstantiated_Post_E += OnAvatarInstantiated_Post_E;
    }
    public override void OnApplicationLateStart() {
    }

    private void ButtonAPI_OnInit() {
        ChilloutButtonAPI.UI.SubMenu menu = ButtonAPI.MainPage;
        menu.AddToggle("Immobilize", "Immobilize Player", (bool enabled) => {
            MovementSystem.Instance.canMove = !enabled;
        }, false);
        menu.AddToggle("Freeze", "Freeze Player", (bool enabled) => {
            MovementSystem.Instance.enabled = !enabled;
        }, false);
#if DEBUG
        menu.AddButton("Print Layers", "", () => {
            for (int i = 0; i < 100; i++) {
                var name = LayerMask.LayerToName(i);
                if (!string.IsNullOrWhiteSpace(name)) MelonLogger.Msg($"{name} ({i})");
            }
        });
        menu.AddButton("Dump Players", "", () => {
            Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var path = $"UserData/Logs/PlayerDump-{unixTimestamp}.json";
            var json = CVRPlayerManager.Instance.NetworkPlayers.ToJson();
            File.WriteAllText(path, json.ToString());
            MelonLogger.Msg($"Dumped {CVRPlayerManager.Instance.NetworkPlayers.Count} to {path}");
        });
#endif
        menu.AddButton("Print Players", "", () => {
            MelonLogger.Msg($"=== {CVRPlayerManager.Instance.NetworkPlayers.Count} Players ===");
            int i = 0;
            foreach (CVRPlayerEntity player in CVRPlayerManager.Instance.NetworkPlayers) {
                MelonLogger.Msg($"= Player {i} =");
                MelonLogger.Msg($"Username: {player.Username}");
                MelonLogger.Msg($"DarkRift2Player.name: {player.DarkRift2Player.name}");
                MelonLogger.Msg($"PlayerDescriptor.userName: {player.PlayerDescriptor.userName}");
                MelonLogger.Msg($"Uuid: {player.Uuid}");
                MelonLogger.Msg($"DarkRift2Player.PlayerId: {player.DarkRift2Player.PlayerId}");
                MelonLogger.Msg($"PlayerDescriptor.ownerId: {player.PlayerDescriptor.ownerId}");
                MelonLogger.Msg($"AvatarId: {player.AvatarId}");
                //MelonLogger.Msg($"PlayerDescriptor.avtrId: {player.PlayerDescriptor.avtrId}");
                //MelonLogger.Msg($"PlayerMetaData.State: {player.PlayerMetaData.State}");
                MelonLogger.Msg($"= Player {i} =");
                i++;
            }
            MelonLogger.Msg($"=== {CVRPlayerManager.Instance.NetworkPlayers.Count} Players ===");
        });
    }

    private void OnPlayerJoin(PlayerDescriptor player) {
    }
    private void OnAvatarInstantiated_Post_E(PuppetMaster arg1, GameObject arg2) {
    }
    private bool OnAvatarInstantiated_Pre_E(PuppetMaster arg1, GameObject arg2) {
        return true;
    }
    private void OnPlayerLeave(PlayerDescriptor player) {
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
    }
    public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
    }
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) { }

    public override void OnApplicationQuit() {
    }
}