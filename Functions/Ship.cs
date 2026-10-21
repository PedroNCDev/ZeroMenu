using System;
using HarmonyLib;
using UnityEngine;
using ZeroMenu.Functions;

namespace ZeroMenu.Functions
{
    public static class Ship
    {
        public static bool UnfixableLights = false;
        public static bool CallMeeting = false;
        public static bool CloseMeeting = false;
        public static bool AutoReportBodies = false;
        public static bool AutoOpenDoors = false;

        public static bool ReactorSab = false;
        public static bool OxygenSab = false;
        public static bool LightsSab = false;
        public static bool CommsSab = false;
        public static bool MushMixupSab = false;
        public static bool TriggerSpores = false;

        public static bool UnlockVents = false;
        public static bool KickVents = false;
        public static bool WalkInVents = false;

        private static bool _reactorActive;
        private static bool _oxygenActive;
        private static bool _commsActive;
        private static bool _lightsActive;
        private static bool _unfixableActive;

        public static void Update()
        {
            if (ShipStatus.Instance == null || PlayerControl.LocalPlayer == null) return;

            if (CallMeeting) 
            { 
                Utils.CallMeeting(); 
                CallMeeting = false; 
            }

            if (CloseMeeting) 
            { 
                Utils.CloseMeeting(); 
                CloseMeeting = false; 
            }

            if (KickVents)
            {
                foreach (var vent in ShipStatus.Instance.AllVents)
                    VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, vent.Id);
                KickVents = false;
            }

            if (TriggerSpores)
            {
                if (Utils.GetCurrentMapID() == 5) 
                {
                    var fungleStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
                    if (fungleStatus != null)
                    {
                        foreach (var mushroom in fungleStatus.sporeMushrooms.Values)
                            PlayerControl.LocalPlayer.CmdCheckSporeTrigger(mushroom);
                    }
                }
                TriggerSpores = false;
            }

            ProcessSabotages();
            ProcessAutoReport();
            ProcessVents();
        }

        private static void ProcessSabotages()
        {
            var shipStatus = ShipStatus.Instance;
            byte mapId = (byte)Utils.GetCurrentMapID();

            HandleReactor(shipStatus, mapId);
            HandleOxygen(shipStatus, mapId);
            HandleComms(shipStatus, mapId);
            HandleElectrical(shipStatus, mapId);
            HandleUnfixLights(shipStatus);
            HandleMushMix(shipStatus, mapId);
        }

        private static void ProcessAutoReport()
        {
            if (!AutoReportBodies) return;

            try
            {
                var bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
                foreach (var bodyObject in bodyObjects)
                {
                    var deadBody = bodyObject.GetComponent<DeadBody>();
                    if (deadBody != null && !deadBody.Reported)
                    {
                        deadBody.Reported = true;
                        PlayerControl.LocalPlayer.CmdReportDeadBody(GameData.Instance.GetPlayerById(deadBody.ParentId));
                    }
                }
            } catch { }
        }

        private static void ProcessVents()
        {
            try
            {
                if (UnlockVents && !PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(true);

                if (WalkInVents && PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.inVent = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            } catch { }
        }

        private static void HandleReactor(ShipStatus shipStatus, byte mapId)
        {
            if (mapId == 2)
            {
                var labSys = shipStatus.Systems[SystemTypes.Laboratory].Cast<ReactorSystemType>();
                if (ReactorSab != _reactorActive)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Laboratory, _reactorActive ? (byte)16 : (byte)128);
                    _reactorActive = ReactorSab;
                }
                ReactorSab = _reactorActive = labSys.IsActive;
            }
            else if (mapId == 4)
            {
                var heliSys = shipStatus.Systems[SystemTypes.HeliSabotage].Cast<HeliSabotageSystem>();
                if (ReactorSab != _reactorActive)
                {
                    if (_reactorActive)
                    {
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0);
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
                    }
                    else
                    {
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 128);
                    }
                    _reactorActive = ReactorSab;
                }
                ReactorSab = _reactorActive = heliSys.IsActive;
            }
            else
            {
                var reactorSys = shipStatus.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (ReactorSab != _reactorActive)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Reactor, _reactorActive ? (byte)16 : (byte)128);
                    _reactorActive = ReactorSab;
                }
                ReactorSab = _reactorActive = reactorSys.IsActive;
            }
        }

        private static void HandleOxygen(ShipStatus shipStatus, byte mapId)
        {
            if (mapId != 4 && mapId != 2 && mapId != 5)
            {
                var oxygenSys = shipStatus.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
                if (OxygenSab != _oxygenActive)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, _oxygenActive ? (byte)16 : (byte)128);
                    _oxygenActive = OxygenSab;
                }
                OxygenSab = _oxygenActive = oxygenSys.IsActive;
                return;
            }
            if (!OxygenSab) return;
            HudManager.Instance.Notifier.AddDisconnectMessage("Oxygen system not present on this map");
            OxygenSab = false;
        }

        private static void HandleComms(ShipStatus shipStatus, byte mapId)
        {
            if (mapId is 1 or 5)
            {
                var hqCommsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HqHudSystemType>();
                if (CommsSab != _commsActive)
                {
                    if (_commsActive)
                    {
                        shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                        shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else shipStatus.RpcUpdateSystem(SystemTypes.Comms, 128);
                    _commsActive = CommsSab;
                }
                CommsSab = _commsActive = hqCommsSys.IsActive;
            }
            else
            {
                var commsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (CommsSab != _commsActive)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, _commsActive ? (byte)16 : (byte)128);
                    _commsActive = CommsSab;
                }
                CommsSab = _commsActive = commsSys.IsActive;
            }
        }

        private static void HandleElectrical(ShipStatus shipStatus, byte mapId)
        {
            if (mapId != 5)
            {
                var elecSys = shipStatus.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                HandleUnfixLights(shipStatus);

                if (LightsSab != _lightsActive)
                {
                    if (_lightsActive)
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            var switchMask = 1 << (i & 0x1F);
                            if ((elecSys.ActualSwitches & switchMask) != (elecSys.ExpectedSwitches & switchMask))
                                shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                        }
                    }
                    else
                    {
                        UnfixableLights = false;
                        byte b = 4;
                        for (var i = 0; i < 5; i++)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) > 0.5f) b |= (byte)(1 << i);
                        }
                        shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b | 128));
                    }
                    _lightsActive = LightsSab;
                }
                LightsSab = _lightsActive = elecSys.IsActive && !_unfixableActive;
                return;
            }
            if (!LightsSab && !UnfixableLights) return;
            HudManager.Instance.Notifier.AddDisconnectMessage("Electrical system not present on this map");
            LightsSab = UnfixableLights = false;
        }

        private static void HandleUnfixLights(ShipStatus shipStatus)
        {
            if (UnfixableLights == _unfixableActive) return;
            if (!_unfixableActive) LightsSab = false;
            shipStatus.RpcUpdateSystem(SystemTypes.Electrical, 69);
            _unfixableActive = UnfixableLights;
        }

        private static void HandleMushMix(ShipStatus shipStatus, byte mapId)
        {
            if (!MushMixupSab) return;
            if (mapId == 5) shipStatus.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 1);
            else HudManager.Instance.Notifier.AddDisconnectMessage("Mushrooms not present on this map");
            MushMixupSab = false;
        }

        [HarmonyPatch(typeof(ShipStatus), "FixedUpdate")]
        public static class ShipStatus_FixedUpdate
        {
            public static void Postfix(ShipStatus __instance)
            {
                Update();
            }
        }
    }
}