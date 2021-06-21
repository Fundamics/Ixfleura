
![Logo](Assets/Ixfleura-03.jpg)


Ixfleura (pronounced - 'eeks-fluh-ra') is a discord bot designed to help the Fundamics community. Ixfleura means 'night flower' or 'flower of the night'


## Features

- Tags
- Suggestions
- Moderation (Includes auto-mod)
- Campaigns

## Development

Ixfleura uses the following resources
- [Disqord](https://github.com/Quahu/Disqord)
- [PostgreSQL](https://www.postgresql.org/)

### Discord
All bots need a token to run. You can get one by creating an application [here](https://discord.com/developers/applications)

### Database
Ixfleura uses PostgreSQL for it's backend database. So make sure you have it installed and configured before running the bot.

### Configuration
To manage and store certain info, Ixfleura uses a `config.json` file. An example config file can be found [here](https://github.com/Fundamics/Ixfleura/blob/master/Ixfleura/config.example.json)

### Running Locally
After finishing the configuration steps. First update the database using
```
dotnet ef database update
```
or if you're using Visual Studio and the `Package Manager Console`
```
Update-Database
```


## Support

For support, you can join the [discord guild](https://discord.gg/uXhtH9ZbxN)

  

