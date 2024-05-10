using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace PersistentEmpiresLib.Helpers
{
    public class BannerRenderer
    {
        public delegate void RequestRenderBannerFromView(Banner banner, GameEntity renderOnTo);
        public static event RequestRenderBannerFromView OnRequestRenderBanner;

        // [HandleProcessCorruptedStateExceptions]
        // [SecurityCritical]
        public static void RequestRenderBanner(Banner banner, GameEntity renderOnTo)
        {
            if (OnRequestRenderBanner != null) OnRequestRenderBanner(banner, renderOnTo);
        }

        // [HandleProcessCorruptedStateExceptions]
        // [SecurityCritical]
        public static void OnBannerTableauRenderDone(GameEntity gameEntity, Texture bannerTexture)
        {
            if (gameEntity == null) return;
            try
            {
                foreach (Mesh bannerMesh in gameEntity.GetAllMeshesWithTag("banner_replacement_mesh"))
                {
                    if (bannerMesh.IsValid)
                    {
                        BannerRenderer.ApplyBannerTextureToMesh(bannerMesh, bannerTexture);
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
            Skeleton skeleton = gameEntity.Skeleton;
            try
            {
                if (((skeleton != null) ? skeleton.GetAllMeshes() : null) != null)
                {
                    Skeleton skeleton2 = gameEntity.Skeleton;
                    foreach (Mesh mesh in ((skeleton2 != null) ? skeleton2.GetAllMeshes() : null))
                    {
                        if (mesh.HasTag("banner_replacement_mesh"))
                        {
                            BannerRenderer.ApplyBannerTextureToMesh(mesh, bannerTexture);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        public static void ApplyBannerTextureToMesh(Mesh bannerMesh, Texture bannerTexture)
        {
            if (bannerMesh != null)
            {
                Material material = bannerMesh.GetMaterial().CreateCopy();
                material.SetTexture(Material.MBTextureType.DiffuseMap2, bannerTexture);
                uint num = (uint)material.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
                ulong shaderFlags = material.GetShaderFlags();
                material.SetShaderFlags(shaderFlags | (ulong)num);
                bannerMesh.SetMaterial(material);
            }
        }
    }
}
