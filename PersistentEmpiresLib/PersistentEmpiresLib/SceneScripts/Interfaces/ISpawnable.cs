﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.SceneScripts.Interfaces
{
    public interface ISpawnable
    {
        void OnSpawnedByPrefab(PE_PrefabSpawner spawner);
    }
}
