using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEAgentLabelUIHandler : MissionView
    {
        private TaleWorlds.TwoDimension.TwoDimensionDrawContext _drawContext;
        private ProximityChatComponent _proximityChatComponent;
        private PEProximityChatView _proximityChatView;
        private FactionsBehavior _factionsBehavior;
        private PersistentEmpireClientBehavior _persistentEmpireClientBehavior;
        private Dictionary<Agent, bool> _renderedHasVoiceData = new Dictionary<Agent, bool>();
        private Dictionary<Agent, MetaMesh> _voiceMeshes = new Dictionary<Agent, MetaMesh>();
        private Dictionary<Agent, long> _voiceMeshSettedAt = new Dictionary<Agent, long>();
        private List<string> _validStealthItems = new List<string>() { "PE_pilgrim_hood" };
        private bool disabled = false;

        private OrderController PlayerOrderController
        {
            get
            {
                Team playerTeam = base.Mission.PlayerTeam;
                if (playerTeam == null)
                {
                    return null;
                }
                return playerTeam.PlayerOrderController;
            }
        }

        // Token: 0x17000044 RID: 68
        // (get) Token: 0x060002EB RID: 747 RVA: 0x00019E7A File Offset: 0x0001807A
        private SiegeWeaponController PlayerSiegeWeaponController
        {
            get
            {
                Team playerTeam = base.Mission.PlayerTeam;
                if (playerTeam == null)
                {
                    return null;
                }
                return playerTeam.PlayerOrderController.SiegeWeaponController;
            }
        }

        // Token: 0x060002EC RID: 748 RVA: 0x00019E97 File Offset: 0x00018097
        public PEAgentLabelUIHandler()
        {
            this._agentMeshes = new Dictionary<Agent, MetaMesh>();
            this._labelMaterials = new Dictionary<Texture, Material>();
        }

        // Token: 0x060002ED RID: 749 RVA: 0x00019EB8 File Offset: 0x000180B8
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            base.Mission.OnMainAgentChanged += this.OnMainAgentChanged;
            base.MissionScreen.OnSpectateAgentFocusIn += this.HandleSpectateAgentFocusIn;
            base.MissionScreen.OnSpectateAgentFocusOut += this.HandleSpectateAgentFocusOut;

            this._drawContext = new TaleWorlds.TwoDimension.TwoDimensionDrawContext();
            this._proximityChatComponent = base.Mission.GetMissionBehavior<ProximityChatComponent>();
            this._proximityChatView = base.Mission.GetMissionBehavior<PEProximityChatView>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._persistentEmpireClientBehavior = base.Mission.GetMissionBehavior<PersistentEmpireClientBehavior>();
            this._factionsBehavior.OnFactionUpdated += OnFactionUpdated;
            this._factionsBehavior.OnPlayerJoinedFaction += OnPlayerJoinedFaction;
            this._factionsBehavior.OnFactionDeclaredWar += OnFactionDeclaredWar;
            this._factionsBehavior.OnFactionMakePeace += OnFactionMakePeace;
            if (this._proximityChatComponent != null && this._proximityChatView != null)
            {
                // this._proximityChatComponent.OnPeerVoiceStatusUpdated += OnPeerVoiceStatusUpdated;
                this._proximityChatView.OnPeerVoiceStatusUpdated += OnPeerVoiceStatusUpdated;
            }
            if (this._persistentEmpireClientBehavior != null)
            {
                this._persistentEmpireClientBehavior.OnAgentLabelConfig += this.OnAgentLabelConfig;

            }

        }

        private bool isMyEnemy(Agent agent)
        {
            if (agent.MissionPeer == null) return false;
            NetworkCommunicator networkCommunicator = agent.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative representative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (representative == null) return false;
            if (GameNetwork.MyPeer == null) return false;
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (myRepresentative == null) return false;

            if (representative.GetFaction() == null || myRepresentative.GetFaction() == null) return false;
            if (representative.GetFaction().warDeclaredTo.Contains(myRepresentative.GetFactionIndex()) || myRepresentative.GetFaction().warDeclaredTo.Contains(representative.GetFactionIndex()))
            {
                return true;
            }
            return false;

        }

        private void OnFactionMakePeace(int maker, int taker)
        {
            NetworkCommunicator thisPlayer = GameNetwork.MyPeer;
            PersistentEmpireRepresentative representative = thisPlayer.GetComponent<PersistentEmpireRepresentative>();
            if (representative.GetFactionIndex() != maker && representative.GetFactionIndex() != taker) return;
            foreach (Agent agent in Mission.Current.Agents.Where(a => a.IsPlayerControlled))
            {
                if (agent.MissionPeer == null) continue;
                NetworkCommunicator peer = agent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative representative1 = peer.GetComponent<PersistentEmpireRepresentative>();
                if (representative.GetFactionIndex() == maker && representative1.GetFactionIndex() == taker)
                {
                    Banner b = representative1.GetFaction().banner;
                    this.UpdateVisibilityOfAgentMesh(agent);
                }
                else if (representative.GetFactionIndex() == taker && representative1.GetFactionIndex() == maker)
                {
                    Banner b = representative1.GetFaction().banner;
                    this.UpdateVisibilityOfAgentMesh(agent);
                }
            }

        }

        private void OnFactionDeclaredWar(int declarer, int declaredTo)
        {
            NetworkCommunicator thisPlayer = GameNetwork.MyPeer;
            PersistentEmpireRepresentative representative = thisPlayer.GetComponent<PersistentEmpireRepresentative>();
            if (representative.GetFactionIndex() != declarer && representative.GetFactionIndex() != declaredTo) return;
            foreach (Agent agent in this._agentMeshes.Keys.ToList())
            {
                if (agent.MissionPeer == null) continue;
                NetworkCommunicator peer = agent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative representative1 = peer.GetComponent<PersistentEmpireRepresentative>();
                if (representative.GetFactionIndex() == declarer && representative1.GetFactionIndex() == declaredTo)
                {
                    this.UpdateVisibilityOfAgentMesh(agent);
                }
                else if (representative.GetFactionIndex() == declaredTo && representative1.GetFactionIndex() == declarer)
                {
                    this.UpdateVisibilityOfAgentMesh(agent);
                }
            }
        }

        private void OnAgentLabelConfig(bool enabled)
        {
            if (enabled == false) this.Disable();
        }

        public void Enable()
        {
            this.disabled = false;
        }

        public void Disable()
        {
            foreach (Agent agent in this._agentMeshes.Keys.ToList())
            {
                this.RemoveAgentBanner(agent);
            }
            this.disabled = true;
        }

        private void OnPlayerJoinedFaction(int factionIndex, Faction faction, int joinedFromIndex, NetworkCommunicator player)
        {
            if (player.ControlledAgent == null || !player.ControlledAgent.IsActive() || factionIndex == -1) return;
            Banner b = faction.banner;
            this.InitAgentLabel(player.ControlledAgent, b);

        }

        private void OnFactionUpdated(int factionIndex, Faction faction)
        {
            foreach (NetworkCommunicator member in faction.members)
            {
                if (member.ControlledAgent == null || !member.ControlledAgent.IsActive()) continue;
                this.InitAgentLabel(member.ControlledAgent, faction.banner);
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            foreach (Agent agent in this._voiceMeshSettedAt.Keys.ToList())
            {
                if (_voiceMeshSettedAt[agent] + 3 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    _voiceMeshSettedAt.Remove(agent);
                    if (this._renderedHasVoiceData.ContainsKey(agent))
                    {
                        this._renderedHasVoiceData.Remove(agent);
                    }
                    if (this._voiceMeshes.ContainsKey(agent))
                    {
                        agent.AgentVisuals.ReplaceMeshWithMesh(this._voiceMeshes[agent], this._agentMeshes.ContainsKey(agent) ? this._agentMeshes[agent] : null, BodyMeshTypes.Label);
                        this._voiceMeshes[agent].SetVisibilityMask(0);
                        if (this._agentMeshes.ContainsKey(agent) && (this.isAgentStealth(agent) || this.isMyEnemy(agent)))
                        {
                            this._agentMeshes[agent].SetVisibilityMask(0);
                        }
                        else if (this._agentMeshes.ContainsKey(agent))
                        {
                            this._agentMeshes[agent].SetVisibilityMask(VisibilityMaskFlags.Final);
                        }
                    }
                }
            }
        }

        private void OnPeerVoiceStatusUpdated(PlayerVoiceData playerVoiceData, NetworkCommunicator player)
        {
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false) return;
            bool hasVoiceData = playerVoiceData.InputWaveProvider.BufferedDuration.TotalMilliseconds > 0;
            Agent agent = player.ControlledAgent;
            if (hasVoiceData && this._renderedHasVoiceData.ContainsKey(agent) == false)
            {
                this._renderedHasVoiceData[agent] = true;
                this._voiceMeshSettedAt[agent] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                // Texture texture = null;
                MetaMesh copy = MetaMesh.GetCopy("troop_banner_selection", false, true);
                Material tableauMaterial = player.GetComponent<MissionPeer>().IsMutedFromGameOrPlatform ? Material.GetFromResource("alverrt_is_yapti") : Material.GetFromResource("santafor_is_yapti");
                if (copy != null && tableauMaterial != null)
                {
                    copy.SetMaterial(tableauMaterial);
                    copy.SetVectorArgument(0.6f, 0.6f, 0.26f, 0.26f);
                    copy.SetVectorArgument2(31f, 0.5f, 0.45f, -1f);

                    // agent.AgentVisuals.Mesh

                    if (this._voiceMeshes.ContainsKey(agent))
                    {
                        if (this._agentMeshes.ContainsKey(agent))
                        {
                            agent.AgentVisuals.ReplaceMeshWithMesh(this._agentMeshes[agent], copy, BodyMeshTypes.Label);
                        }
                        else
                        {
                            agent.AgentVisuals.AddMultiMesh(copy, BodyMeshTypes.Label);
                        }
                    }
                    else
                    {
                        agent.AgentVisuals.AddMultiMesh(copy, BodyMeshTypes.Label);
                        this._voiceMeshes[agent] = copy;
                    }
                    this._voiceMeshes[agent].SetVisibilityMask(VisibilityMaskFlags.Final);
                    if (this._agentMeshes.ContainsKey(agent))
                    {
                        this._agentMeshes[agent].SetVisibilityMask(0);
                    }
                }
            }
            else if (!hasVoiceData && this._renderedHasVoiceData.ContainsKey(agent) && _voiceMeshSettedAt[agent] + 3 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this._renderedHasVoiceData.Remove(agent);
                agent.AgentVisuals.ReplaceMeshWithMesh(this._voiceMeshes[agent], this._agentMeshes.ContainsKey(agent) ? this._agentMeshes[agent] : null, BodyMeshTypes.Label);
                this._voiceMeshes[agent].SetVisibilityMask(0);
                if (this._agentMeshes.ContainsKey(agent))
                {
                    this._agentMeshes[agent].SetVisibilityMask(VisibilityMaskFlags.Final);
                }
            }

        }

        // Token: 0x060002EE RID: 750 RVA: 0x00019F4C File Offset: 0x0001814C
        public override void AfterStart()
        {

        }

        // Token: 0x060002F0 RID: 752 RVA: 0x0001A008 File Offset: 0x00018208
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            base.Mission.OnMainAgentChanged -= this.OnMainAgentChanged;
            base.MissionScreen.OnSpectateAgentFocusIn -= this.HandleSpectateAgentFocusIn;
            base.MissionScreen.OnSpectateAgentFocusOut -= this.HandleSpectateAgentFocusOut;
        }

        // Token: 0x060002F1 RID: 753 RVA: 0x0001A0D8 File Offset: 0x000182D8
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            this.RemoveAgentBanner(affectedAgent);
        }

        // Token: 0x060002F2 RID: 754 RVA: 0x0001A130 File Offset: 0x00018330
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            Banner fBanner = null;

            if (agent.MissionPeer != null)
            {
                NetworkCommunicator peer = agent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();

                fBanner = persistentEmpireRepresentative == null || persistentEmpireRepresentative.GetFaction() == null ? null : persistentEmpireRepresentative.GetFaction().banner;

            }

            this.InitAgentLabel(agent, fBanner ?? banner);
        }

        // Token: 0x060002F3 RID: 755 RVA: 0x0001A13C File Offset: 0x0001833C
        public override void OnAssignPlayerAsSergeantOfFormation(Agent agent)
        {
            float friendlyTroopsBannerOpacity = BannerlordConfig.FriendlyTroopsBannerOpacity;
            this._agentMeshes[agent].SetVectorArgument2(30f, 0.4f, 0.44f, 1f * friendlyTroopsBannerOpacity);
        }

        // Token: 0x060002F4 RID: 756 RVA: 0x0001A176 File Offset: 0x00018376
        public override void OnClearScene()
        {
            this._agentMeshes.Clear();
            this._labelMaterials.Clear();
        }

        // Token: 0x060002F9 RID: 761 RVA: 0x0001A2B4 File Offset: 0x000184B4
        public void OnAgentListSelectionChanged(bool selectionMode, List<Agent> affectedAgents)
        {
            foreach (Agent key in affectedAgents)
            {
                float num = selectionMode ? 1f : -1f;
                if (this._agentMeshes.ContainsKey(key))
                {
                    MetaMesh metaMesh = this._agentMeshes[key];
                    float friendlyTroopsBannerOpacity = BannerlordConfig.FriendlyTroopsBannerOpacity;
                    metaMesh.SetVectorArgument2(30f, 0.4f, 0.44f, num * friendlyTroopsBannerOpacity);
                }
            }
        }

        public void RemoveAgentBanner(Agent affectedAgent)
        {
            if (affectedAgent.IsHuman && this._agentMeshes.ContainsKey(affectedAgent))
            {
                if (affectedAgent.AgentVisuals != null)
                {
                    affectedAgent.AgentVisuals.ReplaceMeshWithMesh(this._agentMeshes[affectedAgent], null, BodyMeshTypes.Label);
                }
                this._agentMeshes.Remove(affectedAgent);
            }
        }

        // Token: 0x060002FA RID: 762 RVA: 0x0001A344 File Offset: 0x00018544
        public void InitAgentLabel(Agent agent, Banner peerBanner = null)
        {
            if (disabled) return;


            if (agent.IsHuman)
            {
                this.RemoveAgentBanner(agent);
                Texture texture = null;
                MetaMesh copy = MetaMesh.GetCopy("troop_banner_selection", false, true);
                Material tableauMaterial = Material.GetFromResource("agent_label_with_tableau");
                if (agent.Origin.Banner != null || peerBanner != null)
                {
                    texture = (peerBanner ?? agent.Origin.Banner).GetTableauTextureSmall(null);
                }
                if (copy != null && tableauMaterial != null)
                {
                    Texture fromResource = Texture.GetFromResource("banner_top_of_head");
                    Material tableauMaterial2;
                    if (this._labelMaterials.TryGetValue((texture != null) ? texture : fromResource, out tableauMaterial2))
                    {
                        tableauMaterial = tableauMaterial2;
                    }
                    else
                    {
                        tableauMaterial = tableauMaterial.CreateCopy();
                        Action<Texture> setAction = delegate (Texture tex)
                        {
                            tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap, tex);
                        };
                        if (agent.Origin.Banner != null || peerBanner != null)
                        {
                            texture = (peerBanner ?? agent.Origin.Banner).GetTableauTextureSmall(setAction);
                        }
                        tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap2, fromResource);
                        this._labelMaterials.Add(texture, tableauMaterial);
                    }
                    copy.SetMaterial(tableauMaterial);
                    copy.SetVectorArgument(0.5f, 0.5f, 0.25f, 0.25f);
                    copy.SetVectorArgument2(30f, 0.4f, 0.44f, -1f);
                    if (this._agentMeshes.ContainsKey(agent))
                    {
                        agent.AgentVisuals.ReplaceMeshWithMesh(this._agentMeshes[agent], copy, BodyMeshTypes.Label);
                    }
                    else
                    {
                        agent.AgentVisuals.AddMultiMesh(copy, BodyMeshTypes.Label);
                    }
                    this._agentMeshes[agent] = copy;
                    this.UpdateVisibilityOfAgentMesh(agent);
                    this.UpdateSelectionVisibility(agent, this._agentMeshes[agent], new bool?(false));
                }
            }
        }

        public void AddStealthItem(string item)
        {
            if(!_validStealthItems.Contains(item))
            {
                _validStealthItems.Add(item);
            }
        }

        private bool isAgentStealth(Agent agent)
        {
            if (agent.IsActive() && agent.SpawnEquipment[EquipmentIndex.Head].IsEmpty != true && _validStealthItems.Contains(agent.SpawnEquipment[EquipmentIndex.Head].Item.StringId))
            {
                return true;
            }
            return false;
        }

        // Token: 0x060002FB RID: 763 RVA: 0x0001A4D8 File Offset: 0x000186D8
        private void UpdateVisibilityOfAgentMesh(Agent agent)
        {
            if (agent.IsActive() && this._agentMeshes.ContainsKey(agent) && base.Mission.MainAgent != agent)
            {
                this._agentMeshes[agent].SetVisibilityMask(VisibilityMaskFlags.Final);
            }
            else if (this._agentMeshes.ContainsKey(agent))
            {
                this._agentMeshes[agent].SetVisibilityMask(0);
            }

            if (this._agentMeshes.ContainsKey(agent) && (this.isAgentStealth(agent) || this.isMyEnemy(agent)))
            {
                this._agentMeshes[agent].SetVisibilityMask(0);
            }
        }

        // Token: 0x060002FE RID: 766 RVA: 0x0001A590 File Offset: 0x00018790
        private bool IsAllyInAllyTeam(Agent agent)
        {
            if (((agent != null) ? agent.Team : null) != null && agent != base.Mission.MainAgent)
            {
                Team team = null;
                Team team2;
                if (GameNetwork.IsSessionActive)
                {
                    team2 = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : null);
                }
                else
                {
                    team2 = base.Mission.PlayerTeam;
                    team = base.Mission.PlayerAllyTeam;
                }
                return agent.Team == team2 || agent.Team == team;
            }
            return false;
        }

        // Token: 0x060002FF RID: 767 RVA: 0x0001A60C File Offset: 0x0001880C
        private void OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (Agent agent in this._agentMeshes.Keys)
            {
                this.UpdateVisibilityOfAgentMesh(agent);
            }
        }

        // Token: 0x06000300 RID: 768 RVA: 0x0001A664 File Offset: 0x00018864
        private void HandleSpectateAgentFocusIn(Agent agent)
        {
            this.UpdateVisibilityOfAgentMesh(agent);
        }

        // Token: 0x06000301 RID: 769 RVA: 0x0001A66D File Offset: 0x0001886D
        private void HandleSpectateAgentFocusOut(Agent agent)
        {
            this.UpdateVisibilityOfAgentMesh(agent);
        }


        // Token: 0x06000303 RID: 771 RVA: 0x0001A6E8 File Offset: 0x000188E8
        private bool IsAgentListeningToOrders(Agent agent)
        {
            if (this.IsOrderScreenVisible() && agent.Formation != null && this.IsAllyInAllyTeam(agent))
            {
                return false;
            }
            return false;
        }

        // Token: 0x06000304 RID: 772 RVA: 0x0001A798 File Offset: 0x00018998
        private void UpdateSelectionVisibility(Agent agent, MetaMesh mesh, bool? visibility = null)
        {
            // mesh.SetVectorArgument2(30f, 0.4f, 0.44f, 1f);
            /*if (visibility == null)
			{
				visibility = new bool?(this.IsAgentListeningToOrders(agent));
			}
			float num = visibility.Value ? 1f : -1f;
			if (agent.MissionPeer == null)
			{
				float config = ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.FriendlyTroopsBannerOpacity);
				mesh.SetVectorArgument2(30f, 0.4f, 0.44f, num * config);
			}*/
        }

        // Token: 0x06000305 RID: 773 RVA: 0x0001A7FA File Offset: 0x000189FA
        private bool IsOrderScreenVisible()
        {
            return this.PlayerOrderController != null && base.MissionScreen.OrderFlag != null && base.MissionScreen.OrderFlag.IsVisible;
        }
        public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
        {
            this.UpdateVisibilityOfAgentMesh(agent);
        }

        // Token: 0x04000212 RID: 530
        private const float _highlightedLabelScaleFactor = 30f;

        // Token: 0x04000213 RID: 531
        private const float _labelBannerWidth = 0.4f;

        // Token: 0x04000214 RID: 532
        private const float _labelBlackBorderWidth = 0.44f;

        // Token: 0x04000215 RID: 533
        private readonly Dictionary<Agent, MetaMesh> _agentMeshes;

        // Token: 0x04000216 RID: 534
        private readonly Dictionary<Texture, Material> _labelMaterials;
    }
}