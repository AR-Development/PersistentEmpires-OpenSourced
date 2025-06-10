#if SERVER
using PersistentEmpiresLib;
using System;
using System.Net.Http;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ServerMissions
{
    public class DiscordBehavior : MissionNetwork
    {
        private static HttpClient HttpClient = null;
        private static bool DiscordAdminMessageEnabled;
        private static string DiscordAdminMessageUrl;
        private static bool DiscordAnnounceEnabled;
        private static string DiscordAnnounceUrl;
        private static bool DiscordServeStatusEnabled;
        private static string DiscordServeStatusUrl;
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
        }

        public static bool NotifyServerStatus(string message)
        {
            if (!DiscordServeStatusEnabled || string.IsNullOrEmpty(DiscordServeStatusUrl))
            {
                return false;
            }

            var content = new StringContent("{\"content\": \"" +
                    $"UTC time: {DateTime.UtcNow.ToLocalTime()}" +
                    Environment.NewLine +
                    message +
                    "\"}", System.Text.Encoding.UTF8, "application/json");

            return Notify(content, DiscordServeStatusUrl);
        }

        public static bool NotifyAdminMessage(string message)
        {
            if(!DiscordAdminMessageEnabled || string.IsNullOrEmpty(DiscordAdminMessageUrl))
            {
                return false;
            }
            
            var content = new StringContent("{\"content\": \"" +
                    //$"UTC time: {DateTime.UtcNow.ToLocalTime()}" +
                    //Environment.NewLine +
                    message +
                    "\"}", System.Text.Encoding.UTF8, "application/json");

            return Notify(content, DiscordAdminMessageUrl);
        }

        public static bool NotifyAnnounce(string message)
        {
            if (!DiscordAnnounceEnabled || string.IsNullOrEmpty(DiscordAnnounceUrl))
            {
                return false;
            }

            var content = new StringContent("{\"content\": \"" +
                    $"UTC time: {DateTime.UtcNow.ToLocalTime()}" +
                    Environment.NewLine +
                    message +
                    "\"}", System.Text.Encoding.UTF8, "application/json");

            return Notify(content, DiscordAnnounceUrl);
        }

        private static bool Notify(StringContent content, string url)
        {
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