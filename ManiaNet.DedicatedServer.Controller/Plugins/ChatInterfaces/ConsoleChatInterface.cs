using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Chat;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ManiaNet.DedicatedServer.Controller.Plugins.ChatInterfaces
{
    internal class ConsoleChatInterface : IChatInterface
    {
        public void SendToAll(string message, IClient sender = null, object image = null)
        {
            if (image == null)
            {
                PrintColored(formatMessage(sender, message));
            }
            else if (image.GetType() == typeof(string))
            {
                switch ((string)image)
                {
                    case "info":
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;

                    case "warn":
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;

                    case "error":
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                }
                message = formatMessage(sender, message, "lfc", true);
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(formatMessage(sender, message, "lfc", true));
            }
        }

        public void SendToPlayer(string message, IClient recipient, IClient sender = null, object image = null)
        {
            SendToAll(message, sender, image);
        }

        private static string formatMessage(IClient sender, string message, string striplvl = "lf", bool addTime = false)
        {
            string msg = message;
            if (sender != null)
                msg = string.Format("[{0}] {1}",
                    sender.Nickname.Contains("$s") ? sender.Nickname.Remove(sender.Nickname.IndexOf("$s"), 2) : sender.Nickname,
                    message);

            msg = Tools.StripFormatting(msg, striplvl.ToCharArray());
            if (addTime)
                msg = String.Format("[{0} - {1}] {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), msg);
            return msg;
        }

        private static void PrintColored(string text)
        {
            Regex r = new Regex(@"\$[ownist]", RegexOptions.IgnoreCase);
            text = r.Replace(text, "");
            Regex colorregex = new Regex(@"[0-9a-f]", RegexOptions.IgnoreCase);
            Stack<ConsoleColor> formats = new Stack<ConsoleColor>();
            ConsoleColor defaultColor = ConsoleColor.Gray;
            formats.Push(defaultColor);
            int cursor = 0;
            char[] characters = text.ToCharArray();
            while (cursor < characters.Length - 1)
            {
                char current = characters[cursor];
                char next = characters[cursor + 1];
                if (current == '$')
                {
                    cursor++;
                    if (next == '<')
                    {
                        formats.Push(defaultColor);
                        cursor++;
                    }
                    else if (next == '>')
                    {
                        try
                        {
                            formats.Pop();
                        }
                        catch
                        {
                            if (formats.Count == 0)
                                formats.Push(defaultColor);
                        }
                        cursor++;
                    }
                    else if (next == 'z' || next == 'g')
                    {
                        try
                        {
                            formats.Pop();
                        }
                        finally
                        {
                            formats.Push(defaultColor);
                        }
                        cursor++;
                    }
                    else if (next == '$')
                    {
                        Console.ForegroundColor = formats.Peek();
                        Console.Write(characters[cursor]);
                        cursor += 1;
                    }
                    else if (colorregex.IsMatch(next.ToString()))
                    {
                        string color = text.Substring(cursor, 3);
                        try
                        {
                            formats.Pop();
                        }
                        finally
                        {
                            formats.Push(translateColor(color));
                        }
                        cursor += 3;
                    }
                }
                else if (current != '$' || (current == '$' && next == '$'))
                {
                    Console.ForegroundColor = formats.Peek();
                    Console.Write(characters[cursor]);
                    ++cursor;
                }
            }
            Console.Write(characters[characters.Length - 1] + "\n");
            Console.ResetColor();
        }

        private static ConsoleColor translateColor(string color)
        {
            string newColor = "";
            foreach (char c in color.ToCharArray())
            {
                int x;
                try
                {
                    x = int.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    x = 15;
                }
                if (x <= 3) x = 0;
                if (x > 3 && x <= 9) x = 5;
                if (x > 9) x = 15;
                newColor += x.ToString("X");
            }
            newColor = newColor.ToLower();
            if (newColor == "000") return ConsoleColor.DarkGray;
            if (newColor == "005") return ConsoleColor.DarkBlue;
            if (newColor == "00f") return ConsoleColor.Blue;
            if (newColor == "050") return ConsoleColor.DarkGreen;
            if (newColor == "055") return ConsoleColor.DarkCyan;
            if (newColor == "05f") return ConsoleColor.Blue;
            if (newColor == "0f0") return ConsoleColor.Green;
            if (newColor == "0f5") return ConsoleColor.Green;
            if (newColor == "0ff") return ConsoleColor.Cyan;
            if (newColor == "500") return ConsoleColor.DarkRed;
            if (newColor == "505") return ConsoleColor.DarkMagenta;
            if (newColor == "50f") return ConsoleColor.DarkMagenta;
            if (newColor == "550") return ConsoleColor.DarkYellow;
            if (newColor == "555") return ConsoleColor.Gray;
            if (newColor == "55f") return ConsoleColor.DarkBlue;
            if (newColor == "5f0") return ConsoleColor.Green;
            if (newColor == "5f5") return ConsoleColor.DarkGreen;
            if (newColor == "5ff") return ConsoleColor.Cyan;
            if (newColor == "f00") return ConsoleColor.Red;
            if (newColor == "f05") return ConsoleColor.Red;
            if (newColor == "f0f") return ConsoleColor.Magenta;
            if (newColor == "f50") return ConsoleColor.DarkRed;
            if (newColor == "f55") return ConsoleColor.DarkRed;
            if (newColor == "f5f") return ConsoleColor.Magenta;
            if (newColor == "ff0") return ConsoleColor.Yellow;
            if (newColor == "ff5") return ConsoleColor.DarkYellow;
            if (newColor == "fff") return ConsoleColor.White;
            return ConsoleColor.Gray;
        }
    }
}