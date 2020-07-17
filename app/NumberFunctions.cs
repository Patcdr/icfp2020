using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    // Please feel free to move these functions somewhere else!
    public class NumberFunctions
    {
        public static int Dem(string input)
        {
            // Special case to make parsing easier.
            if (input == "010")
            {
                return 0;
            }

            bool isPositive = input[0] == '0';
            int index = 2;
            while (input[index] == '1')
            {
                index++;
            }

            if (isPositive)
            {
                return Convert.ToInt32(input.Substring(index), 2);
            }
            else
            {
                return -1 * Convert.ToInt32(input.Substring(index), 2);
            }
        }

        public static string Mod(int input)
        {
            // Special case to make parsing easier
            if (input == 0)
            {
                return "010";
            }

            // Sign
            StringBuilder builder = new StringBuilder();
            if (input >= 0)
            {
                builder.Append("01");
            }
            else
            {
                builder.Append("10");
            }

            // Number of bits
            string binaryString = Convert.ToString(input, 2);
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
}
