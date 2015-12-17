using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CSGOGameObserverSDK;
using CSGOGameObserverSDK.GameDataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace csgo_banter
{
    class Program
    {
        static List<Keys> KeyBinds;

        static int KillCount = 0;
        static int HSCount = 0;
        static int DeathCount = 0;

        static bool FirstRun = true;

        static string CurrentPlayer = "";

        static DateTime LastUpdate = DateTime.Now;

        static bool tilter = true;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs,
           [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
           int cbSize);

        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;

        const uint MAPVK_VK_TO_VSC = 0x0;

        static void Main(string[] args)
        {
            FillBants();

            new Thread(() =>
            {
                while (true)
                {
                    TimeSpan t = DateTime.Now - LastUpdate;

                    Console.Title = "Last Update " + t.TotalSeconds.ToString("F2") + " seconds ago";

                    Thread.Sleep(30);
                }
            }).Start();

            CSGOGameObserverServer csgoGameObserverServer = new CSGOGameObserverServer("http://127.0.0.1:3000/");
            csgoGameObserverServer.receivedCSGOServerMessage += OnReceivedCsgoServerMessage;
            csgoGameObserverServer.Start();

            Log.WriteLine("Server started ...");

            while (true)
            {
                Console.Read();
                Thread.Sleep(200);
            }
        }

        private static void OnReceivedCsgoServerMessage(object sender, JObject gameData)
        {
            CSGOGameState state = new CSGOGameState(gameData);

            bool bantsdone = false;

            LastUpdate = DateTime.Now;

            Random R = new Random();

            if (state != null)
            {
                if ((state.Player != null) && (state.Player.State != null))
                {
                    if (!state.Player.Name.Equals(CurrentPlayer))
                    {
                        Log.WriteLine("Player switched to: " + state.Player.Name + " (" + state.Player.State.RoundKillHs.ToString() + "/" + state.Player.State.RoundKills.ToString() + ")", ConsoleColor.Yellow);
                        CurrentPlayer = state.Player.Name;
                    }

                    if (state.Player.Steamid == state.Provider.Steamid)
                    {
                        Keys RandomKey = KeyBinds[R.Next(KeyBinds.Count)];

                        if (tilter)
                        {
                            if (state.Player.MatchStats.Deaths.Value != DeathCount)
                            {
                                DeathCount = state.Player.MatchStats.Deaths.Value;

                                if (DeathCount > 0)
                                {
                                    Log.WriteLine("RIP again ... (" + RandomKey.ToString() + ")");
                                    Thread.Sleep(R.Next(1000, 2000));
                                    SendKey(RandomKey);
                                }
                            }
                        }
                        else
                        {
                            if (state.Player.State.RoundKillHs != null)
                            {
                                if (state.Player.State.RoundKillHs != HSCount)
                                {
                                    HSCount = (int)state.Player.State.RoundKillHs;
                                    KillCount = (int)state.Player.State.RoundKills;

                                    if (KillCount == 0) FirstRun = true;

                                    if (!FirstRun)
                                    {
                                        Log.WriteLine("Nice Headshot! (" + RandomKey.ToString() + ")");

                                        Thread.Sleep(R.Next(1000, 2000));
                                        SendKey(RandomKey);
                                    }

                                    bantsdone = true;
                                }
                            }
                            if (state.Player.State.RoundKills != null)
                            {
                                if (state.Player.State.RoundKills != KillCount)
                                {
                                    KillCount = (int)state.Player.State.RoundKills;

                                    if (KillCount == 0) FirstRun = true;

                                    if (!FirstRun && !bantsdone)
                                    {
                                        Log.WriteLine("Great Kill! (" + RandomKey.ToString() + ")");

                                        Thread.Sleep(R.Next(1000, 2000));
                                        SendKey(RandomKey);
                                    }

                                    bantsdone = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (state.Player != null)
                    {
                        if (state.Player.Activity.Equals("menu"))
                        {
                            Log.WriteLine("Player in menu (Ping packet)", ConsoleColor.Cyan);
                        }
                        else
                        {
                            Log.WriteLine("??? :D", ConsoleColor.Cyan);
                        }
                    }
                }
            }
            else
            {
                Log.WriteLine("null?", ConsoleColor.Gray);
            }

            FirstRun = false;
        }

        public static void SendKey(Keys key)
        {

            byte code = (byte)MapVirtualKey((uint)key, MAPVK_VK_TO_VSC);

            keybd_event((byte)key, code, 0, 0);
            Thread.Sleep(10);
            keybd_event((byte)key, code, KEYEVENTF_KEYUP, 0);

            
        }


        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        private static void FillBants()
        {
            KeyBinds = new List<Keys>();
            KeyBinds.Add(Keys.NumPad0);
            KeyBinds.Add(Keys.NumPad1);
            KeyBinds.Add(Keys.NumPad2);
            KeyBinds.Add(Keys.NumPad3);
            KeyBinds.Add(Keys.NumPad4);
            KeyBinds.Add(Keys.NumPad5);
            KeyBinds.Add(Keys.NumPad6);
            KeyBinds.Add(Keys.NumPad7);
        }
    }
}
