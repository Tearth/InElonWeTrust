<h1 align="center" style="font-weight: bold">In Elon We Trust, In Thrust We Trust</h1>
<p align="center">
<img src="https://i.imgur.com/cYPoKXr.jpg" alt="SpaceXLogo">
</p>

<p align="center">
<a href="https://discordbots.org/bot/462742130016780337" >
<img src="https://discordbots.org/api/widget/status/462742130016780337.svg" alt="InElonWeTrust" />
<img src="https://discordbots.org/api/widget/servers/462742130016780337.svg?noavatar=true" alt="InElonWeTrust" />
<img src="https://discordbots.org/api/widget/upvotes/462742130016780337.svg?noavatar=true" alt="InElonWeTrust" />
<img src="https://discordbots.org/api/widget/lib/462742130016780337.svg?noavatar=true" alt="InElonWeTrust" />
</a>
</p>


Discord bot providing a lot of funny (or not) commands related with SpaceX and Elon Musk. Written at the beginning only to collect information about the upcoming start, it developed into a full-fledged bot providing a lot of information about SpaceX and Elon Musk. 

Application is written in C# and uses [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) as Discord client and [Oddity](https://github.com/Tearth/Oddity) as [SpaceX API](https://github.com/r-spacex/SpaceX-API) wrapper.

You can find this bot on [discordbots.org](https://discordbots.org/bot/462742130016780337).

**Main features**:
  * previous, upcoming and more specialized list of launches (missions with failed landings or to specified orbit? No problem!)
  * counter for the next launch
  * random stuff like SpaceX or Elon's tweet, photo or Reddit topic 
  * notifications (you will be notified about all new tweets, Flickr photos, hottest topics on /r/spacex and upcoming launches)
  * other fun stuff: Elon's quotes, company data and history, random videos, launchpads, rockets and more

<h1 align="center" style="font-weight: bold">
	<a href="https://discordapp.com/oauth2/authorize?client_id=462742130016780337&permissions=27712&scope=bot">Invitation link</a>
</h1>

<p align="center" style="font-style: italic">
Requires Manage Messages permission to do pagination properly. It will work without it but it won't be that fun.
</p>

# Prefixes
There is a few methods to call bot command (space between prefix and command is allowed):
  * e!command
  * elon!command

# Commands
### Launches
| Command | Description | Required permissions |
|---|---|---|
| e!AllLaunches  | get list of all launches (past and upcoming) | None |
| e!FailedLaunches  | get list of launches which landings were unsuccesfull | None |
| e!FailedStarts  | get list of launches where rocket did rapid unscheduled disassembly | None |
| e!GetLaunch [FlightNumber]  | get information about the launch with the specified flight number (which can be obtained by e!AllLaunches or similar command) | None |
| e!LatestLaunch  | get information about the latest launch | None |
| e!LaunchesWithOrbit [OrbitType] | get list of launches with the specified target orbit (type e!help LaunchesWithOrbit to get list of them) | None |
| e!NextLaunch  | get information about the next lanuch | None |
| e!PastLaunches  | get list of past launches | None |
| e!RandomLaunch  | get information about the random launch | None |
| e!UpcomingLaunches  | get list of upcoming launches | None |

### Media
| Command | Description | Required permissions |
|---|---|---|
| e!RandomElonTweet  | get random tweet from Elon Musk's Twitter profile | None |
| e!RandomFlickrPhoto  | get random photo from SpaceX's Flickr profile | None |
| e!RandomRedditTopic  | get random topic from /r/spacex subreddit | None |
| e!RandomSpaceXTweet  | get random tweet from SpaceX's Twitter profile | None |

### Misc
| Command | Description | Required permissions |
|---|---|---|
| e!Changelog  | get bot changelog | None |
| e!CompanyHistory  | get list of the most imporatnt events for SpaceX | None |
| e!CompanyInfo  | get information about company | None |
| e!CoreInfo [CoreSerial] | get information about the specified core | None |
| e!Cores | get list of all cores | None |
| e!GetEvent [EventNumber] | get information about the event with the specified id (which can be obtained by e!CompanyHistory) | None |
| e!Launchpads  | get list of all launchpads used by SpaceX | None |
| e!Links  | get list useful links related with SpaceX | None |
| e!Ping  | pong | None |
| e!RandomElonQuote  | get random Elon Musk's quote | None |
| e!RandomVideo  | get random video related with SpaceX | None |
| e!Roadster  | get information about Roadster launched by Falcon Heavy | None |
| e!Rockets  | get list of all rockets used by SpaceX | None |
| e!Uptime  | how long am I working? | None |

### Notifications
| Command | Description | Required permissions |
|---|---|---|
| e!EnableAllNotifications  | enable all notifications at the specified channel | Manage Messages |
| e!DisableAllNotifications  | disable all notifications at the specified channel | Manage Messages |
| e!NotificationsStatus  | get information about subscriptions at the current channel | None |
| e!ToggleLaunches  | toggle launches subscription (when enabled, bot will post information about next launch) | Manage Messages |
| e!ToggleFlickr  | toggle Flickr subscription (when enabled, all newest photos from SpaceX Flickr profile will be posted at the specified channel) | Manage Messages |
| e!ToggleReddit  | toggle Reddit subscription (when enabled, the hottest topics from /r/spacex will be posted at the specified channel) | Manage Messages |
| e!ToggleElonTwitter  | toggle Elon Musk Twitter subscription (when enabled, all newest tweets from Elon Musk profile will be posted at the specified channel) | Manage Messages |
| e!ToggleSpaceXTwitter  | toggle SpaceX Twitter subscription (when enabled, all newest tweets from SpaceX profile will be posted at the specified channel) | Manage Messages |

# Examples
<p align="center">
<img src="https://i.imgur.com/WnTuvV8.png" alt="Example1">
<img src="https://i.imgur.com/NnOKjEa.png" alt="Example2">
<img src="https://i.imgur.com/o3oU66w.png" alt="Example3">
<img src="https://i.imgur.com/UMHKOjK.png" alt="Example4">
<img src="https://i.imgur.com/N0UGeTU.png" alt="Example5">
</p>