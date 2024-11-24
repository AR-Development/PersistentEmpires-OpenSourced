using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.AnimationMenu;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEAnimationsView : MissionView
    {
        private bool IsActive;
        private PEAnimationMenuVM _dataSource;
        private GauntletLayer _gauntletLayer;
        

        public PEAnimationsView() { }

        private List<PEAnimationSubMenuVM> ParseXml()
        {
            List<PEAnimationSubMenuVM> animationSubMenuVMs = new List<PEAnimationSubMenuVM>();
            string Animations = ModuleHelper.GetXmlPath(AnimationBehavior.AnimationModuleName, AnimationBehavior.AnimationFileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Animations);

            XmlNodeList categoryNodes = xmlDoc.SelectNodes("/Animations/Category");
            if (categoryNodes != null && categoryNodes.Count > 0)
            {
                int i = 1;
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // Get the category name
                    string categoryName = i + " - " + categoryNode.SelectSingleNode("Name")?.InnerText.Trim();
                    // Console.WriteLine("Category Name: " + categoryName);

                    // Get the animation nodes within the category
                    XmlNodeList animationNodes = categoryNode.SelectNodes("Childrens/Animation");
                    List<PEAnimationVM> animations = new List<PEAnimationVM>();
                    if (animationNodes != null && animationNodes.Count > 0)
                    {
                        int j = 1;
                        foreach (XmlNode animationNode in animationNodes)
                        {
                            // Get the animation name and ID
                            string animationName = j + " - " + animationNode.SelectSingleNode("Name")?.InnerText.Trim();
                            string animationId = animationNode.SelectSingleNode("AnimationId")?.InnerText.Trim();
                            animations.Add(new PEAnimationVM(animationName, animationId));
                            j++;
                        }
                    }
                    PEAnimationSubMenuVM animationSubMenuVM = new PEAnimationSubMenuVM(categoryName, animations);
                    animationSubMenuVMs.Add(animationSubMenuVM);
                    i++;
                }
            }
            return animationSubMenuVMs;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            List<PEAnimationSubMenuVM> animations = this.ParseXml();
            this._dataSource = new PEAnimationMenuVM(animations);

            this._dataSource.OnAnimationSelected += _dataSource_OnAnimationSelected;
        }

        private void _dataSource_OnAnimationSelected(string actionId)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestAnimation(actionId));
            GameNetwork.EndModuleEventAsClient();

            this.CloseImportExportAux();
            this.IsActive = false;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!this.IsActive && base.MissionScreen.SceneLayer.Input.IsGameKeyReleased(31))
            {
                this.OnOpen();
            }

            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D0))
            {
                this._dataSource.SelectInput(0);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D1))
            {
                this._dataSource.SelectInput(1);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D2))
            {
                this._dataSource.SelectInput(2);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D3))
            {
                this._dataSource.SelectInput(3);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D4))
            {
                this._dataSource.SelectInput(4);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D5))
            {
                this._dataSource.SelectInput(5);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D6))
            {
                this._dataSource.SelectInput(6);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D7))
            {
                this._dataSource.SelectInput(7);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D8))
            {
                this._dataSource.SelectInput(8);
            }
            if (this.IsActive && base.MissionScreen.SceneLayer.Input.IsKeyReleased(InputKey.D9))
            {
                this._dataSource.SelectInput(9);
            }
        }

        private void OnOpen()
        {
            if (this.IsActive) return;
            if (base.MissionScreen != null)
            {
                this._gauntletLayer = new GauntletLayer(50);
                this._gauntletLayer.LoadMovie("PEAnimationMenu", this._dataSource);
                base.MissionScreen.AddLayer(this._gauntletLayer);
                this.IsActive = true;
            }
        }

        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            if (this.IsActive)
            {

                this.CloseImportExportAux();
                this.IsActive = false;
                return true;
            }
            return result;
        }

        private void CloseImportExportAux()
        {
            if (this._dataSource.IsSelected)
            {
                this._dataSource.SelectedMenu.PageNumber = 0;
                this._dataSource.SelectedMenu = null;
            }
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }


    }
}
