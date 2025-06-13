#if SERVER
using Newtonsoft.Json;
using PersistentEmpiresLib;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ServerMissions
{
    // logs
    public class DiscordBehavior : MissionNetwork
    {
        private static HttpClient HttpClient = null;
        private static bool? _discordAdminMessageEnabled;
        private static bool DiscordAdminMessageEnabled
        {
            get
            {
                if (_discordAdminMessageEnabled == null)
                {
                    _discordAdminMessageEnabled = ConfigManager.GetBoolConfig("DiscordAdminMessageEnabled", true);
                }
                return _discordAdminMessageEnabled.Value;
            }
        }
        private static string _discordAdminMessageUrl;
        private static string DiscordAdminMessageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_discordAdminMessageUrl))
                {
                    _discordAdminMessageUrl = ConfigManager.GetStrConfig("DiscordAdminMessageUrl", "");
                }
                return _discordAdminMessageUrl;
            }
        }
        private static bool? _discordAnnounceEnabled;
        private static bool DiscordAnnounceEnabled
        {
            get
            {
                if (_discordAnnounceEnabled == null)
                {
                    _discordAnnounceEnabled = ConfigManager.GetBoolConfig("DiscordAnnounceEnabled", true);
                }
                return _discordAnnounceEnabled.Value;
            }
        }
        private static string _discordAnnounceUrl;
        private static string DiscordAnnounceUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_discordAnnounceUrl))
                {
                    _discordAnnounceUrl = ConfigManager.GetStrConfig("DiscordAnnounceUrl", "");
                }
                return _discordAnnounceUrl;
            }
        }
        private static bool? _discordServeStatusEnabled;
        private static bool DiscordServeStatusEnabled
        {
            get
            {
                if (_discordServeStatusEnabled == null)
                {
                    _discordServeStatusEnabled = ConfigManager.GetBoolConfig("DiscordServeStatusEnabled", true);
                }
                return _discordServeStatusEnabled.Value;
            }
        }
        private static string _discordServeStatusUrl;
        private static string DiscordServeStatusUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_discordServeStatusUrl))
                {
                    _discordServeStatusUrl = ConfigManager.GetStrConfig("DiscordServeStatusUrl", "");
                }
                return _discordServeStatusUrl;
            }
        }
        private static bool? _discordExceptionEnabled;
        private static bool DiscordExceptionEnabled
        {
            get
            {
                if (_discordExceptionEnabled == null)
                {
                    _discordExceptionEnabled = ConfigManager.GetBoolConfig("DiscordExceptionEnabled", true);
                }
                return _discordExceptionEnabled.Value;
            }
        }
        private static string _discordExceptionUrl;
        private static string DiscordExceptionUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_discordExceptionUrl))
                {
                    _discordExceptionUrl = ConfigManager.GetStrConfig("DiscordExceptionUrl", "");
                }
                return _discordExceptionUrl;
            }
        }
        private static bool? _discordLogEnabled;
        private static bool DiscordLogEnabled
        {
            get
            {
                if (_discordLogEnabled == null)
                {
                    _discordLogEnabled = ConfigManager.GetBoolConfig("DiscordLogEnabled", true);
                }
                return _discordLogEnabled.Value;
            }
        }
        private static string _discordLogUrl;
        private static string DiscordLogUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_discordLogUrl))
                {
                    _discordLogUrl = ConfigManager.GetStrConfig("DiscordLogUrl", "");
                }
                return _discordLogUrl;
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

        public static bool NotifyLog(PersistentEmpiresLib.Database.DBEntities.DBLog dbLog)
        {
            if (!DiscordLogEnabled || string.IsNullOrEmpty(DiscordLogUrl))
            {
                return false;
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

            return Notify(request, DiscordLogUrl);
        }

        public static bool NotifyException(Exception ex)
        {
            if (!DiscordExceptionEnabled || string.IsNullOrEmpty(DiscordExceptionUrl))
            {
                return false;
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

            return Notify(request, DiscordExceptionUrl);
        }

        public static bool NotifyException(string ex)
        {
            if (!DiscordExceptionEnabled || string.IsNullOrEmpty(DiscordExceptionUrl))
            {
                return false;
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

            return Notify(request, DiscordExceptionUrl);
        }

        public static bool NotifyServerStatus(string message, string color)
        {
            if (!DiscordServeStatusEnabled || string.IsNullOrEmpty(DiscordServeStatusUrl))
            {
                return false;
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

            return Notify(request, DiscordServeStatusUrl);
        }

        public static bool NotifyAdminMessage(NetworkCommunicator player, string message)
        {
            if(!DiscordAdminMessageEnabled || string.IsNullOrEmpty(DiscordAdminMessageUrl))
            {
                return false;
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
                            name = player,
                            //url = string.IsNullOrEmpty(dBPlayer.DiscordId) ? "" : ,
                        },
                    },
                }
            };

            return Notify(request, DiscordServeStatusUrl);
        }

        public static bool NotifyAnnounce(string user, string message)
        {
            if (!DiscordAnnounceEnabled || string.IsNullOrEmpty(DiscordAnnounceUrl))
            {
                return false;
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

            return Notify(request, DiscordAnnounceUrl);
        }

        private static bool Notify(object request, string url)
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
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
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