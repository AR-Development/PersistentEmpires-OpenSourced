#if SERVER
using Newtonsoft.Json;
using PersistentEmpiresLib;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using TaleWorlds.MountAndBlade;
using System.Threading.Tasks;

namespace PersistentEmpiresServer.ServerMissions
{
    // logs
    public class DiscordBehavior : MissionNetwork
    {
        private static HttpClient HttpClient = null;
        private static bool DiscordAdminMessageEnabled
        {
            get
            {
                return  ConfigManager.GetBoolConfig("DiscordAdminMessageEnabled", true);
            }
        }
        private static string DiscordAdminMessageUrl
        {
            get
            {
                return ConfigManager.GetStrConfig("DiscordAdminMessageUrl", "");
            }
        }
        private static bool DiscordAnnounceEnabled
        {
            get
            {
                return ConfigManager.GetBoolConfig("DiscordAnnounceEnabled", true);
            }
        }
        private static string DiscordAnnounceUrl
        {
            get
            {
                return ConfigManager.GetStrConfig("DiscordAnnounceUrl", "");
            }
        }
        private static bool DiscordServeStatusEnabled
        {
            get
            {
                return ConfigManager.GetBoolConfig("DiscordServeStatusEnabled", true);
            }
        }
        private static string DiscordServeStatusUrl
        {
            get
            {
                return ConfigManager.GetStrConfig("DiscordServeStatusUrl", "");
            }
        }
        private static bool DiscordExceptionEnabled
        {
            get
            {
                return ConfigManager.GetBoolConfig("DiscordExceptionEnabled", true);
            }
        }
        private static string DiscordExceptionUrl
        {
            get
            {
                return ConfigManager.GetStrConfig("DiscordExceptionUrl", "");
            }
        }
        private static bool DiscordLogEnabled
        {
            get
            {
                return ConfigManager.GetBoolConfig("DiscordLogEnabled", true);
            }
        }
        private static string DiscordLogUrl
        {
            get
            {
                return ConfigManager.GetStrConfig("DiscordLogUrl", "");
            }
        }
        private static string ServerName = "Server Watcher";
        public const string ColorBlue = "1F61E6";
        public const string ColorGreen = "80E61F";
        public const string ColorRed = "E7421F";
        public const string ColorPurple = "C61FE6";
        public const string ColorYellow = "E6C71F";
        public static bool Initialized = false;

        public DiscordBehavior()
        {
        }

        public static void NotifyLog(PersistentEmpiresLib.Database.DBEntities.DBLog dbLog)
        {
            Task.Run(() =>
            {
                if (!DiscordLogEnabled || string.IsNullOrEmpty(DiscordLogUrl))
                {
                    return;
                }

                var request = new
                {
                    username = ServerName,
                    content = "Log",
                    embeds = new List<object>
                    {
                        new
                        {
                            title = dbLog.ActionType,
                            color = int.Parse(ColorGreen, System.Globalization.NumberStyles.HexNumber),
                            fields = new List<object>
                            {
                                new
                                {
                                    name = "Player name",
                                    value = dbLog.IssuerPlayerName,
                                    inline = false,
                                },
                                new
                                {
                                    name = "Player Id",
                                    value = dbLog.IssuerPlayerId,
                                    inline = false,
                                },
                                new
                                {
                                    name = "Coordinates",
                                    value = dbLog.IssuerCoordinates,
                                    inline = false,
                                },
                                new
                                {
                                    name = "Message",
                                    value = dbLog.LogMessage,
                                    inline = false,
                                },
                                new
                                {
                                    name = "Affected Players",
                                    value = string.Join(", ", dbLog.AffectedPlayers.Value.Select(x=> x.SteamId + "." + x.PlayerName + "(" + x.FactionName + ")")),
                                    inline = false,
                                }
                            }
                        },
                    }
                };

                Notify(request, DiscordLogUrl);
            });
        }

        public static void NotifyException(Exception ex)
        {
            Task.Run(() =>
            {
                if (!DiscordExceptionEnabled || string.IsNullOrEmpty(DiscordExceptionUrl))
                {
                    return;
                }

                var request = new
                {
                    username = ServerName,
                    content = "Exception Report",
                    embeds = new List<object>
                    {
                        new
                        {
                            title = "UNHANDLED EXCEPTION THROWN",
                            color = int.Parse(ColorRed, System.Globalization.NumberStyles.HexNumber),
                            fields = new List<object>
                            {
                                new
                                {
                                    name = "Message",
                                    value = ex.Message,
                                    inline = false,
                                },
                                new
                                {
                                    name = "StackTrace",
                                    value = ex.StackTrace,
                                    inline = true,
                                },
                                new
                                {
                                    name = "Exception",
                                    value = ex.ToString(),
                                    inline = true,
                                }
                            }
                        },
                        new
                        {
                            title = "Inner Exception",
                            color = int.Parse(ColorRed, System.Globalization.NumberStyles.HexNumber),
                            fields = new List<object>
                            {
                                new
                                {
                                    name = "Message",
                                    value = ex.InnerException?.Message,
                                    inline = false,
                                },
                                new
                                {
                                    name = "StackTrace",
                                    value = ex.InnerException?.StackTrace,
                                    inline = true,
                                },
                                new
                                {
                                    name = "Exception",
                                    value = ex.InnerException?.ToString(),
                                    inline = true,
                                }
                            }
                        },
                    }
                };

                Notify(request, DiscordExceptionUrl);
            });
        }

