using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BabySiimDiscordBot.DbContexts;
using BabySiimDiscordBot.Models;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace BabySiimDiscordBot.Modules
{
    /// <inheritdoc />
    [Group("math")]
    [Alias("m")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Dictionary<string, double> _fredyDict = new Dictionary<string, double>();
        private readonly DiscordBotDbContext _discordBotDbContext;

        /// <summary>Setup for the math module.</summary>
        public MathModule()
        {
            _discordBotDbContext = new DiscordBotDbContext();
        }

        /// <summary>Define a new variable to be used in other math commands.</summary>
        [Command("fconst")]
        [Summary("Define a new variable to be used in other math commands.")]
        public Task FredyDefine(string variable, string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            {
                _fredyDict[variable] = d;

                var discordBotDbContext = _discordBotDbContext;
                discordBotDbContext.FredyConstants.Add(new FredyConstant {Name = variable, Value = d});
                discordBotDbContext.SaveChanges();
            }

            return Task.CompletedTask;
        }

        /// <summary>Prints the list of currently defined variables.</summary>
        [Command("env")]
        [Summary("Prints the list of currently defined variables.")]
        public async Task PrintEnv()
        {
            var strings = _discordBotDbContext.FredyConstants
                .ToList()
                .Select(f => $" - {f.Name} = {f.Value}");

            var msgs = string.Join(Environment.NewLine, strings);

            await Context.Channel.SendMessageAsync($"Currently defined environment:{Environment.NewLine}{msgs}");
        }

        /// <summary>Squares a number.</summary>
        [Command("square")]
        [Alias("sqr", "s")]
        [Summary("Squares a number.")]
        public async Task SquareAsync([Summary("The number to square.")] string num)
        {
            var cachedDbEntry = await _discordBotDbContext.FredyConstants.FindAsync(num);
            if (int.TryParse(num, out var parsedNum))
            {
                // We can also access the channel from the Command Context.
                await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(parsedNum, 2)}");
            }
            else if (num == "kristjan")
            {
                // We can also access the channel from the Command Context.
                await Context.Channel.SendMessageAsync(@"```javax.servlet.ServletException: Something bad happened
    at com.example.myproject.OpenSessionInViewFilter.doFilter(OpenSessionInViewFilter.java:60)
    at org.mortbay.jetty.servlet.ServletHandler$CachedChain.doFilter(ServletHandler.java:1157)
    at com.example.myproject.ExceptionHandlerFilter.doFilter(ExceptionHandlerFilter.java:28)
    at org.mortbay.jetty.servlet.ServletHandler$CachedChain.doFilter(ServletHandler.java:1157)
    at com.example.myproject.OutputBufferFilter.doFilter(OutputBufferFilter.java:33)
    at org.mortbay.jetty.servlet.ServletHandler$CachedChain.doFilter(ServletHandler.java:1157)
    at org.mortbay.jetty.servlet.ServletHandler.handle(ServletHandler.java:388)
    at org.mortbay.jetty.security.SecurityHandler.handle(SecurityHandler.java:216)
    at org.mortbay.jetty.servlet.SessionHandler.handle(SessionHandler.java:182)
    at org.mortbay.jetty.handler.ContextHandler.handle(ContextHandler.java:765)
    at org.mortbay.jetty.webapp.WebAppContext.handle(WebAppContext.java:418)
    at org.mortbay.jetty.handler.HandlerWrapper.handle(HandlerWrapper.java:152)
    at org.mortbay.jetty.Server.handle(Server.java:326)
    at org.mortbay.jetty.HttpConnection.handleRequest(HttpConnection.java:542)
    at org.mortbay.jetty.HttpConnection$RequestHandler.content(HttpConnection.java:943)
    at org.mortbay.jetty.HttpParser.parseNext(HttpParser.java:756)
    at org.mortbay.jetty.HttpParser.parseAvailable(HttpParser.java:218)
    at org.mortbay.jetty.HttpConnection.handle(HttpConnection.java:404)
    at org.mortbay.jetty.bio.SocketConnector$Connection.run(SocketConnector.java:228)
    at org.mortbay.thread.QueuedThreadPool$PoolThread.run(QueuedThreadPool.java:582)```");
            }
            else if (cachedDbEntry != null)
            {
                await Context.Channel.SendMessageAsync($"{num}^2 = {cachedDbEntry.Value * cachedDbEntry.Value}");
            }
            else
            {
                try
                {
                    foreach (var variable in _discordBotDbContext.FredyConstants)
                    {
                        num = num.Replace(variable.Name, variable.Value.ToString(CultureInfo.InvariantCulture));
                    }

                    var result = await CSharpScript.EvaluateAsync<double>(num, ScriptOptions.Default, _fredyDict);
                    await Context.Channel.SendMessageAsync($"({num})^2 = {Math.Pow(result, 2)}");
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync($"Argument '{num}' cannot be parsed :( ({e.Message})");
                }
            }
        }
    }
}
