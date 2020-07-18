using System;
using System.Collections.Generic;
using System.Text;
using static Core.Library;

namespace Core
{
    public class StatefulDrawProtocol : IProtocol
    {
        private const string PROTOCOL = "statefuldraw = ap ap b ap b ap ap s ap ap b ap b ap cons 0 ap ap c ap ap b b cons ap ap c cons nil ap ap c cons nil ap c cons";

        public IProtocol.Response call(Value state, Value point)
        {
            Dictionary<string, Node> env = Parser.Parse(new List<string> { PROTOCOL });
            Value protocol = env["statefuldraw"].Evaluate(env);

            // ap ap statefuldraw <state> <point>
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
