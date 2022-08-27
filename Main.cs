using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using AutoConnect;
using HarmonyLib;
using MelonLoader;
using System;
using System.IO;
using UnityEngine;
using CVRPlayerEntity = ABI_RC.Core.Player.CVRPlayerEntity;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;

[assembly: MelonInfo(typeof(AutoConnect.Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace AutoConnect;
public static class Guh {
    public const string Name = "Bluscream";
    public const string Author = "Bluscream";
    public const string Version = "1.0.0";
    public const string DownloadLink = "";
}
public static class Patches {
    public static void Init(HarmonyLib.Harmony harmonyInstance) {
        MelonLogger.Msg("Harmony patches completed!");
    }
    public static void Patch(HarmonyLib.Harmony harmonyInstance, string methodName, Type[] types = null) {
        MelonLogger.Msg("Patching {0}", methodName);
        _ = types != null
            ? harmonyInstance.Patch(typeof(ABI_RC.Core.UI.CohtmlHud).GetMethod(methodName), prefix: new HarmonyMethod(typeof(Patches).GetMethod(methodName, types)))
            : harmonyInstance.Patch(typeof(ABI_RC.Core.UI.CohtmlHud).GetMethod(methodName), prefix: new HarmonyMethod(typeof(Patches).GetMethod(methodName)));
    }
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

        Patches.Init(HarmonyInstance);
    }
    public override void OnApplicationLateStart() {
    }

    private void ButtonAPI_OnInit() {

        ChilloutButtonAPI.UI.SubMenu menu = ButtonAPI.MainPage.AddSubMenu("Locomotion", "Movement");
        _ = menu.AddToggle("Freeze!", "Freeze Player", (bool enabled) => {
            MovementSystem.Instance.enabled = !enabled;
        }, false);
        _ = menu.AddToggle("canMove", "canMove", (bool enabled) => {
            MovementSystem.Instance.canMove = !enabled;
        }, false);
        _ = menu.AddToggle("Immobilize", "Immobilize Player", (bool enabled) => {
            MovementSystem.Instance.SetImmobilized(!enabled);
        }, false);
        _ = menu.AddToggle("Crouch", "Toggle Crouch", (bool enabled) => {
            MovementSystem.Instance.ChangeCrouch(!enabled);
        }, false);
        _ = menu.AddToggle("Prone", "Toggle Prone", (bool enabled) => {
            MovementSystem.Instance.ChangeProne(!enabled);
        }, false);
        _ = menu.AddToggle("Sitting", "Toggle Sitting", (bool enabled) => {
            MovementSystem.Instance.sitting = !enabled;
        }, false);
        _ = menu.AddToggle("Locomotion Lock", "Lock Locomotion", (bool enabled) => {
            PlayerSetup.Instance.LockLocomotion(!enabled);
        }, false);
        _ = menu.AddToggle("Rotation Lock", "Lock Rotation", (bool enabled) => {
            MovementSystem.Instance.canRot = !enabled;
        }, false);
        _ = menu.AddToggle("Rotation Lock 2", "Lock Rotation (alternative)", (bool enabled) => {
            PlayerSetup.Instance.LockRotation(!enabled);
        }, false);

        menu = ButtonAPI.MainPage.AddSubMenu("Debug", "Debug");
        _ = menu.AddButton("Print Layers", "", () => {
            for (int i = 0; i < 100; i++) {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrWhiteSpace(name)) {
                    MelonLogger.Msg($"{name} ({i})");
                }
            }
        });
        _ = menu.AddButton("Print Sort Layers", "", () => {
            for (int i = 0; i < 50; i++) {
                // foreach (var layer in SortingLayer.layers) {
                SortingLayer layer = SortingLayer.layers[i];
                if (SortingLayer.IsValid(i)) {
                    MelonLogger.Msg($"{layer.name} ({layer.id}) = {layer.value}");
                }
            }
        });
        _ = menu.AddButton("Dump Players", "", () => {
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string path = $"UserData/Logs/PlayerDump-{unixTimestamp}.json";
            object json = CVRPlayerManager.Instance.NetworkPlayers.ToJson();
            File.WriteAllText(path, json.ToString());
            MelonLogger.Msg($"Dumped {CVRPlayerManager.Instance.NetworkPlayers.Count} to {path}");
        });
        _ = menu.AddButton("Print Players", "", () => {
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
                MelonLogger.Msg($"Position: {player.PlayerObject.transform.position}");
                
                //MelonLogger.Msg($"PlayerDescriptor.avtrId: {player.PlayerDescriptor.avtrId}");
                MelonLogger.Msg($"PlayerMetaData.State: {player.PlayerMetaData.State}");
                MelonLogger.Msg($"= Player {i} =");
                i++;
            }
            MelonLogger.Msg($"=== {CVRPlayerManager.Instance.NetworkPlayers.Count} Players ===");
        });

        menu = ButtonAPI.MainPage.AddSubMenu("Teleport", "Teleport");
        _ = menu.AddButton("Debug", "", () => {
            MelonLogger.Msg("=== TELEPORT DEBUG ===");
            MelonLogger.Msg("PlayerSetup.Instance.gameObject.transform.position");
            MelonLogger.Msg(PlayerSetup.Instance.gameObject.transform.position);
            MelonLogger.Msg("PlayerSetup.Instance.gameObject.transform.rotation");
            MelonLogger.Msg(PlayerSetup.Instance.gameObject.transform.rotation);
            MelonLogger.Msg("MovementSystem.Instance.gameObject.transform.position");
            MelonLogger.Msg(MovementSystem.Instance.gameObject.transform.position);
            MelonLogger.Msg("MovementSystem.Instance.transform.position");
            MelonLogger.Msg(MovementSystem.Instance.transform.position);
            //MelonLogger.Msg("CVR_MovementSystem.Instance.gameObject.transform.position");
            //MelonLogger.Msg(CVR_MovementSystem.Instance.gameObject.transform.position);
            //MelonLogger.Msg("CVR_MovementSystem.Instance.transform.position");
            //MelonLogger.Msg(CVR_MovementSystem.Instance.transform.position);
            var localPlayer = GameObject.Find("_PLAYERLOCAL/[PlayerAvatar]");
            MelonLogger.Msg("localPlayer.transform.position");
            MelonLogger.Msg(localPlayer.transform.position);
            MelonLogger.Msg("=== TELEPORT DEBUG ===");
        });
        
        _ = menu.AddButton("0, 0, 0", "", () => { Teleport(0f, 0f, 0f); });
        _ = menu.AddButton("Forward", "", () => { TeleportRelative(TeleportDirection.Front, 10); });
        _ = menu.AddButton("Up", "", () => { TeleportRelative(TeleportDirection.Up, 5); });
    }

    private enum TeleportDirection { Left, Right, Up, Down, Front, Back }
    private void TeleportRelative(TeleportDirection direction, int distance) {
        MelonLogger.Msg("Teleporting {0}", direction);
        var newPos = PlayerSetup.Instance.gameObject.transform.position;
        switch (direction) {
            case TeleportDirection.Left:
                newPos -= PlayerSetup.Instance.gameObject.transform.right * distance;
                break;
            case TeleportDirection.Right:
                newPos += PlayerSetup.Instance.gameObject.transform.right * distance;
                break;
            case TeleportDirection.Down:
                newPos -= PlayerSetup.Instance.gameObject.transform.up * distance;
                break;
            case TeleportDirection.Up:
                newPos += PlayerSetup.Instance.gameObject.transform.up * distance;
                break;
            case TeleportDirection.Back:
                newPos -= PlayerSetup.Instance.gameObject.transform.forward * distance;
                break;
            case TeleportDirection.Front:
                newPos += PlayerSetup.Instance.gameObject.transform.forward * distance;
                break;
        }
        Teleport(newPos);
    }
    private void Teleport(float x, float y, float z) => Teleport(new Vector3(x, y, z));
    private void Teleport(Vector3 pos) {
        MelonLogger.Msg("Teleporting to {0}", pos);
        MovementSystem.Instance.TeleportToPosRot(pos, PlayerSetup.Instance.gameObject.transform.rotation);
        //PlayerSetup.Instance.gameObject.transform.SetPositionAndRotation(pos, PlayerSetup.Instance.gameObject.transform.rotation);
        //PlayerSetup.Instance.gameObject.transform.position = pos;
        //PlayerSetup.Instance.gameObject.transform.rotation = PlayerSetup.Instance.gameObject.transform.rotation;
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