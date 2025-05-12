using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using PersistentEmpiresLib;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Xml;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ListedServer;

namespace PersistentEmpiresAPI
{
    public class PersistentEmpiresAPISubModule : MBSubModuleBase
    {
        public static string XmlFile = "ApiConfig";
        public static string SecretKey = "";
        private IWebHost _host;

        public static int GetConfigPort()
        {
            string xmlPath = ModuleHelper.GetXmlPath(Main.ModuleName, "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode portElement = xmlDocument.SelectSingleNode("/ApiConfig/Port");
            return int.Parse(portElement.InnerText);
        }

        public static string GetSecretKey()
        {
            string xmlPath = ModuleHelper.GetXmlPath(Main.ModuleName, "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode secretElement = xmlDocument.SelectSingleNode("/ApiConfig/SecretKey");
            return secretElement.InnerText;
        }
        public static string GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PersistentEmpiresAPISubModule.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var secToken = new JwtSecurityToken(
                signingCredentials: credentials,
                issuer: "Sample",
                audience: "Sample",
                claims: new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, "mentalrob")
                },
                expires: DateTime.UtcNow.AddDays(1));

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }
        public static PersistentEmpiresAPISubModule Instance { get; set; }

        protected override void OnSubModuleLoad()
        {
            PersistentEmpiresAPISubModule.Instance = this;
            base.OnSubModuleLoad();
            InitialListedGameServerState.OnActivated += this.DedicatedCustomGameServerStateActivated;

        }

        private void DedicatedCustomGameServerStateActivated()
        {
            int port = GetConfigPort();
            SecretKey = GetSecretKey();
            if (Module.CurrentModule == null)
            {
                Console.WriteLine("Web panel can't be activated! No modules loaded.");
                return;
            }

            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder().ConfigureLogging(delegate (ILoggingBuilder logging)
            {
                logging.ClearProviders();
            }).UseStartup<Startup>();

            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
            defaultInterpolatedStringHandler.AppendLiteral("http://*:");
            defaultInterpolatedStringHandler.AppendFormatted<int>(port);
            Console.ForegroundColor = ConsoleColor.Green;
            string[] array = new string[1];
            array[0] = defaultInterpolatedStringHandler.ToStringAndClear();
            this._host = webHostBuilder.UseUrls(array).Build();
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Persistent Empires API is live at port ");
            defaultInterpolatedStringHandler.AppendFormatted<int>(port);
            defaultInterpolatedStringHandler.AppendLiteral("!");
            Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.WriteLine(string.Format("Persistent Empires API Token Is: {0}", GenerateToken()));
            Console.ResetColor();
            Task.Run(delegate
            {
                this._host.Run();
            });
        }
        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            InitialListedGameServerState.OnActivated -= this.DedicatedCustomGameServerStateActivated;
        }
    }
}
