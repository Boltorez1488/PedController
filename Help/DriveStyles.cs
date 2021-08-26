using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace PedController.Help {
    public static class DriveStyles {
        public static Dictionary<DrivingStyle, string> Styles = new Dictionary<DrivingStyle, string> {
            {DrivingStyle.Rushed, "Псих"},
            {DrivingStyle.AvoidTraffic, "Анти-траффик"},
            {DrivingStyle.AvoidTrafficExtremely, "Анти-траффик (Экстрим)"},
            {DrivingStyle.IgnoreLights, "Анти-светофоры"},
            {DrivingStyle.Normal, "Нормально"},
            {DrivingStyle.SometimesOvertakeTraffic, "Иногда анти-траффик"},
        };

        public static Dictionary<string, DrivingStyle> Reverse = Build();
        static Dictionary<string, DrivingStyle> Build() {
            return Styles.ToDictionary(x => x.Value, x => x.Key);
        }
    }
}
