using PersistentEmpires.Views.Views;
using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace PersistentEmpires.Views
{
    public class BannerRender
    {
        public Banner RenderBanner { get; set; }
        public GameEntity RenderOnTo { get; set; }
    }

    public class ViewHandler
    {
        public static Queue<BannerRender> RenderQueue;
        public static void OnRequestRenderBanner(Banner banner, GameEntity renderOnTo)
        {
            RenderQueue.Enqueue(new BannerRender
            {
                RenderBanner = banner,
                RenderOnTo = renderOnTo
            });
        }

        public static void Initialize()
        {
            BannerRenderer.OnRequestRenderBanner += OnRequestRenderBanner;
            AgentHelpers.OnResetAgentMeshRequested += OnResetAgentMeshRequested;
            AgentHelpers.OnResetAgentArmorMeshRequested += OnResetAgentArmorMeshRequested;
            PatchGameNetwork.OnHandleServerEventCreatePlayer += PatchGameNetwork_OnHandleServerEventCreatePlayer;
            RenderQueue = new Queue<BannerRender>();
        }

        private static void PatchGameNetwork_OnHandleServerEventCreatePlayer(TaleWorlds.MountAndBlade.Network.Messages.CreatePlayer message)
        {
            if (message.IsNonExistingDisconnectedPeer == false && message.DisconnectedPeerIndex > 0)
            {
                NetworkCommunicator networkCommunicator = GameNetwork.DisconnectedNetworkPeers[message.DisconnectedPeerIndex] as NetworkCommunicator;
                VirtualPlayer vp = networkCommunicator.VirtualPlayer;
                vp.GetType().GetProperty("UserName").SetValue(vp, message.PlayerName);
            }
        }

        private static void UpdateAgentLabel(Agent agent)
        {
            PEAgentLabelUIHandler peAgentLabel = Mission.Current.GetMissionBehavior<PEAgentLabelUIHandler>();
            if (peAgentLabel != null)
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = agent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                Banner fBanner = persistentEmpireRepresentative == null || persistentEmpireRepresentative.GetFaction() == null ? null : persistentEmpireRepresentative.GetFaction().banner;
                peAgentLabel.InitAgentLabel(agent, fBanner ?? agent.Origin.Banner);
            }
        }

        private static void OnResetAgentArmorMeshRequested(Agent agent)
        {
            UpdateAgentLabel(agent);
        }

        private static void OnResetAgentMeshRequested(Agent agent)
        {
            uint color3 = agent.ClothingColor1;
            uint color4 = agent.ClothingColor2;
            for (EquipmentIndex equipmentIndex = EquipmentIndex.NumAllWeaponSlots; equipmentIndex < EquipmentIndex.ArmorItemEndSlot; equipmentIndex++)
            {
                if (!agent.SpawnEquipment[equipmentIndex].IsVisualEmpty)
                {
                    ItemObject itemObject = agent.SpawnEquipment[equipmentIndex].CosmeticItem ?? agent.SpawnEquipment[equipmentIndex].Item;
                    bool hasGloves = equipmentIndex == EquipmentIndex.Body && agent.SpawnEquipment[EquipmentIndex.Gloves].Item != null;
                    bool isFemale = agent.Age >= 14f && agent.IsFemale;
                    MetaMesh oldMesh = agent.SpawnEquipment[equipmentIndex].GetMultiMesh(isFemale, hasGloves, true);
                    MetaMesh newMesh = oldMesh.CreateCopy();
                    if (oldMesh != null)
                    {
                        if (itemObject.IsUsingTeamColor)
                        {
                            for (int i = 0; i < newMesh.MeshCount; i++)
                            {
                                Mesh meshAtIndex = newMesh.GetMeshAtIndex(i);
                                if (!meshAtIndex.HasTag("no_team_color"))
                                {
                                    meshAtIndex.Color = color3;
                                    meshAtIndex.Color2 = color4;
                                    Material material = meshAtIndex.GetMaterial().CreateCopy();
                                    material.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
                                    meshAtIndex.SetMaterial(material);
                                }
                                meshAtIndex.ManualInvalidate();
                            }
                        }
                        if (itemObject.UsingFacegenScaling)
                        {
                            newMesh.UseHeadBoneFaceGenScaling(agent.AgentVisuals.GetSkeleton(), agent.Monster.HeadLookDirectionBoneIndex, agent.AgentVisuals.GetFacegenScalingMatrix());
                        }
                        Skeleton skeleton = agent.AgentVisuals.GetSkeleton();
                        int num = (skeleton != null) ? skeleton.GetComponentCount(GameEntity.ComponentType.ClothSimulator) : -1;
                        agent.AgentVisuals.ReplaceMeshWithMesh(oldMesh, newMesh, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
                        // agent.AgentVisuals.AddMultiMesh(multiMesh, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
                        oldMesh.ManualInvalidate();
                        newMesh.ManualInvalidate();
                        int num2 = (skeleton != null) ? skeleton.GetComponentCount(GameEntity.ComponentType.ClothSimulator) : -1;
                        if (skeleton != null && equipmentIndex == EquipmentIndex.Cape && num2 > num)
                        {
                            GameEntityComponent componentAtIndex = skeleton.GetComponentAtIndex(GameEntity.ComponentType.ClothSimulator, num2 - 1);
                            agent.SetCapeClothSimulator(componentAtIndex);
                        }
                    }
                    if (equipmentIndex == EquipmentIndex.Body && !string.IsNullOrEmpty(itemObject.ArmBandMeshName))
                    {
                        MetaMesh copy = MetaMesh.GetCopy(itemObject.ArmBandMeshName, true, true);
                        if (copy != null)
                        {
                            if (itemObject.IsUsingTeamColor)
                            {
                                for (int j = 0; j < copy.MeshCount; j++)
                                {
                                    Mesh meshAtIndex2 = copy.GetMeshAtIndex(j);
                                    if (!meshAtIndex2.HasTag("no_team_color"))
                                    {
                                        meshAtIndex2.Color = color3;
                                        meshAtIndex2.Color2 = color4;
                                        Material material2 = meshAtIndex2.GetMaterial().CreateCopy();
                                        material2.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
                                        meshAtIndex2.SetMaterial(material2);
                                    }
                                    meshAtIndex2.ManualInvalidate();
                                }
                            }
                            agent.AgentVisuals.AddMultiMesh(copy, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
                            copy.ManualInvalidate();
                        }
                    }
                }
            }
            ItemObject item = agent.SpawnEquipment[EquipmentIndex.Body].Item;
            if (item != null)
            {
                int lodAtlasIndex = item.LodAtlasIndex;
                if (lodAtlasIndex != -1)
                {
                    agent.AgentVisuals.SetLodAtlasShadingIndex(lodAtlasIndex, true, agent.ClothingColor1, agent.ClothingColor2);
                }
            }
            ArmorComponent.ArmorMaterialTypes bodyArmorMaterialType = ArmorComponent.ArmorMaterialTypes.None;
            ItemObject item2 = agent.SpawnEquipment[EquipmentIndex.Body].Item;
            if (item2 != null)
            {
                bodyArmorMaterialType = item2.ArmorComponent.MaterialType;
            }
            agent.SetBodyArmorMaterialType(bodyArmorMaterialType);
            UpdateAgentLabel(agent);
        }
    }
}
