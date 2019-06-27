<h1 align="center" style="font-weight: bold">In Elon We Trust</h1>
<p align="center">
<img src="https://i.imgur.com/67oDn3w.jpg" alt="SpaceXLogo">
</p>

<p align="center">
<img src="https://discordbots.org/api/widget/status/462742130016780337.svg" alt="InElonWeTrust" />
<img src="https://discordbots.org/api/widget/servers/462742130016780337.svg?noavatar=true" alt="InElonWeTrust" />
<img src="https://discordbots.org/api/widget/lib/462742130016780337.svg?noavatar=true" alt="InElonWeTrust" />
</p>


Discord bot providing commands related to SpaceX and Elon Musk.

Application is written in C# and uses [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) as Discord client and [Oddity](https://github.com/Tearth/Oddity) as [SpaceX API](https://github.com/r-spacex/SpaceX-API) wrapper.

**Main features**:
  * information about the next launch
  * previous, upcoming and more specialized list of launches
  * notifications (you will be notified about all new tweets, Flickr photos, hottest topics on /r/spacex and upcoming launches)
  * other fun stuff: Elon's quotes, company data and history, random videos, launchpads, rockets and more

<h1 align="center" style="font-weight: bold">
	<a href="https://discordapp.com/oauth2/authorize?client_id=462742130016780337&permissions=27712&scope=bot">Invitation link</a>
</h1>

<p align="center" style="font-style: italic">
Requires Manage Messages permission to do pagination properly. It will work without (but won't be that fun).
</p>

# Prefixes
There are a few methods to call bot command (space between prefix and command is allowed):
  * e!command
  * elon!command

# Commands
### Launches
| Command | Description | Required permissions |
|---|---|---|
| __e!NextLaunch__  | get information about the next launch | none |
| e!AllLaunches  | get a list of all launches (past and upcoming) | none |
| e!FailedLaunches  | get a list of launches which landings were unsuccessful | none |
| e!GetLaunch [FlightNumber]  | get information about the launch with the specified flight number (which can be obtained by e!AllLaunches or similar command) | none |
| e!LatestLaunch  | get information about the latest launch | none |
| e!LaunchesWithOrbit [OrbitType] | get a list of launches with the specified target orbit (type e!help LaunchesWithOrbit to get a list of them) | none |
| e!PastLaunches  | get a list of past launches | none |
| e!RandomLaunch  | get information about the random launch | none |
| e!UpcomingLaunches  | get a list of upcoming launches | none |

### Media
| Command | Description | Required permissions |
|---|---|---|
| e!RandomElonTweet  | get a random tweet from Elon Musk's Twitter profile | none |
| e!RandomSpaceXTweet  | get a random tweet from SpaceX's Twitter profile | none |
| e!RandomSpaceXFleetTweet  | get a random tweet from SpaceXFleet's Twitter profile | none |
| e!RandomFlickrPhoto  | get a random photo from SpaceX's Flickr profile | none |
| e!RandomRedditTopic  | get a random topic from /r/spacex subreddit | none |

### Notifications
| Command | Description | Required permissions |
|---|---|---|
| e!EnableAllNotifications  | enable all notifications at the current channel | Manage Messages |
| e!DisableAllNotifications  | disable all notifications at the current channel | Manage Messages |
| e!NotificationsStatus  | get information about subscriptions at the current channel | none |
| e!ToggleLaunches  | toggle launches subscription (when enabled, the bot will post information about the next launch at the current channel) | Manage Messages |
| e!ToggleFlickr  | toggle Flickr subscription (when enabled, all newest photos from SpaceX Flickr profile will be posted at the current channel) | Manage Messages |
| e!ToggleReddit  | toggle Reddit subscription (when enabled, the hottest topics from /r/spacex will be posted at the current channel) | Manage Messages |
| e!ToggleElonTwitter  | toggle Elon Musk Twitter subscription (when enabled, all newest tweets from Elon Musk profile will be posted at the current channel) | Manage Messages |
| e!ToggleSpaceXTwitter  | toggle SpaceX Twitter subscription (when enabled, all newest tweets from SpaceX profile will be posted at the current channel) | Manage Messages |
| e!ToggleSpaceXFleetTwitter  | toggle SpaceXFleet Twitter subscription (when enabled, all newest tweets from SpaceXFleet profile will be posted at the current channel) | Manage Messages |

### Time zone
| Command | Description | Required permissions |
|---|---|---|
| e!SetTimeZone  | sets the specified time zone (local time will be displayed in the launch information) | Manage Messages |
| e!ResetTimeZone  | resets time zone (local time won't be shown again) | Manage Messages |

### Miscellaneous
| Command | Description | Required permissions |
|---|---|---|
| e!Avatar  | displays current Elon Musk's Twitter avatar | none |
| e!Changelog  | get bot changelog | none |
| e!CompanyHistory  | get a list of the most important events for SpaceX | none |
| e!CompanyInfo  | get information about company | none |
| e!CoreInfo [CoreSerial] | get information about the specified core | none |
| e!Cores | get a list of all cores | none |
| e!GetEvent [EventNumber] | get information about the event with the specified id (which can be obtained by e!CompanyHistory) | none |
| e!Launchpads  | get a list of all launchpads used by SpaceX | none |
| e!Links  | get a list useful links related with SpaceX | none |
| e!Ping  | pong | none |
| e!RandomElonQuote  | get random Elon Musk's quote | none |
| e!RandomVideo  | get random video related with SpaceX | none |
| e!Roadster  | get information about Roadster launched by Falcon Heavy | none |
| e!Rockets  | get a list of all rockets used by SpaceX | none |
| e!Uptime  | how long am I working? | none |

# Examples
<p align="center">
<img src="https://i.imgur.com/Ym2taqH.png" alt="Example1">
<img src="https://i.imgur.com/9HGxerU.png" alt="Example2">
<img src="https://i.imgur.com/5I9phQj.png" alt="Example3">
<img src="https://i.imgur.com/lmzNNuG.png" alt="Example4">
<img src="https://i.imgur.com/jZsXAZ3.png" alt="Example5">
</p>