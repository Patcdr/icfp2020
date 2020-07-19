using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Core.Library;

namespace Core
{
    public class GalaxyProtocol : IProtocol
    {
        private readonly Dictionary<string, Node> env;

        public GalaxyProtocol()
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(@"..\..\..\..\galaxy.txt");
            }
            catch (FileNotFoundException)
            {
                try
                {
                    lines = File.ReadAllLines(@"../../../../galaxy.txt");
                }
                catch (FileNotFoundException)
                {
                    lines = File.ReadAllLines(@"../galaxy.txt");
                }
            }

            env = Parser.Parse(lines.ToList());
        }

        public IProtocol.Response call(Value state, Value point)
        {
            Value protocol = env["galaxy"].Evaluate(env);

            // ap ap galaxy <state> <point>
            Node root = new Apply(new Apply(protocol, state), point);
            Value result = root.Evaluate(env);

            // The result should look like (Flag, (NewState, Data))
            Value car = result.Invoke(TrueVal, env);
            Value cdr = result.Invoke(FalseVal, env);
            Value cadr = cdr.Invoke(TrueVal, env);
            Value cddr = cdr.Invoke(FalseVal, env);
            Value caddr = cddr.Invoke(TrueVal, env);

            return new IProtocol.Response
            {
                Flag = UtilityFunctions.EvaluateFully(car, env).AsNumber(),
                NewState = UtilityFunctions.EvaluateFully(cadr, env),
                Data = UtilityFunctions.EvaluateFully(caddr, env),
            };
        }
    }
}
