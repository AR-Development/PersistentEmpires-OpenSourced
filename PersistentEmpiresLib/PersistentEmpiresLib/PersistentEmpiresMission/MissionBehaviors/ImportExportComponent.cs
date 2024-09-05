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

using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class ImportExportComponent : MissionNetwork
    {
        public delegate void OpenImportExportHandler(PE_ImportExport ImportExportEntity, Inventory PlayerInventory);
        public event OpenImportExportHandler OnOpenImportExport;


        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenImportExport>(this.HandleOpenImportExportFromServer);
            }
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestExportItem>(this.HandleRequestExportItem);
                networkMessageHandlerRegisterer.Register<RequestImportItem>(this.HandleRequestImportItem);
            }
        }

        private bool HandleRequestImportItem(NetworkCommunicator player, RequestImportItem message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            PE_ImportExport exportEntity = (PE_ImportExport)message.ImportExportEntity;
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false) return false;
            if (player.ControlledAgent.Position.Distance(exportEntity.GameEntity.GlobalPosition) > exportEntity.Distance) return false;
            GoodItem good = exportEntity.GetGoodItems().FirstOrDefault(g => g.ItemObj.StringId == message.Item.StringId);
            if (good.ItemObj == null) return false;

            if (!persistentEmpireRepresentative.GetInventory().HasEnoughRoomFor(message.Item, 1))
            {
                InformationComponent.Instance.SendMessage("You don't have enough space", (new Color(1f, 0, 0)).ToUnsignedInteger(), player);
                return false;
            }
            if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(good.ImportPrice))
            {
                InformationComponent.Instance.SendMessage("You don't have enough money", (new Color(1f, 0, 0)).ToUnsignedInteger(), player);
                return false;
            }
            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(message.Item, 1, ItemHelper.GetMaximumAmmo(message.Item));
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            return true;
        }

        private bool HandleRequestExportItem(NetworkCommunicator player, RequestExportItem message)
        {
            PE_ImportExport exportEntity = (PE_ImportExport)message.ImportExportEntity;
            GoodItem good = exportEntity.GetGoodItems().FirstOrDefault(g => g.ItemObj.StringId == message.Item.StringId);
            if (good.ItemObj == null) return false;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false) return false;
            if (player.ControlledAgent.Position.Distance(exportEntity.GameEntity.GlobalPosition) > exportEntity.Distance) return false;
            if (persistentEmpireRepresentative == null) return false;
            bool itemExists = persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(message.Item, 1);
            if (!itemExists)
            {
                InformationComponent.Instance.SendMessage("You don't have this item", (new Color(1f, 0, 0)).ToUnsignedInteger(), player);
                return false;
            }
            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(message.Item, 1);

            persistentEmpireRepresentative.GoldGain(good.ExportPrice);
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            return true;
        }

        private void HandleOpenImportExportFromServer(OpenImportExport message)
        {
            if (this.OnOpenImportExport != null)
            {
                this.OnOpenImportExport((PE_ImportExport)message.ImportExportEntity, message.PlayerInventory);
            }
        }

        public void OpenImportExportForPeer(NetworkCommunicator peer, PE_ImportExport ImportExportEntity)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new OpenImportExport(ImportExportEntity, persistentEmpireRepresentative.GetInventory()));
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
