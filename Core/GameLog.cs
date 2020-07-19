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
            var last = state;
            var ticks = new List<GameState>();
            while (state != null) {
                ticks.Add(state);
                state = state.Prev;
            }
            ticks.Reverse();

            var sb = new StringBuilder();
            sb.Append("[\n");

            sb.Append("  1,\n"); // isSucceeded
            sb.Append("  0,\n"); // gameType
            sb.Append($"  {last.GameStateVal},\n"); // ApiGameStatus
            sb.Append($"  {last.CurrentTurn},\n"); // ticks

            sb.Append("  [[1, 0, 4], [0, 1, 3]]\n"); // players

            sb.Append("  [\n");
            sb.Append($"  [{last.PlanetRadius}, {last.PlanetSafeRadius}],\n"); // planet
            sb.Append("  [\n");

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
                        ).Replace("\n", "").Replace(",]", "]")
                    );
                }
                sb.Append(String.Join(",\n", tickout) + "\n");
            }
            sb.Append("  ]\n");
            sb.Append("  ]\n");

            sb.Append("]\n");
            return sb.ToString();
        }
    }
}
