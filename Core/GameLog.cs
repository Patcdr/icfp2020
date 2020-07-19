using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

namespace Core
{

    public class GameLog
    {

        public static string Write(GameState state)
        {
            if (state == null) return "";

            var last = state;
            var ticks = new List<GameState>();
            while (state != null) {
                ticks.Add(state);
                state = state.Prev;
            }
            ticks.Reverse();

            var sb = new StringBuilder();
            sb.Append("[");

            sb.Append("1, "); // isSucceeded
            sb.Append("0, "); // gameType
            sb.Append($"{last.GameStateVal}, "); // ApiGameStatus
            sb.Append($"{last.CurrentTurn}, "); // ticks

            sb.Append("[[1, 0, 4], [0, 1, 3]], "); // players

            sb.Append($"[[{last.StarSize}, {last.ArenaSize}], "); // planet
            sb.Append("[\n");
            {
                var tickout = new List<string>();
                for (var i = 0; i < ticks.Count; i++)
                {
                    tickout.Add((
                        "    [" + (i + 1) + ", " +
                        UtilityFunctions.PrettyPrint(
                            UtilityFunctions.Addr("cdddaddar", ticks[i].server_state),
                            true
                        ) +
                        "]"
                        ).Replace("\n", "").Replace(", ]", "]").Replace(')', ']').Replace('(','[')
                    );
                }
                sb.Append(String.Join(",\n", tickout) + "\n");
            }
            sb.Append("]]");

            sb.Append("]");
            return sb.ToString();
        }
    }
}
