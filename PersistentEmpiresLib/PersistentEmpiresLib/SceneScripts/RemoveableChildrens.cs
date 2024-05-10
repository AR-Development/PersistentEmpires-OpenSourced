using PersistentEmpiresLib.SceneScripts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;

namespace PersistentEmpiresLib.SceneScripts
{
    public static class RemoveableChildrens
    {
        public static void OnEntityRemove(GameEntity root)
        {
            List<GameEntity> childrens = new List<GameEntity>();
            root.GetChildrenRecursive(ref childrens);
            foreach (GameEntity child in childrens)
            {
                ScriptComponentBehavior[] scripts = child.GetScriptComponents().Where(s => s is IRemoveable).ToArray();
                foreach (ScriptComponentBehavior script in scripts)
                {
                    ((IRemoveable)script).OnEntityRemove();
                }
            }
        }
    }
}
