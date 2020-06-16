using System.Collections.Generic;
using System.Linq;

namespace BabySiimDiscordBot.Extensions
{
    /// <summary>
    /// Helper methods for lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>Chunks the input into <paramref name="chunkSize"/> sized sub-lists.</summary>
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
            =>
                source
                    .Select((x, i) => new {Index = i, Value = x})
                    .GroupBy(x => x.Index / chunkSize)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();
    }
}
