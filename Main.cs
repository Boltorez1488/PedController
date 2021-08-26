using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.UI;
using LemonUI;
using LemonUI.Menus;
using PedController.Help;

//-----------------------------------------------------------------------------------//
// Y - Select Target
// Shift + Y - Set Target At Main
// T - Select Player
// O - Select Main Target
// Shift + O - Select Last Target
// Ctrl + O - Choose Last Target At Main
// [ - Set/Select Previous Ped
// ] - Set/Select Next Ped
// Shift + [ - Set/Select First Ped
// Shift + ] - Set/Select Last Ped
// Alt + [ - Set Previous Ped
// Alt + ] - Set Next Ped
// X - Select Target And Wander Current Ped
// P - Revive Target
// L - Set BodyGuard Target
// U - Stop Target
// N - Detach Target
// Ctrl + B - Stop & Leave Car Target
// G - Guard
//
//#####{ Current Ped }#####
// Z - Go To Waypoint And Select Player
// Alt + Z - Go To Waypoint
// E - Enter To Car / Change Seat
// Shift + P - Revive
// Shift + U - Stop
// Ctrl + N - Detach
// Ctrl + Y - Set Main
// Ctrl + G - Guard
// 
//#####{ Main Ped }#####
// I - Go To Vector3/Vehicle Target
// Shift + I - Go To Driver At Car
// K - Attack Target (Ped/Crosshair)
// B - Clear Tasks
// Shift + B - Stop & Leave Car
// Shift + X - Wander
// Shift + L - Set BodyGuard
// Ctrl + Shift + L - Set No BodyGuard
// Shift + E - Enter To Current Ped Car / Change Seat
// Shift + N - Detach
// Shift + G - Guard
// 
//-----------------------------------------------------------------------------------//

namespace PedController {
    public class Main : Script {
        public MainMenu Menu;
        public PedManager Manager => Core.Manager;

        public Main() {
            Core.Main = this;
            Core.Pool = new ObjectPool();
            Core.Manager = new PedManager(this);
            Menu = new MainMenu();
            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private static DateTime _ts = DateTime.Now;

        public bool NeedRebuild;
        public bool NeedReload;

        private void OnTick(object sender, EventArgs e) {
            if (NeedRebuild) {
                Menu.RebuildMenu();
                NeedRebuild = false;
            }
            if (NeedReload) {
                Menu.ReloadMenu();
                NeedReload = false;
            }

            Core.Pool.Process();

            if ((DateTime.Now - _ts).TotalSeconds < 10) {
                return;
            }

            // Clear temp peds
            var remover = new List<ulong>();
            foreach (var info in PedExtension.Infos) {
                if (info.Key != Manager.Player.NativeValue && !Manager.Characters.ContainsKey(info.Key)) {
                    remover.Add(info.Key);
                }
            }
            foreach (var r in remover) {
                PedExtension.Infos.Remove(r);
            }

            _ts = DateTime.Now;
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            Ped target;
            if (e.Modifiers == (Keys.Control | Keys.Shift)) {
                switch (e.KeyCode) {
                    case Keys.L:
                        Manager.Target?.ResetGroupHard();
                        break;
                    case Keys.T:
                        if (Manager.SelectedPed != null) {
                            Manager.Player.TpTo(Manager.SelectedPed);
                        }
                        break;
                }
                e.Handled = true;
                return;
            }

            if (e.Modifiers == Keys.Shift) {
                switch (e.KeyCode) {
                    case Keys.Y:
                        Manager.SetMainPedTarget();
                        break;
                    case Keys.G:
                        if (Manager.Current.IsInVehicle())
                            Manager.SelectedPed?.EnterVehicle(Manager.Current.CurrentVehicle, true);
                        break;
                    case Keys.I:
                        if (Manager.Current.IsInVehicle())
                            Manager.SelectedPed?.EnterVehicle(Manager.Current.CurrentVehicle);
                        break;
                    case Keys.B:
                        Manager.Target?.ClearTasks();
                        break;
                    case Keys.OemOpenBrackets:
                        Manager.Index--;
                        Notification.Show($"Выбран {Manager.SelectedPed.Info().Name}");
                        break;
                    case Keys.OemCloseBrackets:
                        Manager.Index++;
                        Notification.Show($"Выбран {Manager.SelectedPed.Info().Name}");
                        break;
                }
                e.Handled = true;
                return;
            }
            if (e.Modifiers == Keys.Control) {
                switch (e.KeyCode) {
                    case Keys.B:
                        Manager.SelectedPed?.ClearTasks();
                        break;
                    case Keys.I:
                        if (Manager.Current.IsInVehicle())
                            Manager.Target?.EnterVehicle(Manager.Current.CurrentVehicle);
                        break;
                    case Keys.G:
                        if (Manager.Current.IsInVehicle())
                            Manager.Target?.EnterVehicle(Manager.Current.CurrentVehicle, true);
                        break;
                }
                e.Handled = true;
                return;
            }
            switch (e.KeyCode) {
                case Keys.F5:
                    Menu.Main.Visible = !Menu.Main.Visible;
                    break;
                case Keys.Y:
                    Manager.Target?.Select();
                    break;
                case Keys.T:
                    Manager.SelectPlayer();
                    break;
                case Keys.O:
                    Manager.SelectMainPed();
                    break;
                case Keys.OemOpenBrackets:
                    Manager.SelectPrev();
                    break;
                case Keys.OemCloseBrackets:
                    Manager.SelectNext();
                    break;
                case Keys.X:
                    if (Manager.Current.IsMain())
                        break;
                    Manager.Current.Wander();
                    Manager.SelectPlayer();
                    break;
                case Keys.P:
                    Manager.Target?.Revive();
                    break;
                case Keys.L:
                    Manager.Target?.ChangeGroup(Game.Player.Character);
                    break;
                case Keys.N:
                    target = Manager.Target;
                    if (target == null)
                        return;
                    if (Manager.IsAttached(target)) {
                        Manager.Detach(target);
                    } else {
                        Manager.Attach(target);
                    }
                    break;
                case Keys.Z:
                    Manager.Current.GoToWaypoint(Manager.DsStyle);
                    Manager.SelectPlayer();
                    break;
                case Keys.B:
                    Manager.Current.EnterTargetVehicle();
                    break;
                case Keys.I:
                    var selected = Manager.SelectedPed;
                    if (selected != null) {
                        var veh = World.GetCrosshairCoordinates().HitEntity as Vehicle;
                        if (veh != null) {
                            selected.EnterVehicle(veh);
                            break;
                        }
                        target = Manager.Target;
                        if (target != null) {
                            selected.FollowTo(target);
                        } else {
                            selected.GoToTargetPoint(Manager.DsStyle);
                        }
                    }
                    break;
                case Keys.K:
                    Manager.ShootAtTarget();
                    break;
                case Keys.U:
                    Manager.Current.ClearTasks();
                    break;
                case Keys.G:
                    Manager.Target?.Guard();
                    break;
            }
        }
    }
}
