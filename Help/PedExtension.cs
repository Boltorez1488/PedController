using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace PedController.Help {
    public class PedInfo {
        public string Name;

        public int GroupId;
        public RelationshipGroup Relationship;

        public bool IsAntiBlip;
    }

    public static class PedExtension {
        public static readonly Dictionary<ulong, PedInfo> Infos = new Dictionary<ulong, PedInfo>();

        public static bool HasInfo(this Ped ped) {
            return Infos.ContainsKey(ped.NativeValue);
        }

        public static PedInfo Info(this Ped ped) {
            if (Infos.ContainsKey(ped.NativeValue))
                return Infos[ped.NativeValue];
            var info = Infos[ped.NativeValue] = new PedInfo {
                Name = ((PedHash)ped.Model.Hash).ToString(),
                Relationship = ped.RelationshipGroup,
                GroupId = ped.GroupId(),

                IsAntiBlip = ped.AttachedBlip != null
            };
            return info;
        }

        public static int GroupId(this Ped ped) {
            return Function.Call<int>(Hash.GET_PED_GROUP_INDEX, ped);
        }

        public static bool IsMain(this Ped ped) {
            return Core.Manager.Player == ped;
        }

        public static void InfoClear(this Ped ped) {
            Infos.Remove(ped.NativeValue);
        }

        public static void Select(this Ped ped) {
            if (ped == Game.Player.Character)
                return;
            if (ped.IsDead)
                ped.Revive();
            var hash = (PedHash)ped.Model.GetHashCode();
            Function.Call(Hash.CHANGE_PLAYER_PED, Game.Player, ped, true, true);
            if (hash.ToString().IndexOf("cop", StringComparison.OrdinalIgnoreCase) != -1) {
                Game.Player.WantedLevel = 0;
                Game.MaxWantedLevel = 0;
            } else {
                Game.MaxWantedLevel = 5;
                Game.Player.WantedLevel = 0;
            }
            Notification.Show($"Вселение в {ped.Info().Name}");
            AttachManager(ped);
        }

        private static void AttachManager(Ped ped) {
            Core.Manager.Attach(ped);
        }

        public static void Revive(this Ped ped) {
            ped.IsCollisionEnabled = true;
            ped.Health = ped.MaxHealth;
            ped.Task.ClearAllImmediately();
            Function.Call(Hash.RESURRECT_PED, ped.Handle);
            ped.Task.PlayAnimation("get_up@directional@movement@from_knees@action", "getup_l_0");
        }

        public static void ClearTasks(this Ped ped) {
            ped.Task.ClearAll();
            Notification.Show($"{ped.Info().Name} задачи очищены");
        }

        public static void Wander(this Ped ped) {
            if (ped.IsInVehicle()) {
                ped.Task.CruiseWithVehicle(ped.CurrentVehicle, 100);
            } else {
                ped.Task.WanderAround();
            }
            Notification.Show($"{ped.Info().Name} перешёл в свободный режим");
        }

        public static void SafeDelete(this Ped ped) {
            if (ped.IsMain())
                return;
            ped.AttachedBlip?.Delete();
            ped.Delete();
            Infos.Remove(ped.NativeValue);
        }

        public static void ManagerSet(this Ped ped) {
            if (ped.AttachedBlip == null || ped.HasInfo() && ped.Info().IsAntiBlip)
                return;
            ped.AttachedBlip.Sprite = BlipSprite.Boost;
            ped.AttachedBlip.Color = BlipColor.White;
        }

        public static void ManagerReset(this Ped ped) {
            if (ped.AttachedBlip == null || ped.HasInfo() && ped.Info().IsAntiBlip)
                return;
            ped.AttachedBlip.Sprite = BlipSprite.Standard;
            ped.AttachedBlip.Color = BlipColor.White;
        }

        public static void ManagerDetach(this Ped ped) {
            if (ped.HasInfo() && !ped.Info().IsAntiBlip) {
                ped.AttachedBlip?.Delete();
            }
            ped.IsInvincible = false;
            ped.IsBulletProof = false;
            ped.IsCollisionProof = false;
            ped.IsExplosionProof = false;
            ped.IsFireProof = false;
            ped.IsMeleeProof = false;
        }

        public static void FollowTo(this Ped ped, Ped target) {
            ped.Task.FollowToOffsetFromEntity(target, Vector3.RelativeBack, ped.Speed);
            Notification.Show($"{ped.Info().Name} следует за {target.Info().Name}");
        }

        public static void ChangeGroup(this Ped ped, int groupId) {
            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, groupId);
        }

        public static void ChangeGroup(this Ped ped, Ped from) {
            ped.ChangeGroup(from.GroupId());
            Notification.Show($"{ped.Info().Name} привязан к {from.Info().Name}");
        }

        public static void ResetGroup(this Ped ped) {
            if (!ped.HasInfo())
                return;
            var info = ped.Info();
            if (info.GroupId != ped.GroupId()) {
                Function.Call(Hash.REMOVE_PED_FROM_GROUP, ped);
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, info.GroupId);
            }
            ped.RelationshipGroup = info.Relationship;
            Notification.Show($"{ped.Info().Name} сбросил группу");
        }

        public static void ResetGroupHard(this Ped ped) {
            ped.LeaveGroup();;
            Notification.Show($"{ped.Info().Name} вышел из группы");
        }

        public static void Guard(this Ped ped) {
            ped.Task.GuardCurrentPosition();
            Notification.Show($"{ped.Info().Name} охраняет точку");
        }

        public static void Shoot(this Ped ped, Ped target) {
            ped.AttackMod();
            if (ped.Weapons.HasWeapon(WeaponHash.Unarmed)) {
                ped.Task.FightAgainst(target);
                Notification.Show($"{ped.Info().Name} вступает в драку");
            } else {
                ped.Task.ShootAt(target);
                Notification.Show($"{ped.Info().Name} стреляет в цель");
            }
        }

        public static void Shoot(this Ped ped, Vector3 pos) {
            ped.AttackMod();
            ped.Task.ShootAt(pos);
            Notification.Show($"{ped.Info().Name} стреляет в точку");
        }

        public static void AttackMod(this Ped ped, int mod = 100) {
            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped, mod, true);
        }

        private static VehicleSeat FreeSeat(Vehicle veh) {
            foreach (VehicleSeat val in Enum.GetValues(typeof(VehicleSeat))) {
                if (veh.IsSeatFree(val))
                    return val;
            }
            return VehicleSeat.Any;
        }

        public static void EnterVehicle(this Ped ped, Vehicle veh, bool isForce = false) {
            if (ped.IsInVehicle()) {
                ped.Task.ShuffleToNextVehicleSeat(ped.CurrentVehicle);
                Notification.Show($"{ped.Info().Name} пересаживается");
            } else if (veh != null) {
                if (isForce) {
                    ped.Task.EnterVehicle(veh, VehicleSeat.Driver);
                    return;
                }
                ped.Task.EnterVehicle(veh, FreeSeat(veh));
                Notification.Show($"{ped.Info().Name} залазит в машину");
            }
        }

        public static void EnterTargetVehicle(this Ped ped, bool isForce = false) {
            var veh = World.GetCrosshairCoordinates().HitEntity as Vehicle;
            ped.EnterVehicle(veh, isForce);
        }

        public static void LeaveVehicle(this Ped ped) {
            if (ped.IsInVehicle()) {
                ped.Task.LeaveVehicle();
                Notification.Show($"{ped.Info().Name} покидает машину");
            }
        }

        public static void GoToPos(this Ped ped, Vector3 pos, DrivingStyle style) {
            if (ped.IsInVehicle()) {
                //Function.Call(Hash.LOAD_ALL_PATH_NODES);
                ped.Task.ClearAll();
                ped.Task.DriveTo(ped.CurrentVehicle, pos, 5, 300, style);
                Notification.Show($"{ped.Info().Name} едет в точку в стиле {style}");
            } else {
                ped.Task.RunTo(pos);
                Notification.Show($"{ped.Info().Name} идёт в точку");
            }
        }

        public static void GoToTargetPoint(this Ped ped, DrivingStyle style) {
            ped.GoToPos(World.GetCrosshairCoordinates().HitPosition, style);
        }

        //public static void GoToCar(this Ped ped, Vehicle vehicle) {
        //    if (ped.IsInVehicle()) {
        //        ped.Task.ShuffleToNextVehicleSeat(ped.LastVehicle);
        //    } else {
        //        ped.Task.EnterVehicle(vehicle, VehicleSeat.Passenger);
        //    }
        //}

        public static void GoToNearestCar(this Ped ped) {
            var cars = World.GetNearbyVehicles(ped, 15);
            if (cars.Length == 0)
                return;
            ped.EnterVehicle(cars[0]);
        }

        public static void GoToWaypoint(this Ped ped, DrivingStyle style) {
            ped.GoToPos(World.WaypointPosition, style);
        }

        public static void GoToActiveBlip(this Ped ped, DrivingStyle style) {
            var blips = World.GetAllCheckpoints();
            if(blips.Length != 0)
                ped.GoToPos(blips.First().Position, style);
        }

        public static void TpTo(this Ped ped, Ped target) {
            ped.Position = target.Position;
            Notification.Show($"{ped.Info().Name} телепортирован к {target.Info().Name}");
        }
    }
}
