using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace BabySiimDiscordBot.Modules
{
    [Group("math")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Dictionary<string, double> FredyDict = new Dictionary<string, double>();
        
        [Command("fconst")]
        public Task FredyDefine(string variable, string value)
        {
            FredyDict[variable] = double.Parse(value, CultureInfo.InvariantCulture);

            return Task.CompletedTask;
        }

        [Command("env")]
        public async Task PrintEnv()
        {
            var strings = FredyDict.Select(pair => $" - {pair.Key} = {pair.Value}");

            var msgs = string.Join(Environment.NewLine, strings);
            
            await Context.Channel.SendMessageAsync($"Currently defined environment:" + Environment.NewLine + msgs);
        }
        
        [Command("crash")]
        public Task Crash()
        {
            throw new Exception();
        }
        
        
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync([Summary("The number to square.")] string num)
        {
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
            else if (FredyDict.TryGetValue(num, out var val))
            {
                
                await Context.Channel.SendMessageAsync($"{num}^2 = {val * val}");
            }
            else
            {
                try
                {
                    foreach (var (key, value) in FredyDict)
                    {
                        num = num.Replace(key, value.ToString(CultureInfo.InvariantCulture));
                    }
                    
                    var result = await CSharpScript.EvaluateAsync<double>(num, ScriptOptions.Default, FredyDict);
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