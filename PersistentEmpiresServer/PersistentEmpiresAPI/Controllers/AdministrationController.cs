using Microsoft.AspNetCore.Mvc;
using PersistentEmpiresAPI.DTO;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresSave.Database;
using PersistentEmpiresSave.Database.Repositories;
using System.Net;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace PersistentEmpiresAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdministrationController : ControllerBase
    {

        private IPAddress getIpAddress(NetworkCommunicator peer)
        {
            byte[] bytes = BitConverter.GetBytes(peer.GetHost());
            return new IPAddress(bytes);
        }

        [HttpGet]
        public bool Index()
        {
            return true;
        }

        [HttpPost("compensateplayer")]
        public ActionResult<ResultDTO> CompensatePlayer(CompensatePlayerDTO request)
        {

            foreach (NetworkCommunicator communicator in GameNetwork.NetworkPeers.ToArray())
            {
                if (communicator.VirtualPlayer.ToPlayerId() == request.PlayerId && communicator.IsConnectionActive)
                {
                    // DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(communicator.VirtualPlayer.Id, false);
                    PersistentEmpireRepresentative persistentEmpireRepresentative = communicator.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative != null)
                    {
                        if (request.Gold > 0) persistentEmpireRepresentative.GoldGain(request.Gold);
                        else persistentEmpireRepresentative.GoldLost(request.Gold * -1);
                    }
                    break;
                }
            }
            string query = "UPDATE Players SET Money = Money + @Gold WHERE PlayerId = @PlayerId";
            DBConnection.ExecuteDapper(query, new
            {
                PlayerId = request.PlayerId,
                Gold = request.Gold
            });
            return new ResultDTO
            {
                Status = true,
                Reason = "Player Compensated"
            };
        }

        [HttpPost("kickplayer")]
        public ActionResult<ResultDTO> KickPlayer(KickPlayerDTO request)
        {

            foreach (NetworkCommunicator communicator in GameNetwork.NetworkPeers.ToArray())
            {
                if (communicator.VirtualPlayer.ToPlayerId() == request.PlayerId && communicator.IsConnectionActive)
                {
                    InformationComponent.Instance.SendMessage("You have been kicked from the server. Please refer to discord for further information.", (new Color(1f, 0f, 0f)).ToUnsignedInteger(), communicator);
                    DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(communicator.VirtualPlayer.Id, false);
                    break;
                }
            }

            return new ResultDTO
            {
                Status = true,
                Reason = "Player kicked"
            };
        }

        [HttpPost("fadeplayer")]
        public ActionResult<ResultDTO> FadePlayer(FadePlayerDTO request)
        {

            foreach (NetworkCommunicator communicator in GameNetwork.NetworkPeers.ToArray())
            {
                if (communicator.VirtualPlayer.ToPlayerId() == request.PlayerId && communicator.IsConnectionActive)
                {
                    if (communicator.ControlledAgent != null && communicator.ControlledAgent.IsActive())
                    {
                        communicator.ControlledAgent.FadeOut(true, true);
                    }
                    break;
                }
            }

            string query = "UPDATE Players SET Horse = NULL, HorseHarness = NULL, Equipment_0 = NULL, Equipment_1 = NULL, Equipment_2 = NULL, Equipment_3 = NULL, Armor_Head = NULL, Armor_Body = NULL, Armor_Leg = NULL, Armor_Gloves = NULL, Armor_Cape = NULL, PosX = 0, PosY = 0, PosZ = 0, Ammo_0 = 0, Ammo_1 = 0, Ammo_2 = 0, Ammo_3 = 0 WHERE PlayerId = @PlayerId";

            DBConnection.ExecuteDapper(query, new { PlayerId = request.PlayerId });

            return new ResultDTO
            {
                Status = true,
                Reason = "Player faded"
            };
        }

        [HttpPost("unbanplayer")]
        public ActionResult<ResultDTO> UnBanPlayer(UnbanPlayerDTO request)
        {
            bool isPlayerBanned = DBBanRecordRepository.IsPlayerBanned(request.PlayerId);

            if (!isPlayerBanned)
            {
                return new ResultDTO
                {
                    Status = false,
                    Reason = "Player not banned"
                };
            }

            DBBanRecordRepository.UnbanPlayer(request.PlayerId, request.UnbanReason);


            return new ResultDTO
            {
                Status = true,
                Reason = "Player UnBanned"
            };
        }

        [HttpPost("banplayer")]
        public ActionResult<ResultDTO> BanPlayer(BanPlayerDTO request)
        {
            IEnumerable<DBPlayer> players = DBPlayerRepository.GetPlayerFromId(request.PlayerId);
            if (players.Count() == 0)
            {
                return new ResultDTO
                {
                    Status = false,
                    Reason = "Player not found"
                };
            }
            DBPlayer player = players.First();

            DBBanRecordRepository.BanPlayer(request.PlayerId, player.Name, request.BanEndsAt, request.BanReason);

            foreach (NetworkCommunicator communicator in GameNetwork.NetworkPeers.ToArray())
            {
                if (communicator.VirtualPlayer.ToPlayerId() == request.PlayerId && communicator.IsConnectionActive)
                {
                    InformationComponent.Instance.SendMessage("You have been banned from the server. Reason is " + request.BanReason + " please refer to discord for further information.", (new Color(1f, 0f, 0f)).ToUnsignedInteger(), communicator);
                    DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(communicator.VirtualPlayer.Id, false);
                    break;
                }
            }

            return new ResultDTO
            {
                Status = true,
                Reason = "Player banned"
            };
        }

        [HttpGet("servercap")]
        public ActionResult<ResultDTO> ServerCap()
        {
            return new ResultDTO
            {
                Status = true,
                Reason = GameNetwork.NetworkPeerCount.ToString()
            };
        }

        [HttpGet("restart")]
        public ActionResult<ResultDTO> Restart()
        {
            Mission.Current.EndMission();
            return new ResultDTO
            {
                Status = true,
                Reason = GameNetwork.NetworkPeerCount.ToString()
            };
        }

        [HttpPost("announce")]
        public ActionResult<ResultDTO> Announce(AnnounceDTO request)
        {
            InformationComponent.Instance.BroadcastAnnouncement(request.Message, Color.ConvertStringToColor("#FFFF6666").ToUnsignedInteger());

            return new ResultDTO
            {
                Status = true,
                Reason = "Done"
            };
        }

        [HttpGet("shutdown")]
        public ActionResult<ResultDTO> Shutdown()
        {
            System.Environment.Exit(0);
            return new ResultDTO
            {
                Reason = "Done.",
                Status = true
            };
        }
    }
}
