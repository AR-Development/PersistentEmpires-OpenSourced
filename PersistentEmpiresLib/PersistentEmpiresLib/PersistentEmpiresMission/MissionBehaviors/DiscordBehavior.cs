#if SERVER
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ServerMissions
{
    // logs
    public class DiscordBehavior : MissionNetwork
    {
        private static HttpClient HttpClient = null;
        private static bool DiscordAdminMessageEnabled;
        private static string DiscordAdminMessageUrl;
        private static bool DiscordAnnounceEnabled;
        private static string DiscordAnnounceUrl;
        private static bool DiscordServeStatusEnabled;
        private static string DiscordServeStatusUrl;
        private static bool DiscordExceptionEnabled;
        private static string DiscordExceptionUrl;
        private static bool DiscordLogEnabled;
        private static string DiscordLogUrl;
        private static string ServerName = "Server Watcher";
        public const string ColorBlue = "1F61E6";
        public const string ColorGreen = "80E61F";
        public const string ColorRed = "E7421F";
        public const string ColorPurple = "C61FE6";
        public const string ColorYellow = "E6C71F";

        public DiscordBehavior()
        {
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            DiscordAdminMessageEnabled = ConfigManager.GetBoolConfig("DiscordAdminMessageEnabled", true);
            DiscordAdminMessageUrl = ConfigManager.GetStrConfig("DiscordAdminMessageUrl", "");
            DiscordAnnounceEnabled = ConfigManager.GetBoolConfig("DiscordAnnounceEnabled", true);
            DiscordAnnounceUrl = ConfigManager.GetStrConfig("DiscordAnnounceUrl", "");
            DiscordServeStatusEnabled = ConfigManager.GetBoolConfig("DiscordServeStatusEnabled", true);
            DiscordServeStatusUrl = ConfigManager.GetStrConfig("DiscordServeStatusUrl", "");
            DiscordExceptionEnabled = ConfigManager.GetBoolConfig("DiscordExceptionEnabled", true);
            DiscordExceptionUrl = ConfigManager.GetStrConfig("DiscordExceptionUrl", "");
            DiscordLogEnabled = ConfigManager.GetBoolConfig("DiscordLogEnabled", true);
            DiscordLogUrl = ConfigManager.GetStrConfig("DiscordLogUrl", "");
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
                                value = dbLog.AffectedPlayers,
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
                        description = message,
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

        public static bool NotifyAdminMessage(string message)
        {
            if(!DiscordAdminMessageEnabled || string.IsNullOrEmpty(DiscordAdminMessageUrl))
            {
                return false;
            }
            
            var content = new StringContent("{\"content\": \"" +
                    message +
                    "\"}", System.Text.Encoding.UTF8, "application/json");

            return Notify(request, DiscordAdminMessageUrl);
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
            var dataString = JsonConvert.SerializeObject(request);
            var content = new StringContent(dataString, Encoding.UTF8, "application/json");

            if (HttpClient == null)
            {
                HttpClient = new HttpClient();
            }
            try
            {
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