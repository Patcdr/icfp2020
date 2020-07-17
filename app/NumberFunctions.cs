using System;
using System.Collections.Generic;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    // Please feel free to move these functions somewhere else!
    public class NumberFunctions
    {

        public class BitStream
        {
            private string bits;
            private int dex;

            public BitStream(string bits)
            {
                this.bits = bits;
                this.dex = 0;
            }

            public string Read(int len)
            {
                string ret = this.bits.Substring(this.dex, len);
                this.dex += len;
                return ret;
            }
        }

        public static Value RecDem(BitStream stream)
        {
            string type = stream.Read(2);
            switch (type)
            {
                case "00":
                    return Nil;
                case "11":
                    return new ConsIntermediate2(RecDem(stream), RecDem(stream));
                default:
                    long sign = type[0] == '0' ? 1L : -1L;
                    int size = 0;
                    while (stream.Read(1) == "1")
                        size += 1;
                    if (size == 0)
                        return new Number(0L * sign);
                    long ret = Convert.ToInt64(stream.Read(size * 4), 2);
                    return new Number(ret * sign);

            }
        }

        public static Value Dem(string input)
        {
            return RecDem(new BitStream(input));
        }

        public static string RecMod(Value val)
        {
            if (val == Nil)
                return "00";
            else if(val is Number)
            {
                long ret = val.AsNumber();
                if (ret == 0) return "010";

                string str = Convert.ToString(ret, 2);
                int l_mod = (str.Length % 4);
                if (l_mod > 0)
                {
                    str = str.PadLeft(str.Length + 4 - l_mod, '0');
                }
                return str;
            }
            else if (val is ConsIntermediate2)
            {
                return RecMod(val.Invoke(TrueVal)) + RecMod(val.Invoke(FalseVal));
            }
            throw new Exception("WTF is this shit");
        }

        public static string Mod(Value input)
        {
            return RecMod(input);
        }
    }
}
