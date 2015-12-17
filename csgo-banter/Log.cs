using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csgo_banter
{
    public static class Log
    {
        public static void WriteLine(string Text, ConsoleColor ?Color = null)
        {
            ConsoleColor OldColor = Console.ForegroundColor;
            if (Color != null)
                Console.ForegroundColor = (ConsoleColor) Color;

            Console.WriteLine("[{0}] - {1}", DateTime.Now.ToLongTimeString(), Text);

            Console.ForegroundColor = OldColor;
        }
    }
}
