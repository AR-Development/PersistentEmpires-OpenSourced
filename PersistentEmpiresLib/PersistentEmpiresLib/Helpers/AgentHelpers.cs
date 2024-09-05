/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Helpers
{
    public class AgentHelpers
    {
        public delegate void ResetAgentMeshRequested(Agent agent);
        public static event ResetAgentMeshRequested OnResetAgentMeshRequested;

        public delegate void ResetAgentArmorMeshRequested(Agent agent);
        public static event ResetAgentArmorMeshRequested OnResetAgentArmorMeshRequested;
        public static void UpdateAgentVisuals(Agent agent)
        {
            Equipment equipment = AgentHelpers.GetCurrentAgentEquipment(agent);
            agent.UpdateSpawnEquipmentAndRefreshVisuals(equipment);
        }
        public static Equipment GetCurrentAgentEquipment(Agent agent)
        {
            Equipment equipment = new Equipment();
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                equipment[i] = new EquipmentElement(agent.Equipment[i].Item);
            }
            for (EquipmentIndex i = EquipmentIndex.ArmorItemBeginSlot; i < EquipmentIndex.ArmorItemEndSlot; i++)
            {
                equipment[i] = agent.SpawnEquipment[i];
            }
            return equipment;
        }

        // [HandleProcessCorruptedStateExceptions]
        // [SecurityCritical]
        public static void ResetAgentArmor(Agent agent, Equipment equipment)
        {
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new ResetAgentArmor(agent, equipment));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            if (agent != null && agent.IsActive() && agent.AgentVisuals.GetVisible())
            {

                try
                {

                    agent.InitializeSpawnEquipment(equipment);
                    agent.AgentVisuals.ClearVisualComponents(false);
                    agent.Mission.OnEquipItemsFromSpawnEquipment(agent, Agent.CreationType.FromCharacterObj);
                    agent.CheckEquipmentForCapeClothSimulationStateChange();
                    agent.EquipItemsFromSpawnEquipment(true);
                    agent.UpdateAgentProperties();
                    agent.PreloadForRendering();
                    if (OnResetAgentArmorMeshRequested != null)
                    {
                        OnResetAgentArmorMeshRequested(agent);
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
        }

        public static void ResetAgentMesh(Agent agent)
        {
            if (!GameNetwork.IsClientOrReplay) return;

            if (OnResetAgentMeshRequested != null) OnResetAgentMeshRequested(agent);


        }

        public static Agent RespawnAgentOnPlaceForFaction(Agent agent, Faction f, Equipment overrideEquipment = null, BasicCharacterObject overrideCharacter = null)
        {
            MissionPeer component = agent.MissionPeer;
            BasicCharacterObject character = agent.Character;
            AgentBuildData agentBuildData = new AgentBuildData(overrideCharacter == null ? character : overrideCharacter);
            Equipment equipment = new Equipment();

            int[] ammo = new int[5];

            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                equipment[i] = new EquipmentElement(agent.Equipment[i].Item);
                ammo[(int)i] = agent.Equipment[i].Amount;
            }
            for (EquipmentIndex i = EquipmentIndex.ArmorItemBeginSlot; i < EquipmentIndex.ArmorItemEndSlot; i++)
            {
                equipment[i] = agent.SpawnEquipment[i];
            }
            if (overrideEquipment != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (!overrideEquipment[i].IsInvalid() || !overrideEquipment[i].IsEmpty)
                    {
                        equipment[i] = overrideEquipment[i];
                    }
                }
            }
            NetworkCommunicator player = agent.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            uint clothingColor1 = component.Culture.Color;
            uint clothingColor2 = component.Culture.Color2;
            if (f != null)
            {
                clothingColor1 = f.banner.GetPrimaryColor();
                clothingColor2 = f.banner.GetFirstIconColor();
            }
            agentBuildData = agentBuildData
                 // .Index(agent.Index)
                 .Equipment(equipment)
                 .MissionPeer(component)
                 .InitialPosition(agent.Position)
                 .InitialDirection(agent.LookDirection.AsVec2)
                 .Team(component.Team)
                 .TroopOrigin(new BasicBattleAgentOrigin(character))
                 .IsFemale(component.Peer.IsFemale)
                 .BodyProperties(agent.BodyPropertiesValue)
                 .ClothingColor1(clothingColor1)
                 .ClothingColor2(clothingColor2);
            float oldHealdth = agent.Health;
            agent.FadeOut(true, true);
            Agent newAgent = agent.Mission.SpawnAgent(agentBuildData);
            newAgent.Health = oldHealdth;
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                if (newAgent.Equipment[i].IsEmpty == false)
                {
                    MissionWeapon weapon = new MissionWeapon(newAgent.Equipment[i].Item, null, null, (short)ammo[(int)i]);
                    newAgent.EquipWeaponWithNewEntity(i, ref weapon);
                }
            }
            return newAgent;
        }
        public static Agent RespawnAgentOnPlace(Agent agent, Equipment overrideEquipment = null, BasicCharacterObject overrideCharacter = null)
        {
            MissionPeer component = agent.MissionPeer;
            BasicCharacterObject character = agent.Character;
            AgentBuildData agentBuildData = new AgentBuildData(overrideCharacter == null ? character : overrideCharacter);
            Equipment equipment = new Equipment();
            int[] ammo = new int[5];
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                equipment[i] = new EquipmentElement(agent.Equipment[i].Item);
                ammo[(int)i] = agent.Equipment[i].Amount;
            }
            for (EquipmentIndex i = EquipmentIndex.ArmorItemBeginSlot; i < EquipmentIndex.ArmorItemEndSlot; i++)
            {
                equipment[i] = agent.SpawnEquipment[i];
            }
            if (overrideEquipment != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (!overrideEquipment[i].IsInvalid() || !overrideEquipment[i].IsEmpty)
                    {
                        equipment[i] = overrideEquipment[i];
                    }
                }
            }
            NetworkCommunicator player = agent.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            uint clothingColor1 = component.Culture.Color;
            uint clothingColor2 = component.Culture.Color2;
            if (persistentEmpireRepresentative != null && persistentEmpireRepresentative.GetFactionIndex() != -1)
            {
                clothingColor1 = persistentEmpireRepresentative.GetFaction().banner.GetPrimaryColor();
                clothingColor2 = persistentEmpireRepresentative.GetFaction().banner.GetFirstIconColor();
            }
            agentBuildData = agentBuildData
                 // .Index(agent.Index)
                 .Equipment(equipment)
                 .MissionPeer(component)
                 .InitialPosition(agent.Position)
                 .InitialDirection(agent.LookDirection.AsVec2)
                 .Team(component.Team)
                 .TroopOrigin(new BasicBattleAgentOrigin(character))
                 .IsFemale(component.Peer.IsFemale)
                 .BodyProperties(agent.BodyPropertiesValue)
                 .ClothingColor1(clothingColor1)
                 .ClothingColor2(clothingColor2);
            float oldHealdth = agent.Health;
            agent.FadeOut(true, true);
            Agent newAgent = agent.Mission.SpawnAgent(agentBuildData);
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                if (newAgent.Equipment[i].IsEmpty == false)
                {
                    MissionWeapon weapon = new MissionWeapon(newAgent.Equipment[i].Item, null, null, (short)ammo[(int)i]);
                    newAgent.EquipWeaponWithNewEntity(i, ref weapon);
                }
            }
            newAgent.Health = oldHealdth;
            return newAgent;
        }
    }
}
