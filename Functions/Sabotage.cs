using System.Collections.Generic;
using Hazel;
using Il2CppInterop.Runtime;
using InnerNet;
using UnityEngine;
using ZeroMenu.Utilities;

namespace ZeroMenu.Functions
{
    public static class Sabotage
    {
        public static bool UpdateSystemsDirectly = true;

        public static readonly Dictionary<string, SystemTypes> SkeldSabotages = new Dictionary<string, SystemTypes>()
        {
            { "Reactor", SystemTypes.Reactor }, { "Oxygen", SystemTypes.LifeSupp },
            { "Lights", SystemTypes.Electrical }, { "Communications", SystemTypes.Comms }
        };

        public static readonly Dictionary<string, SystemTypes> SkeldDoors = new Dictionary<string, SystemTypes>()
        {
            { "Cafeteria", SystemTypes.Cafeteria }, { "Storage", SystemTypes.Storage },
            { "Medbay", SystemTypes.MedBay }, { "Security", SystemTypes.Security },
            { "Upper Engine", SystemTypes.UpperEngine }, { "Lower Engine", SystemTypes.LowerEngine },
            { "Electrical", SystemTypes.Electrical }
        };

        public static readonly Dictionary<string, SystemTypes> MiraSabotages = new Dictionary<string, SystemTypes>()
        {
            { "Reactor", SystemTypes.Reactor }, { "Oxygen", SystemTypes.LifeSupp },
            { "Lights", SystemTypes.Electrical }, { "Communications", SystemTypes.Comms }
        };

        public static readonly Dictionary<string, SystemTypes> PolusSabotages = new Dictionary<string, SystemTypes>()
        {
            { "Reactor", SystemTypes.Laboratory }, { "Lights", SystemTypes.Electrical },
            { "Communications", SystemTypes.Comms }
        };

        public static readonly Dictionary<string, SystemTypes> PolusDoors = new Dictionary<string, SystemTypes>()
        {
            { "Office", SystemTypes.Office }, { "Communications", SystemTypes.Comms },
            { "Laboratory", SystemTypes.Laboratory }, { "Decontamination", SystemTypes.Decontamination },
            { "Electrical", SystemTypes.Electrical }, { "Oxygen", SystemTypes.LifeSupp },
            { "Weapons", SystemTypes.Weapons }, { "Storage", SystemTypes.Storage }
        };

        public static readonly Dictionary<string, SystemTypes> AirshipSabotages = new Dictionary<string, SystemTypes>()
        {
            { "Reactor", SystemTypes.HeliSabotage }, { "Lights", SystemTypes.Electrical },
            { "Communications", SystemTypes.Comms }
        };

        public static readonly Dictionary<string, SystemTypes> AirshipDoors = new Dictionary<string, SystemTypes>()
        {
            { "Brig", SystemTypes.Brig }, { "Records", SystemTypes.Records },
            { "Communications", SystemTypes.Comms }, { "Main Hall", SystemTypes.MainHall },
            { "Kitchen", SystemTypes.Kitchen }, { "Medical", SystemTypes.Medical },
            { "Lounge", SystemTypes.Lounge }
        };

        public static readonly Dictionary<string, SystemTypes> FungleSabotages = new Dictionary<string, SystemTypes>()
        {
            { "Reactor", SystemTypes.Reactor }, { "Communications", SystemTypes.Comms },
            { "Mushroom Mixup", SystemTypes.MushroomMixupSabotage }
        };

        public static readonly Dictionary<string, SystemTypes> FungleDoors = new Dictionary<string, SystemTypes>()
        {
            { "Storage", SystemTypes.Storage }, { "Kitchen", SystemTypes.Kitchen },
            { "Laboratory", SystemTypes.Laboratory }, { "Lookout", SystemTypes.Lookout },
            { "Mining Pit", SystemTypes.MiningPit }, { "Communications", SystemTypes.Comms },
            { "Reactor", SystemTypes.Reactor }
        };

        public static Dictionary<string, SystemTypes> GetSabotages()
        {
            MapNames map = (MapNames)Utils.GetCurrentMapID();
            return map switch
            {
                MapNames.Skeld or MapNames.Dleks => SkeldSabotages,
                MapNames.MiraHQ => MiraSabotages,
                MapNames.Polus => PolusSabotages,
                MapNames.Airship => AirshipSabotages,
                MapNames.Fungle => FungleSabotages,
                _ => SkeldSabotages,
            };
        }

