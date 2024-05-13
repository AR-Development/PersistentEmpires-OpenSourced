using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class PreferredSpawnPoint : GameNetworkMessage
    {
        public PE_SpawnFrame SpawnFrame;
        public PreferredSpawnPoint() { }
        public PreferredSpawnPoint(PE_SpawnFrame spawnFrame)
        {
            this.SpawnFrame = spawnFrame;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Agents;
        }

        protected override string OnGetLogFormat()
        {
            return "Set Preferred Spawn Frame";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.SpawnFrame = (PE_SpawnFrame)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.SpawnFrame.Id);
        }
    }
}
