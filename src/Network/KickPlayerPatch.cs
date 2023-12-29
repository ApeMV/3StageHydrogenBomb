﻿using HarmonyLib;
using Hazel;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class Network_KickPlayerPostfix
{
    //Postfix patch of PlayerPhysics.LateUpdate to open player pick menu to kick any player
    public static bool isActive;
    public static void Postfix(PlayerPhysics __instance){
        if (CheatSettings.kickPlayer){

            if (!isActive){

                //Close any player pick menus already open & their cheats
                if (Utils_PlayerPickMenu.playerpickMenu != null){
                    Utils_PlayerPickMenu.playerpickMenu.Close();
                    CheatSettings.spectate = CheatSettings.teleportPlayer = CheatSettings.murderPlayer = false;
                }

                List<PlayerControl> playerList = new List<PlayerControl>();

                //All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls){
                    if (!player.AmOwner){
                        playerList.Add(player);
                    }
                }

                //New player pick menu made for kicking players
                Utils_PlayerPickMenu.openPlayerPickMenu(playerList, (Action) (() =>
                {
                    //Votekick any player from the game by faking votes from all players
                    //Original concept by @NikoCat233 on Github
                    var HostData = AmongUsClient.Instance.GetHost();
                    if (HostData != null && !HostData.Character.Data.Disconnected){
                        foreach (var item in PlayerControl.AllPlayerControls)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(VoteBanSystem.Instance.NetId, (byte)RpcCalls.AddVote, SendOption.None, HostData.Id);
                            writer.Write(AmongUsClient.Instance.GetClientIdFromCharacter(item));
                            writer.Write(AmongUsClient.Instance.GetClientIdFromCharacter(Utils_PlayerPickMenu.targetPlayer));
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                
                }));

                isActive = true;
            }

            //Deactivate cheat if menu is closed
            if (Utils_PlayerPickMenu.playerpickMenu == null){
                CheatSettings.kickPlayer = false;
            }

        }else{
            if (isActive){
                isActive = false;
            }
        }
    }
}