        public static Dictionary<string, SystemTypes> GetDoors()
        {
            MapNames map = (MapNames)Utils.GetCurrentMapID();
            return map switch
            {
                MapNames.Skeld or MapNames.Dleks => SkeldDoors,
                MapNames.MiraHQ => new Dictionary<string, SystemTypes>(),
                MapNames.Polus => PolusDoors,
                MapNames.Airship => AirshipDoors,
                MapNames.Fungle => FungleDoors,
                _ => SkeldDoors,
            };
        }

        public static bool CanUnlockDoors()
        {
            MapNames map = (MapNames)Utils.GetCurrentMapID();
            return AmongUsClient.Instance.AmHost || map == MapNames.Polus || map == MapNames.Airship || map == MapNames.Fungle;
        }

        public static void SabotageSystem(SystemTypes system)
        {
            if (!UpdateSystemsDirectly)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)system);
                return;
            }

            BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);

            switch (system)
            {
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                case SystemTypes.HeliSabotage:
                case SystemTypes.LifeSupp:
                case SystemTypes.Comms:
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 128);
                    break;

                case SystemTypes.Electrical:
                    byte amount = 4;
                    for (byte i = 0; i < 5; i++)
                    {
                        if (BoolRange.Next(0.5f)) amount |= (byte)(1 << i);
                    }
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Electrical, (byte)(amount | 128));
                    break;

                case SystemTypes.MushroomMixupSabotage:
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, (byte)MushroomMixupSabotageSystem.Operation.TriggerSabotage);
                    break;
            }

            batch.FinishBatch();
        }

        public static void FixSabotage(SystemTypes system)
        {
            BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);

            switch (system)
            {
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                case SystemTypes.LifeSupp:
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16);
                    break;

                case SystemTypes.Comms:
                case SystemTypes.HeliSabotage:
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16 | 0);
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16 | 1);
                    break;

                case SystemTypes.Electrical:
                    SwitchSystem switches = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    int amount = switches.ActualSwitches ^ switches.ExpectedSwitches;
                    if (amount == 0) break;
                    batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Electrical, (byte)(amount | 128));
                    break;

                case SystemTypes.MushroomMixupSabotage:
                    if (!AmongUsClient.Instance.AmHost) break;
                    MushroomMixupSabotageSystem mixupSystem = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
                    if (!mixupSystem.IsActive) break;
                    mixupSystem.currentSecondsUntilHeal = 0.1f;
                    mixupSystem.IsDirty = true;
                    break;
            }

            batch.FinishBatch();
        }

        public static void LockDoor(SystemTypes door)
        {
            ShipStatus.Instance.RpcCloseDoorsOfType(door);
        }

        public static void UnlockDoor(SystemTypes system)
        {
            for (byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
            {
                OpenableDoor door = ShipStatus.Instance.AllDoors[i];
                if (door.Room != system) continue;
                UnlockDoor(door, i);
            }
        }

        public static void UnlockDoor(OpenableDoor door, int index = 0)
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64));
                return;
            }

            door.SetDoorway(true);

            Il2CppSystem.Type doorType = door.GetIl2CppType();
            if (doorType == Il2CppType.From(typeof(AutoOpenDoor)))
            {
                AutoDoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<AutoDoorsSystemType>();
                doorSystem.dirtyBits |= 1U << index;
            }
            else if (doorType == Il2CppType.From(typeof(PlainDoor)))
            {
                DoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<DoorsSystemType>();
                doorSystem.IsDirty = true;
            }
        }

        public static void SabotageAll()
        {
            foreach (SystemTypes system in GetSabotages().Values) SabotageSystem(system);
        }

        public static void FixAllSabotages()
        {
            foreach (SystemTypes system in GetSabotages().Values) FixSabotage(system);
        }

        public static void LockAll()
        {
            BatchedMessage batch = new BatchedMessage();
            foreach (SystemTypes door in GetDoors().Values) batch.QueueCloseDoors(door);
            batch.FinishBatch();
        }

        public static void UnlockAll()
        {
            for (byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
            {
                UnlockDoor(ShipStatus.Instance.AllDoors[i], i);
            }
        }
    }
}