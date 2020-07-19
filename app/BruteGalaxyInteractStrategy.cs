using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;

namespace app
{
    class BruteGalaxyInteractStrategy : BaseInteractStrategy
    {
        private readonly IProtocol protocol;

        public BruteGalaxyInteractStrategy(Sender sender) : base(sender)
        {
            protocol = new GalaxyProtocol();
        }

        public override Value Start()
        {
            var points = new ConsIntermediate2[] {
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 0 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 1 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 2 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 3 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // multidraw
                new ConsIntermediate2(new Number(0), new Number(0)),  // nil ? ? ?
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 0,0
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 8,4
            };

            ConsoleDrawer.drawing = true;

            Interactor.Result result = null;
            for (int i = 0; i < points.Length; i++)
            {
                var next = result == null ? Nil : result.NewState;

                var point = points[i];
                Console.WriteLine(point.ToString());

                result = Interactor.Interact(protocol, next, point);
            }

            long last_x = -10000;
            long last_y = -100000;
            while (true)
            {
                Dictionary<long, int> y_count = new Dictionary<long, int>();
                Dictionary<long, int> x_count = new Dictionary<long, int>();

                foreach (Value point_list in UtilityFunctions.ListAsEnumerable(result.RawData, null))
                {
                    foreach (Value point in UtilityFunctions.ListAsEnumerable(point_list, null))
                    {
                        var x = point.Invoke(Library.TrueVal, null).AsNumber();
                        var y = point.Invoke(Library.FalseVal, null).AsNumber();
                        if (!y_count.ContainsKey(y)) y_count[y] = 0;
                        if (!x_count.ContainsKey(x)) x_count[x] = 0;
                        y_count[y]++;
                        x_count[x]++;
                    }
                }

                var max_y = y_count.OrderByDescending(i => i.Value).First().Key;
                var max_x = x_count.OrderByDescending(i => i.Value).First().Key;
                Console.WriteLine($"{max_x}, {max_y} {result.Flag}");
                if (max_x == last_x && max_y == last_y)
                {
                    ConsoleDrawer.drawing = true;
                    Brute(protocol, result.NewState);
                }
                result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(max_x), new Number(max_y)));
                if (max_x == last_x && max_y == last_y) break;
                last_x = max_x;
                last_y = max_y;
            }

            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));

            return null;
        }

        private void Brute(IProtocol protocol, Value state)
        {
            var drawing = ConsoleDrawer.drawing;
            ConsoleDrawer.drawing = false;

            var start = state.ToString();

            for (int y = -110; y < 110; y += 3)
            {
                for (int x = -110; x < 110; x += 3)
                {
                    var next = Interactor.Interact(protocol, state, new ConsIntermediate2(new Number(x), new Number(y)));
                    if (start != next.NewState.ToString())
                    {
                        Console.WriteLine($"\n    ({x}, {y}) {start} -> {next.NewState}");
                        ConsoleDrawer.drawing = true;
                        Interactor.Interact(protocol, state, new ConsIntermediate2(new Number(x), new Number(y)));
                        ConsoleDrawer.drawing = false;
                    }
                }
                Console.Write($"{y} ");
            }

            Console.WriteLine();

            ConsoleDrawer.drawing = drawing;
        }

        public override Value Next(GameState state)
        {
            return null;
        }
    }
}
