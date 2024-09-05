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
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Roll : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!roll";
        }

        public string Description()
        {
            return $"/roll (intMin) (intMax) generates a random number between intMin (0) and intMax (100)";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var minInt = 0;
            var maxInt = 100;
            int tmp;

            if (args.Count() > 1)
            {
                if (int.TryParse(args[1], out tmp))
                {
                    if (args.Count() > 2)
                    {
                        minInt = tmp;
                    }
                    else
                    {
                        maxInt = tmp;
                    }
                }
            }

            if (args.Count() > 2)
            {
                if (int.TryParse(args[2], out tmp))
                {
                    maxInt = tmp;
                }
            }

            var rnd = new Random();
            var random = rnd.Next(minInt, maxInt + 1);
            var message = $"{player.UserName} rolls {random} from between {minInt} to {maxInt}.";

            this.SendMessageToPlayers(player, 30, message);

            return true;
        }
    }
}