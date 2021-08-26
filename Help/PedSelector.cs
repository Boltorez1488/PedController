using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace PedController.Help {
    public static class PedSelector {
        public static Ped PlayerPed = Game.Player.Character;

        public static void TpTo(Ped ped) {
            PlayerPed.Position = ped.Position;
        }

        public static Ped GetTarget() {
            var target = Game.Player.TargetedEntity;
            if (target == null) {
                target = World.GetCrosshairCoordinates().HitEntity;
            } else if (!target.Model.IsPed) {
                target = World.GetCrosshairCoordinates().HitEntity;
            }
            if (target == null) {
                return null;
            }
            if (!target.Model.IsPed) {
                return null;
            }
            return target as Ped;
        }

        public static Ped GetRayCastTarget(float radius = 3) {
            var pos = World.GetCrosshairCoordinates().HitPosition;
            var target = World.GetNearbyPeds(pos, radius);
            return target.Length == 0 ? null : target[0];
        }
    }
}
