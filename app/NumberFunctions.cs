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

        public static string Mod(int[] input)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                builder.Append("11");
                builder.Append(Mod(input[i]));
            }
            builder.Append("00");
            return builder.ToString();
        }

        public static string Mod(int input)
        {
            // Special case to make parsing easier
            if (input == 0)
            {
                return "010";
            }

            StringBuilder builder = new StringBuilder();

            // Sign
            if (input >= 0)
            {
                builder.Append("01");
            }
            else
            {
                builder.Append("10");
            }

            // Number of bits
            string binaryString = Convert.ToString(Math.Abs(input), 2);
            int bitsNeeded = (binaryString.Length + 3) / 4;
            for (int i = 0; i < bitsNeeded; i++)
            {
                builder.Append("1");
            }
            builder.Append("0");

            // Actual number
            for (int i = 0; i < bitsNeeded * 4 - binaryString.Length; i++)
            {
                builder.Append("0");
            }

            builder.Append(binaryString);
            return builder.ToString();
        }
    }
}
