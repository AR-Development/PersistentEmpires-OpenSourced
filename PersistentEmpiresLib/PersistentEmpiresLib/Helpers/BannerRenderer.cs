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