        public static void NotifyException(string ex)
        {
            Task.Run(() =>
            {
                if (!DiscordExceptionEnabled || string.IsNullOrEmpty(DiscordExceptionUrl))
            {
                return;
            }

            var request = new
            {
                username = ServerName,
                content = "Exception Report",
                embeds = new List<object>
                {
                    new
                    {
                        title = "UNHANDLED EXCEPTION THROWN",
                        color = int.Parse(ColorRed, System.Globalization.NumberStyles.HexNumber),
                        fields = new List<object>
                        {
                            new
                            {
                                name = "Exception",
                                value = ex,
                                inline = false,
                            },
                        }
                    },
                }
            };

            Notify(request, DiscordExceptionUrl);
            });
        }

        public static void NotifyServerStatus(string message, string color)
        {
            Task.Run(() =>
            {
                if (!DiscordServeStatusEnabled || string.IsNullOrEmpty(DiscordServeStatusUrl))
            {
                return;
            }

            var request = new
            {
                username = ServerName,
                embeds = new List<object>
                {
                    new
                    {
                        title = message,
                        //description = message,
                        color = int.Parse(color, System.Globalization.NumberStyles.HexNumber),
                        author = new
                        {
                            name = "Server Status",
                        },
                    },
                }
            };

            Notify(request, DiscordServeStatusUrl);
            });
        }

        public static void NotifyAdminMessage(NetworkCommunicator player, string message)
        {
            Task.Run(() =>
            {
                if (!DiscordAdminMessageEnabled || string.IsNullOrEmpty(DiscordAdminMessageUrl))
            {
                return;
            }
            //DBPlayer dBPlayer = SaveSystemBehavior.GetDBPlayer(player.VirtualPlayer.ToPlayerId());
            var request = new
            {
                username = ServerName,
                embeds = new List<object>
                {
                    new
                    {
                        title = "Admin chat",
                        description = message,
                        color = int.Parse(ColorGreen, System.Globalization.NumberStyles.HexNumber),
                        author = new
                        {
                            name = player.VirtualPlayer?.UserName,
                            //url = string.IsNullOrEmpty(dBPlayer.DiscordId) ? "" : ,
                        },
                    },
                }
            };

            Notify(request, DiscordAdminMessageUrl);
            });
        }

        public static void NotifyAnnounce(string user, string message)
        {
            Task.Run(() =>
            {
                if (!DiscordAnnounceEnabled || string.IsNullOrEmpty(DiscordAnnounceUrl))
            {
                return;
            }

            var request = new
            {
                username = ServerName,
                embeds = new List<object>
                {
                    new
                    {
                        title = "Admin message",
                        description = message,
                        color = int.Parse(ColorRed, System.Globalization.NumberStyles.HexNumber),
                        author = new
                        {
                            name = user,
                        },
                    },
                }
            };

            Notify(request, DiscordAnnounceUrl);
            });
        }

        private static void Notify(object request, string url)
        {
            try
            {
                var dataString = JsonConvert.SerializeObject(request);
                var content = new StringContent(dataString, Encoding.UTF8, "application/json");

                if (HttpClient == null)
                {
                    HttpClient = new HttpClient();
                }
                
                var response = HttpClient.PostAsync(url, content).Result;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
#endif

/*
 var request = new
            {
                username = user,
                content = "Admin message" + Environment.NewLine + "Line 222",
                //avatar_url = "https://cdn.shopify.com/s/files/1/0185/5092/products/persons-0041_large.png?v=1369543932",
                embeds = new List<object>
                {
                    new
                    {
                        title = "Embed",
                        url = "https://www.google.com/search?q=something",
                        description = "This is the description section of the Embed, the embed has a color bar to the left side",
                        color = int.Parse(ColorGreen, System.Globalization.NumberStyles.HexNumber),
                        timestamp = DateTime.UtcNow.ToString(),
                        author = new
                        {
                            name = "Author name",
                        },
                        footer  = new
                        {
                            text = "footer text",
                        },
                        fields = new List<object>
                        {
                            new
                            {
                                name = "question",
                                value = "answer",
                                inline = false,
                            },
                            new
                            {
                                name = "question 1",
                                value = "answer 1",
                                inline = true,
                            }
                        }
                    },
                    new
                    {
                        title = "Another Embed 2",
                        url = "https://www.google.com/search?q=somethingElse",
                        description = "Line 1 " +Environment.NewLine +
                        "Line 2",
                        color = int.Parse(colorYellow, System.Globalization.NumberStyles.HexNumber)
                    },
                    new
                    {
                        title = "Another Embeddddd",
                        url = "https://www.google.com/search?q=somethingElse",
                        description = "This is the description section of the Embed, the embed has a color bar to the left side",
                        color = int.Parse(colorPurple, System.Globalization.NumberStyles.HexNumber)
                    },
                    new
                    {
                        title = "Another Embeddddd",
                        url = "https://www.google.com/search?q=somethingElse",
                        description = "This is the description section of the Embed, the embed has a color bar to the left side",
                        color = int.Parse(colorPurple, System.Globalization.NumberStyles.HexNumber)
                    },
                    new
                    {
                        title = "Another Embeddddd",
                        url = "https://www.google.com/search?q=somethingElse",
                        description = "This is the description section of the Embed, the embed has a color bar to the left side",
                        color = int.Parse(colorPurple, System.Globalization.NumberStyles.HexNumber)
                    },
                }
            };
 */