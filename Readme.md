# Persistent Empires Open Sourced

Persistent empires is a Mount & Blade II: Bannerlord mod that introduces some new mechanics to multiplayer gaming that allow players to do roleplay, team fight, clan fight, farming, mining etc...

## Requirements

A Windows Server ( )

You need a database, MariaDB (Preferably 10.4.27-MariaDB) installation (https://mariadb.com/kb/en/installing-mariadb-msi-packages-on-windows/)
or you may use MYSQL8 (8.0.36 should work fine) (https://dev.mysql.com/downloads/installer/)

You need Mount & Blade II: Bannerlord Dedicated Server ( can be installed from steamcmd https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip )

`steamcmd.exe +force_install_dir "C:\Desktop\YourServerFolderLocation" +login your_steam_username "your_steam_password" +app_update 1863440 validate`

## Installation

- After installing the dedicated server, you will see a folder called `Modules` inside of your dedicated server location.

- Extract PersistentEmpires Modules file and folders `YourServerLocation/Modules/PersistentEmpires`

- Extract `PersistentEmpires/bin/Win64_ShippingServer` to `YourServerLocation/bin/Win64_ShippingServer`

- Install a MySQL explorer (Navicat, DBeaver, phpmyadmin or mysqlworkbench)

<p align="center">
  <img src="https://github.com/Heavybob/PersistentEmpires-OpenSourced/assets/4519067/e83817b5-a4e7-44a3-81c0-bb099206452a" alt="DBeaver Setup">
</p>
<p align="center"><em>DBeaver Setup</em></p>

- Authorize to your MariaDB setup and create a database, lets name it pe_production

<p align="center">
  <img src="https://github.com/Heavybob/PersistentEmpires-OpenSourced/assets/4519067/a7c801f7-92a7-430b-a77d-7ee90d3dcff5" alt="image">
</p>
<p align="center"><em>Created a database called pe_production, ignore other databases</em></p>

- Now you need to set up your database connection for your PE server. To do that go to file
`YourServerFolder/Modules/PersistentEmpires/ModuleData/Configs/SaveConfig.xml`

- If you edit the file you see the file content like this:

SaveConfig.xml
```xml
<DatabaseConfig>
	<ConnectionString>
		Server=localhost;User ID=root;Password=password;Database=pe_production
	</ConnectionString>
</DatabaseConfig>
```

- Set the User ID, Password and Database field for your database.

- You need to create this file at this location.  `YourServerLocation/Modules/Native/persistent_empires.txt`

persistent_empires.txt
```txt
ServerName Persistent Empires
GameType PersistentEmpires
Map pe_test3
CultureTeam1 khuzait
CultureTeam2 vlandia
AllowPollsToKickPlayers False
AllowPollsToBanPlayers False
AllowPollsToChangeMaps False
MapTimeLimit 60000
RespawnPeriodTeam1 5
RespawnPeriodTeam1 5
MinNumberOfPlayersForMatchStart 0
MaxNumberOfPlayers 500
DisableInactivityKick True
add_map_to_automated_battle_pool pe_test3
end_game_after_mission_is_over
start_game_and_mission
```
- Create your starter bat file and you are ready to go.

```.bat
start DedicatedCustomServer.Starter.exe /dedicatedcustomserverconfigfile persistent_empires.txt /port 7211 /DisableErrorReporting /no_watchdog /tickrate 75 /multiplayer /dedicatedcustomserverauthtoken INSERTCUSTOMSERVERAUTHTOKENHERE _MODULES_*Native*Multiplayer*PersistentEmpires*_MODULES_
```

- Don't forget to set your `/dedicatedcustomserverauthtoken` that you must obtain from bannerlord.
- Refer to https://moddocs.bannerlord.com/multiplayer/hosting_server/

- Ensure you've also set the correct map you intend to use in your persistent_empires.txt

- Maps must only be placed into the `Multiplayer` module. `YourServerLocation/Modules/Multiplayer/SceneObj` and must be targeted with `add_map_to_automated_battle_pool` in your persistent_empires.txt in order to appear as downloadable via the ingame download panel. 

## Server Configuration

Persistent Empires allow users to configure some options server side.

You can find the configuration file under `YourServerLocation/Modules/PersistentEmpires/ModuleData/Configs/GeneralConfig.xml`

This file could be empty, you can use this configuration file below if you wish however undefined values will just utilize the default values.

GeneralConfig.xml
```xml
<GeneralConfig>
	<!-- Autos -->
	<AutorestartActive>true</AutorestartActive>
	<AutorestartIntervalHours>12</AutorestartIntervalHours>
	<AutosaveDuration>600</AutosaveDuration><!--In seconds-->
	<!--Other-->
	<VoiceChatEnabled>false</VoiceChatEnabled>
	<DontOverrideMangonelHit>false</DontOverrideMangonelHit>
	<AnimationsEnabled>true</AnimationsEnabled>
	<WhitelistEnabled>false</WhitelistEnabled>
	<AgentLabelEnabled>false</AgentLabelEnabled><!-- Banners on top of head -->
	<PersistDisconnectPosition>false</PersistDisconnectPosition>	
	<CombatlogDuration>60</CombatlogDuration>
	<DecapitationChance>0</DecapitationChance>
	<RepairTimeoutAfterHit>60</RepairTimeoutAfterHit><!-- Cooldown for repairs after damage -->
	<ItemDestroyChanceOnDeath>0</ItemDestroyChanceOnDeath>
	<CanUseSuicide>true</CanUseSuicide>
	<!--Money-->
	<StartingGold>0</StartingGold>
	<DeathMoneyDropPercentage>0</DeathMoneyDropPercentage>
	<PickpocketingPercentageThousands>0</PickpocketingPercentageThousands>
	<RequiredPickpocketing>10</RequiredPickpocketing>
	<!--Autopay-->
	<AutoPayEnabled>true</AutoPayEnabled>
	<AutoPayTimeMinutes>20</AutoPayTimeMinutes>
	<AutoPayGold>10</AutoPayGold>
	<!--Name change-->
	<NameChangeGold>0</NameChangeGold>
	<NameChangeCooldownInSeconds>1800</NameChangeCooldownInSeconds>
	<!--Bank-->
	<BankAmountLimit>1</BankAmountLimit>
	<BankTaxRate>0</BankTaxRate>
	<!--Polls-->
	<LordPollEnabled>true</LordPollEnabled>
	<LordPollRequiredGold>100000</LordPollRequiredGold>
	<LordPollTimeOut>60</LordPollTimeOut>
	<WarDeclareTimeOut>30</WarDeclareTimeOut>
	<PeaceDeclareTimeOut>30</PeaceDeclareTimeOut>
	<!--Health-->
	<RequiredMedicineSkillForHealing>50</RequiredMedicineSkillForHealing>
	<MedicineHealingAmount>15</MedicineHealingAmount>
	<WoundingEnabled>true</WoundingEnabled>
	<WoundingTimeMinutes>720</WoundingTimeMinutes>
	<!--Hunger-->
	<HungerInterval>300</HungerInterval>
	<HungerReduceAmount>1</HungerReduceAmount>
	<HungerRefillHealthLowerBoundary>0</HungerRefillHealthLowerBoundary>
	<HungerHealingAmount>1</HungerHealingAmount>
	<HungerHealingReduceAmount>0</HungerHealingReduceAmount>
	<HungerStartHealingUnderHealthPct>100</HungerStartHealingUnderHealthPct>
	<!--Items-->
	<MedicineItemId>pe_doctorscalpel</MedicineItemId>
	<LockpickItem>pe_lockpick</LockpickItem>
	<PickpocketingItem>pe_stealing_dagger</PickpocketingItem>
	<PoisonItemId>pe_poison_dagger</PoisonItemId>
	<AntidoteItemId>pe_antidote</AntidoteItemId>
	<!--Factions-->
	<CommonerCulture>empire</CommonerCulture>
	<OutlawCulture>empire</OutlawCulture>
	<CommonerBanner>17.1.148.1836.1836.765.786.1.0.-30.100.0.143.-295.425.764.764.1.0.0</CommonerBanner>
	<OutlawBanner>24.193.116.1536.1536.768.768.1.0.0</OutlawBanner>
	<CommonerName>Commoners</CommonerName>
	<OutlawName>Outlaws</OutlawName>
	<CanUseDiplomacy>false</CanUseDiplomacy>
	<MaxBannerLengt>100</MaxBannerLengt>
	<WarDeclareTimeOut>30</WarDeclareTimeOut>
	<PeaceDeclareTimeOut>30</PeaceDeclareTimeOut>
	<MaxBannerLength>250</MaxBannerLength>
	<!--Commands-->
	<MessagePrefix>/</MessagePrefix>
	<DefaultMessageColor>#FFFDFDFD</DefaultMessageColor>
	<AnnounceEnabled>true</AnnounceEnabled>
	<AnnounceColor>#FF0800FF</AnnounceColor>
	<CommandsEnabled>true</CommandsEnabled>
	<CommandsColor>#FFFDFDFD</CommandsColor>
	<DiceEnabled>true</DiceEnabled>
	<DiceColor>#FFFF6666</DiceColor>
	<DisableEnabled>true</DisableEnabled>
	<DisableColor>#FFFF6666</DisableColor>
	<DiceDistance>30</DiceDistance>
	<DiceBubble>true</DiceBubble>
	<DoEnabled>true</DoEnabled>
	<DoColor>#FFFF6666</DoColor>
	<DoDistance>30</DoDistance>
	<DoBubble>true</DoBubble>
	<HelpEnabled>true</HelpEnabled>
	<HelpColor>#FFFF6666</HelpColor>
	<MeEnabled>true</MeEnabled>
	<MeColor>#FFFF6666</MeColor>
	<MeDistance>30</MeDistance>
	<MeBubble>true</MeBubble>
	<MuteEnabled>true</MuteEnabled>
	<MuteColor>#FFFF6666</MuteColor>
	<NameEnabled>true</NameEnabled>
	<NameColor>#FFFF6666</NameColor>
	<RollEnabled>true</RollEnabled>
	<RollColor>#FFFF6666</RollColor>
	<RollDistance>30</RollDistance>
	<RollBubble>true</RollBubble>
	<VMuteEnabled>true</VMuteEnabled>
	<VmuteColor>#FFFF6666</VmuteColor>
	<WoundsEnabled>true</WoundsEnabled>
	<WoundsColor>#FFFF6666</WoundsColor>
	<WoundsDistance>0</WoundsDistance>
	<WoundsBubble>false</WoundsBubble>
	<TeleportToPositionColor>#FFFF6666</TeleportToPositionColor>
	<TeleportToPositionEnabled>true</TeleportToPositionEnabled>
	<!--Discord-->
	<DiscordAdminMessageEnabled>false</DiscordAdminMessageEnabled>
	<DiscordAdminMessageUrl></DiscordAdminMessageUrl>
	<DiscordAnnounceEnabled>false</DiscordAnnounceEnabled>
	<DiscordAnnounceUrl></DiscordAnnounceUrl>
	<DiscordServeStatusEnabled>false</DiscordServeStatusEnabled>
	<DiscordServeStatusUrl></DiscordServeStatusUrl>
	<DiscordExceptionEnabled>false</DiscordExceptionEnabled>
	<DiscordExceptionUrl></DiscordExceptionUrl>
	<DiscordLogEnabled>false</DiscordLogEnabled>
	<DiscordLogUrl></DiscordLogUrl>
</GeneralConfig>
```

## API

Persistent Empires features an API 

With it, you can submit GET/POST requests in order to remotely perform functions on the server.
This can be extremely powerful if your intention is to create moderation tools such as through an external panel or a discord bot. 

You can find the configuration file under `YourServerLocation/Modules/PersistentEmpires/ModuleData/Configs/ApiConfig.xml`

You will need to generate a secretkey and use said secretkey to generate a JWT token. DANGER - DO NOT USE THE EXAMPLE KEY, generate your own.

ApiConfig.xml
```xml
<ApiConfig>
	<Port>3169</Port>
	<SecretKey>2b6f1a8c3e74d0f5e92a7bdcf5013c1e9f5aeb6a74066eb45d03f11a5b7486d13721b53223e9f4d87c66b32b6e3b5d4ff8c951b1b05619d1e2c2616c1f8d39ba</SecretKey>
</ApiConfig>
```

Here are the following things you can do using the api.

```
POST /compensateplayer - Gives a player gold.
{
    "PlayerId": "player_id_here",
    "Gold": 100
}

POST /kickplayer - Kicks player from the server.
{
    "PlayerId": "player_id_here"
}

POST /fadeplayer - Kills a player and deletes their armor.
{
    "PlayerId": "player_id_here"
}

POST /unbanplayer - Unbans a player.
{
    "PlayerId": "player_id_here",
    "UnbanReason": "Reason for unbanning"
}

POST /banplayer - Bans a player.
{
    "PlayerId": "player_id_here",
    "BanEndsAt": "2024-05-01T00:00:00",
    "BanReason": "Reason for banning"
}

POST /announce - Posts an announcement into the server for all players to see.
{
    "Message": "Your announcement message here"
}

GET /servercap - Returns player count.

GET /restart - Issues Restart.

GET /shutdown - Shuts down the server.
```
