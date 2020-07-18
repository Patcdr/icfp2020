using System;
using System.Collections.Generic;
using System.Text;
using static Core.Library;

namespace Core
{
    public class StatelessDrawProtocol : IProtocol
    {
        private const string PROTOCOL = "statelessdraw = ap ap c ap ap b b ap ap b ap b ap cons 0 ap ap c ap ap b b cons ap ap c cons nil ap ap c ap ap b cons ap ap c cons nil nil";

        public IProtocol.Response call(Value state, Value point)
        {
            Dictionary<string, Node> env = Parser.Parse(new List<string> { PROTOCOL });
            Value protocol = env["statelessdraw"].Evaluate(env);

            // ap ap statelessdraw <state> <point>
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
