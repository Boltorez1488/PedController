using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using LemonUI.Menus;
using PedController.GUI;
using PedController.Help;

namespace PedController {
    public class PedManager {
        private readonly Main _main;
        public Ped Player = Game.Player.Character;
        public Ped Current => Game.Player.Character;
        public Ped Target => PedSelector.GetTarget() ?? PedSelector.GetRayCastTarget();

        public Dictionary<ulong, Ped> Characters = new Dictionary<ulong, Ped>();

        private int _index;
        public int Index {
            get {
                if (_index < 0) {
                    _index = 0;
                } else if (_index > Characters.Count - 1) {
                    _index = Characters.Count - 1;
                }
                return _index;
            }
            set {
                if (_index < 0) {
                    _index = 0;
                } else if (_index > Characters.Count - 1) {
                    _index = Characters.Count - 1;
                } else {
                    _index = value;
                }

                var selected = SelectedPed;
                foreach (var ped in Characters.Values) {
                    if (ped == selected)
                        ped.ManagerSet();
                    else
                        ped.ManagerReset();
                }
            }
        }

        public Ped LastPed => Characters.Count == 0 ? null : Characters.Last().Value;
        public Ped SelectedPed {
            get {
                if (Characters.Count == 0)
                    return null;
                return Characters.ElementAt(Index).Value;
            }
            set {
                if (value == null)
                    return;
                if (!Characters.ContainsKey(value.NativeValue)) {
                    Characters[value.NativeValue] = value;
                }
                Index = Characters.Keys.ToList().IndexOf(value.NativeValue);
            }
        }

        public DrivingStyle DsStyle;

        public PedManager(Main main) {
            _main = main;
            Player.Info().IsAntiBlip = true;
        }

        public bool IsAttached(Ped ped) {
            return Characters.ContainsKey(ped.NativeValue);
        }

        public void Attach(Ped ped) {
            if (ped == null)
                return;
            if (ped.IsMain())
                return;
            if (IsAttached(ped))
                return;
            SelectedPed = ped;
            if (!ped.Info().IsAntiBlip)
                ped.AddBlip();
            ped.ManagerSet();
            _main.NeedReload = true;

            Notification.Show($"{ped.Info().Name} добавлен в реестр");
        }

        // Detach ped from manager
        public void Detach(Ped ped) {
            if (ped == null)
                return;
            if (ped.IsMain())
                return;
            if (ped == Game.Player.Character)
                SelectPlayer();
            Unmanage(ped);

            Notification.Show($"{ped.Info().Name} больше не в реестре");
        }

        private void Unmanage(Ped ped) {
            if (Characters.ContainsKey(ped.NativeValue)) {
                if (!ped.Info().IsAntiBlip) {
                    ped.AttachedBlip?.Delete();
                }
                var selected = SelectedPed;
                Characters.Remove(ped.NativeValue);
                Index--;
                if (ped != selected && SelectedPed != selected) {
                    Index = Characters.Values.ToList().IndexOf(selected);
                }
            }
            ped.ManagerDetach();
            _main.NeedReload = true;
        }

        public void DetachAll() {
            foreach (var p in Characters.Values.ToArray())
                Detach(p);
            _main.NeedReload = true;
        }

        public void Delete(Ped ped) {
            if (ped.IsMain())
                return;
            if (Game.Player.Character == ped) {
                SelectPlayer();
            }
            Characters.Remove(ped.NativeValue);
            if (Characters.Count > 0)
                Index--;
            _main.NeedReload = true;
            ped.SafeDelete();
        }

        public void SwitchPlayer(Ped ped) {
            if (ped.IsMain())
                return;
            if (IsAttached(ped))
                Detach(ped);
            Player = ped;
            Player.Info().IsAntiBlip = true;
        }

        public void SelectPlayer() {
            if (Current.IsMain())
                return;
            Player.Select();
        }

        public void SelectMainPed() {
            if (Characters.Count == 0)
                return;
            SelectedPed?.Select();
        }

        public void SelectLastPed() {
            if (Characters.Count == 0)
                return;
            LastPed?.Select();
        }

        public void SetFirstPed() {
            if (Characters.Count == 0)
                return;
            Index = 0;
            Characters.First().Value.Select();
        }

        public void SetLastPed() {
            if (Characters.Count == 0)
                return;
            Characters.Last().Value.Select();
            Index = Characters.Count - 1;
        }

        public void SetMainPedCurrent() {
            SelectedPed = Current;
        }

        public void SetMainPedTarget() {
            SelectedPed = Target;
        }

        public void SelectPrev() {
            if (Characters.Count == 0)
                return;
            Index--;
            SelectedPed?.Select();
        }

        public void SelectNext() {
            if (Characters.Count == 0)
                return;
            Index++;
            SelectedPed?.Select();
        }

        public void ShootAtTarget() {
            if (Characters.Count == 0)
                return;
            var selected = SelectedPed;
            selected.ClearTasks();
            var target = PedSelector.GetTarget() ?? PedSelector.GetRayCastTarget();
            if (target == null || target.IsMain()) {
                var pos = World.GetCrosshairCoordinates().HitPosition;
                selected?.Shoot(pos);
            } else {
                selected?.Shoot(target);
            }
        }

        public void SelectTarget(Ped target) {
            if (target == null)
                return;
            if (target.IsMain()) {
                SelectPlayer();
            } else {
                target.Select();
            }
        }

        public Ped GetNearestPed() {
            var peds = World.GetNearbyPeds(Current.Position, 100);
            return peds.Length == 0 ? null : peds[0];
        }
    }
}