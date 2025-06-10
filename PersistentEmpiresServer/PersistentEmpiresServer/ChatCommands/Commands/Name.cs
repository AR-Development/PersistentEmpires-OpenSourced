using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Text.RegularExpressions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Name : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("NameColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}name";
        }

        private bool checkAlphaNumeric(String name)
        {
            // ^[a-zA-Z0-9\s,\[,\],\(,\)]*$
            Regex rg = new Regex(@"^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ.\s,\[,\],\(,\),_,-,\p{IsCJKUnifiedIdeographs}]*$");
            return rg.IsMatch(name);
        }


        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (AdminServerBehavior.Instance.LastChangedName.ContainsKey(networkPeer))
            {
                long lastTime = AdminServerBehavior.Instance.LastChangedName[networkPeer];
                if (lastTime + AdminServerBehavior.Instance.cooldown > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    InformationComponent.Instance.SendMessage(
                        String.Format("You need to wait {0} seconds", (lastTime + AdminServerBehavior.Instance.cooldown) - DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                        Colors.Red.ToUnsignedInteger(), networkPeer
                    );
                    return false;
                }
            }

            if (args == null)
            {
                InformationComponent.Instance.SendMessage("Custom name cannot be empty", Color, networkPeer);
                return false;
            }

            string newName = String.Join(" ", args);
            if (newName.Length == 0)
            {
                InformationComponent.Instance.SendMessage("Custom name cannot be empty", Color, networkPeer);
                return false;
            }
            if (!checkAlphaNumeric(newName))
            {
                InformationComponent.Instance.SendMessage("Custom name should be alpha numeric", Color, networkPeer);
                return false;
            }
            if (persistentEmpireRepresentative.HaveEnoughGold(AdminServerBehavior.Instance.nameChangeGold) == false)
            {
                InformationComponent.Instance.SendMessage("You need " + AdminServerBehavior.Instance.nameChangeGold + " gold.", Color, networkPeer);
                return false;
            }
            bool result = SaveSystemBehavior.HandlePlayerUpdateCustomName(networkPeer, newName);
            if (result == false)
            {
                InformationComponent.Instance.SendMessage("You can't set this name", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }

            persistentEmpireRepresentative.ReduceIfHaveEnoughGold(AdminServerBehavior.Instance.nameChangeGold);
            //InformationComponent.Instance.SendMessage("Your name is changed. You need to relog to take effect.", Colors.Green.ToUnsignedInteger(), networkPeer);
            LoggerHelper.LogAnAction(networkPeer, LogAction.PlayerChangedName, null, new object[] { newName });
            AdminServerBehavior.Instance.LastChangedName[networkPeer] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return true;
        }

        public string Description()
        {
            return "Change your name";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [Name]{Environment.NewLine}" +
                    $"Parameter: [Name] new name for player{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Cost: {AdminServerBehavior.Instance.nameChangeGold}{Environment.NewLine}" +
                    $"Cooldown: {AdminServerBehavior.Instance.cooldown}.{Environment.NewLine}" +
                    $"Description: Changes name fhe player to custom one.{Environment.NewLine}" +
                    $"Example: {Command()} Player1{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("NameEnabled", true);
        }
    }
}