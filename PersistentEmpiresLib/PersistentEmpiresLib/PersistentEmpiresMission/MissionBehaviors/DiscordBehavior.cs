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
        private static bool DiscordServerRestartEnabled;
        private static string DiscordServerRestartUrl;
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
            DiscordServerRestartEnabled = ConfigManager.GetBoolConfig("DiscordServerRestartEnabled", true);
            DiscordServerRestartUrl = ConfigManager.GetStrConfig("DiscordServerRestartUrl", "");
        }

        public static bool NotifyServerRestart(string message)
        {
            if (!DiscordServerRestartEnabled || string.IsNullOrEmpty(DiscordServerRestartUrl))
            {
                return false;
            }

            var content = new StringContent("{\"content\": \"" +
                    $"UTC time: {DateTime.UtcNow.ToLocalTime()}" +
                    Environment.NewLine +
                    message +
                    "\"}", System.Text.Encoding.UTF8, "application/json");

            return Notify(content, DiscordServerRestartUrl);
        }

        public static bool NotifyAdminMessage(string message)
        {
            if(!DiscordAdminMessageEnabled || string.IsNullOrEmpty(DiscordAdminMessageUrl))
            {
                return false;
            }
            
            var content = new StringContent("{\"content\": \"" +
                    $"UTC time: {DateTime.UtcNow.ToLocalTime()}" +
                    Environment.NewLine +
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