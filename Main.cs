using MelonLoader;
using OpenRGB;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Bluscream;
using System.Reflection;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = OpenRGB.NET.Models.Color;
using Harmony;

namespace MelonLoaderMod
{
    public static class BuildInfo
    {
        public const string Name = "OpenRGB Mod"; // Name of the Mod.  (MUST BE SET)
        public const string Description = ""; // Description for the Mod.  (Set as null if none)
        public const string Author = "Bluscream"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class OpenRGBMod : MelonMod
    {
        private static OpenRGBMod instance;
        private const string ModCategory = "OpenRGB";
        private static OpenRGBClient client;
        private static List<Device> oldDevices;

        private IPAddress Ip
        {
            get
            {
                var ip = MelonPrefs.GetString(ModCategory, "ip");
                if (IPAddress.TryParse(ip, out var ipv4)) return ipv4;
                return IPAddress.Parse(Dns.GetHostEntry(ip).AddressList.First(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString());
            }
        }

        private int Port => MelonPrefs.GetInt(ModCategory, "port");
        private static float MinInterval => MelonPrefs.GetFloat(ModCategory, "min interval s");
        private static float MaxInterval => MelonPrefs.GetFloat(ModCategory, "max interval s");
        private bool isHunting => LevelController.instance.currentGhost.isHunting;
        private GhostAI.States lastState = GhostAI.States.favouriteRoom;
        private static bool isEnabled = false;
        private static bool isRunning = false;
        private object[] Timers = new object[] { };
        private static Color black = new Color(0, 0, 0);

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Log("OnApplicationStart");
            // instance = this;
            MelonPrefs.RegisterCategory(this.GetType().Name, BuildInfo.Name);
            MelonPrefs.RegisterString(ModCategory, "ip", "127.0.0.1", "IP Address or hostname of the OpenRGB server");
            MelonPrefs.RegisterInt(ModCategory, "port", 6742, "Port of the OpenRGB server");
            MelonPrefs.RegisterFloat(ModCategory, "min interval s", .5f, "Minimal time in seconds for turning off/on");
            MelonPrefs.RegisterFloat(ModCategory, "max interval s", 1f, "Maximal time in seconds for turning off/on");

            Connect();

            // var harmonyinf = new HarmonyLib.Harmony("PhasmophobiaOpenRGBMod");
            // harmonyinf.Patch(AccessTools.Method(typeof(GhostAI), "ChangeState"), null, new HarmonyMethod(typeof(OpenRGBMod), "ChangeStateEvent"));
        }
        // public void ChangeState(GhostAI.States state, PhotonObjectInteract obj = null, PhotonObjectInteract[] objects = null)
        /*public static void ChangeStateEvent(GhostAI.States __0, PhotonObjectInteract __1 = null, PhotonObjectInteract[] __2 = null, GhostAI __instance = null)
        {
            if (instance.lastState == __0) return;
            MelonLogger.Log($"New GhostAI State: {(GhostAI.States)__0}");
            switch (__0)
            {
                case GhostAI.States.hunting:
                    MelonLogger.Log("Starts hunting!");
                    // instance.DimLEDs();
                    break;

                default:
                    // instance.RestoreLEDs();
                    break;
            }
        }*/

        public override void OnUpdate()
        {
            if (!isEnabled) return;
            /* if (Input.GetKeyDown(KeyCode.F2))
             {
                 LevelController.instance.currentGhost.isHunting = true;
             }
             else if (Input.GetKeyDown(KeyCode.F3))
             {
                 LevelController.instance.currentGhost.isHunting = false;
             }*/
            if (isHunting && !isRunning)
            {
                isRunning = true;
                MelonLogger.Log("Is Hunting!");
                // DimLEDs();
                for (int i = 0; i < oldDevices.Count; i++)
                {
                    Timers.Add(MelonCoroutines.Start(FlickerLEDs(oldDevices[i], i)));
                }
            }
            else if (!isHunting && isRunning)
            {
                isRunning = false;
                MelonLogger.Log("No longer Hunting!");
                for (int i = 0; i < Timers.Length; i++)
                {
                    MelonCoroutines.Stop(Timers[i]);
                }
            }
            Timers = new object[] { };
            RestoreAll();
        }

        public override void OnLevelWasInitialized(int level) // Runs when a Scene has Initialized.
        {
            MelonLogger.Log($"OnLevelWasInitialized: {level}");
            if (level < 1) return;
            if (!client.Connected) client.Connect();
            if (!isEnabled && level > 1) isEnabled = true;
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Log("OnApplicationQuit");
            RestoreAll();
        }

        private bool Connect()
        {
            try
            {
                client = new OpenRGBClient(ip: Ip.ToString(), port: Port, name: "Phasmophobia OpenRGB Mod", timeout: 15000);
                if (!client.Connected) throw new Exception("Disconnected!");
            }
            catch (Exception ex)
            {
                MelonLogger.LogError($"Unable to connect to {Ip}:{Port} ({ex.Message})");
                return false;
            }
            oldDevices = client.GetAllControllerData().ToList();
            MelonLogger.Log($"Connected to {Ip}:{Port}");
            MelonLogger.Log($"Devices: {string.Join(", ", oldDevices.Select(d => d.Name))}");
            return true;
        }

        public static IEnumerator FlickerLEDs(Device device, int deviceId)
        {
            var min = MinInterval; var max = MaxInterval;
            while (isRunning)
            {
                DimLEDs(device, deviceId);
                yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
                RestoreLEDs(device, deviceId);
                yield return new WaitForSeconds(UnityEngine.Random.Range(min, max));
            }
        }

        private static void RestoreAll()
        {
            for (int i = 0; i < oldDevices.Count; i++)
            {
                RestoreLEDs(oldDevices[i], i);
            }
        }

        private static void RestoreLEDs(Device oldDevice, int deviceId)
        {
            client.UpdateLeds(deviceId, oldDevice.Colors);
        }

        private static void DimLEDs(Device device, int deviceId)
        {
            var leds = Enumerable.Repeat(black, device.Leds.Length).ToArray();
            client.UpdateLeds(deviceId, leds);
            /*var colors = oldDevices[i].Colors.Count
            var hsvs = oldDevices[i].Colors.Select(c => c.ToHsv());
            hsvs.ForEach(c => c.v = c.v - 5);
            for (int _i = 0; i < oldDevices[i].Colors.Length; _i++)
            {
                var color = oldDevices[i].Colors[_i];
                var hsv = color.ToHsv();

                leds.Add(Color.FromHsv(hsv.h, hsv.s, hsv.v)); // color.Dim(-1f)
            }*/
            // var leds = oldDevices[i].Leds.Select(_ => black).ToArray();
        }
    }
}