# BabySiimDiscordBot

A discord bot written using the Discord.net framework. 

The appsettings.json file is added to gitignore so you will need to add it yourself.

Sample appsettings.json:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "AccessToken": "<YOUR_TOKEN_HERE>"
}
```

Dependencies you need to download yourself and place in the project's root:
* fm.exe (ffmpeg.exe renamed to fm.exe)
* libsodium.dll
* opus.dll

For playing audio, you need to create a folder named 'audio' in the project's root and add the sounds there. 
