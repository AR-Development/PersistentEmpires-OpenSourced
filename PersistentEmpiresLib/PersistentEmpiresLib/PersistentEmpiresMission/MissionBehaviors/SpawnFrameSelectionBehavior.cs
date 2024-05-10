using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using Debug = TaleWorlds.Library.Debug;
namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class SpawnFrameSelectionBehavior : MissionNetwork
    {
        public List<PE_SpawnFrame> DefaultSpawnFrames = new List<PE_SpawnFrame>();
        public override void OnBehaviorInitialize()
        {
            Debug.Print("** Persistent Empires ** SpawnFrameSelectionBehavior", 0, Debug.DebugColor.Cyan);
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.DefaultSpawnFrames = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>().Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>()).Where(p => p.FactionIndex == 0 && !p.SpawnFromCastle).ToList();

            Debug.Print("** Persistent Empires ** Length of default spawn is : " + this.DefaultSpawnFrames.Count, 0, Debug.DebugColor.Cyan);
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<PreferredSpawnPoint>(this.HandlePreferredSpawnPoint);
            }
        }

        private bool HandlePreferredSpawnPoint(NetworkCommunicator peer, PreferredSpawnPoint message)
        {
            PE_SpawnFrame frame = message.SpawnFrame;
            bool canPeerSpawn = frame.CanPeerSpawnHere(peer);
            if (!canPeerSpawn)
            {
                Random rng = new Random();
                int randomIndex = rng.Next(this.DefaultSpawnFrames.Count);
                frame = this.DefaultSpawnFrames[randomIndex];
            }
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            persistentEmpireRepresentative.SetSpawnFrame(frame);
            return true;
        }
    }
}
