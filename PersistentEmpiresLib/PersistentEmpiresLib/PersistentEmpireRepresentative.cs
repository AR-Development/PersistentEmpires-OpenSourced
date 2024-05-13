using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib
{
    public class PersistentEmpireRepresentative : MissionRepresentativeBase
    {
        public const string defaultClass = "pe_peasant";

        private Faction _playerFaction;
        private int _factionIndex = -1;
        private string _classId = defaultClass;
        private Inventory playerInventory;
        private int hunger = 0;
        private PE_SpawnFrame nextSpawnFrame = null;
        public Timer SpawnTimer;
        public int LoadedHealth = 100;
        public bool IsAdmin = false;
        public bool IsFirstAgentSpawned = false; // To check if player's initial agent is spawned on connection
        public bool KickedFromFaction = false;
        public long DisconnectedAt = 0;
        public bool LoadFromDb = false;
        public Vec3 LoadedDbPosition;
        public Equipment LoadedSpawnEquipment;
        public string AttachToAgentId { get; set; } 
        public int[] LoadedAmmo { get; set; }

        public PersistentEmpireRepresentative()
        {
            playerInventory = new Inventory(5, 10, "PlayerInventory");
            hunger = 100;
            this.SpawnTimer = new Timer(Mission.Current.CurrentTime, 3f, false);
        }

        public PE_SpawnFrame GetNextSpawnFrame()
        {
            return this.nextSpawnFrame;
        }

        public List<PE_SpawnFrame> GetSpawnableCastleFrames()
        {
            List<PE_SpawnFrame> liste = new List<PE_SpawnFrame>();
            if (this.GetFaction() != null)
            {
                //Faction f = this.GetFaction();
                List<PE_CastleBanner> castleBanners;
                castleBanners = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_CastleBanner>().Select(g => g.GetFirstScriptOfType<PE_CastleBanner>()).Where(c => c.FactionIndex == this.GetFactionIndex()).ToList();
                foreach (PE_CastleBanner castleBanner in castleBanners)
                {
                    List<PE_SpawnFrame> spawnFrame = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>().Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>()).Where(frame => frame.CastleIndex == castleBanner.CastleIndex).ToList();

                    liste.AddRange(spawnFrame);
                }
            }
            return liste;
        }

        public void SetSpawnFrame(PE_SpawnFrame frame)
        {
            this.nextSpawnFrame = frame;

        }
        public int GetHunger()
        {
            return this.hunger;
        }
        public void SetHunger(int hunger)
        {
            this.hunger = hunger;
            if (this.hunger < 0) this.hunger = 0;
            if (this.hunger > 100) this.hunger = 100;
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new SetHunger(this.hunger));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public Inventory GetInventory()
        {
            return this.playerInventory;
        }
        public void SetInventory(Inventory inventory)
        {
            this.playerInventory = inventory;
        }

        public Faction GetFaction()
        {
            return this._playerFaction;
        }
        public int GetFactionIndex()
        {
            return this._factionIndex;
        }
        public string GetClassId()
        {
            return this._classId;
        }
        public void SetClass(string classId)
        {
            this._classId = classId;
        }
        public void SetFaction(Faction f, int factionIndex)
        {
            this._playerFaction = f;
            this._factionIndex = factionIndex;
            if (this._playerFaction != null)
            {
                this.MissionPeer.Team = this._playerFaction.team;
            }
        }

        public void SetGold(int newGold)
        {
            this.UpdateGold(newGold);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new SyncGold(newGold));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public bool ReduceIfHaveEnoughGold(int requiredGold)
        {
            if (requiredGold > this.Gold) return false;
            this.GoldLost(requiredGold);
            return true;
        }
        public bool HaveEnoughGold(int requiredGold)
        {
            if (this.Gold < requiredGold) return false;
            return true;
        }
        public void GoldGain(int goldGain)
        {
            this.UpdateGold(this.Gold + goldGain);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new PEGoldGain(goldGain));
                GameNetwork.EndModuleEventAsServer();
            }
            if (GameNetwork.IsClient)
            {
                SoundEvent.CreateEventFromString("event:/ui/notification/coins_positive", Mission.Current.Scene).Play();
            }
        }
        public void GoldLost(int goldLost)
        {
            int newGold = this.Gold - goldLost;

            this.UpdateGold(newGold > 0 ? newGold : 0);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new PEGoldLost(goldLost));
                GameNetwork.EndModuleEventAsServer();
            }
            if (GameNetwork.IsClient)
            {
                SoundEvent.CreateEventFromString("event:/ui/notification/coins_negative", Mission.Current.Scene).Play();
            }
        }
    }

}
