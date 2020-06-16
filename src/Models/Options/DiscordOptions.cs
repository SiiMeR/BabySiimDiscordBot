namespace BabySiimDiscordBot.Models.Options
{
    /// <summary>General options for the discord bot.</summary>
    public class DiscordOptions
    {
        /// <summary>Discord API token.</summary>
        public string AccessToken { get; set; }

        /// <summary>Prefix used to distinguish commands from normal messages.</summary>
        public char CommandPrefix { get; set; }
    }
}
