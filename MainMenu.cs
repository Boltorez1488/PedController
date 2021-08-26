using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using LemonUI.Menus;
using PedController.GUI;
using PedController.Help;

namespace PedController {
    public class MainMenu {
        public MenuElement Main = new MenuElement("Управление педами", "Мастер перевоплощения");
        public MenuElement Peds = new MenuElement("Педы", "Контроллируемые");

        public NativeListItem<string> Drive = new NativeListItem<string>("Стиль вождения", DriveStyles.Reverse.Keys.ToArray());

        public NativeListItem<WeaponHash> Weapons = new NativeListItem<WeaponHash>("Оружие");

        public PedManager Manager => Core.Manager;

        public MainMenu() {
            Drive.ItemChanged += (sender, args) => {
                Manager.DsStyle = DriveStyles.Reverse[args.Object];
            };
            Drive.SelectedIndex = 1;
            Main.Add(Drive);

            foreach (WeaponHash w in Enum.GetValues(typeof(WeaponHash))) {
                Weapons.Items.Add(w);
            }
            Weapons.Activated += (sender, args) => {
                foreach (var ped in Manager.Characters.Values) {
                    ped.Weapons.Give(Weapons.SelectedItem, 999999, true, true);
                }
            };
            Main.Add(Weapons);

            Main.AddElement("Сделать главным").On(() => {
                Manager.Current.Select();
                if (Manager.IsAttached(Manager.Current)) {
                    Manager.Detach(Manager.Current);
                }
                Manager.Player = Manager.Current;
            });

            Main.AddElement("Удалить").On(() => {
                Manager.Delete(Manager.Current);
            });
            Main.AddElement("Убить").On(() => {
                Manager.Current.Kill();
            });
            Main.AddElement("Остановить").On(() => {
                Manager.Current.ClearTasks();
            });
            Main.AddElement("Отцепить").On(() => {
                Manager.Detach(Manager.Current);
            });
            Main.AddElement("Отправить (Waypoint)").On(() => {
                Manager.Current.GoToWaypoint(Manager.DsStyle);
            });
            Main.AddElement("Отцепить всех").On(() => {
                Manager.DetachAll();
            });

            Main.UseMouse = false;
            Main.AddSubMenu(Peds);
            Peds.UseMouse = false;

            Core.Pool.Add(Main);
            Core.Pool.Add(Peds);
        }

        private readonly List<MenuElement> _pedsElements = new List<MenuElement>();

        public void ReloadMenu() {
            foreach (var menu in _pedsElements) {
                Core.Pool.Remove(menu);
            }
            _pedsElements.Clear();
            Peds.Clear();
            foreach (var c in Manager.Characters) {
                var m = AddPedMenu(c.Value);
                _pedsElements.Add(m);
                Core.Pool.Add(m);
            }
            Core.Pool.RefreshAll();
        }

        public void RebuildMenu() {
            foreach (var menu in _pedsElements) {
                Core.Pool.Remove(menu);
            }
            Peds.Clear();
            foreach (var el in _pedsElements) {
                Peds.AddSubMenu(el);
                Core.Pool.Add(el);
            }
            Core.Pool.RefreshAll();
        }

        public MenuElement AddPedMenu(Ped ped) {
            var name = ped.Info().Name;
            var menu = new MenuElement(name, name) {UseMouse = false};
            Peds.AddSubMenu(menu);

            menu.AddElement("Переименовать").On(() => {
                var text = Game.GetUserInput();
                if (string.IsNullOrEmpty(text))
                    return;
                menu.Title.Text = text;
                menu.Subtitle = text;
                ped.Info().Name = text;
                Core.Main.NeedRebuild = true;
            });
            menu.Add(new NativeSeparatorItem());

            menu.AddElement("Выбрать").On(ped.Select);
            menu.AddElement("Воскресить").On(ped.Revive);
            menu.AddElement("Убить").On(ped.Kill);
            menu.AddElement("Удалить").On(() => {
                menu.Back();
                Manager.Delete(ped);
            });
            menu.AddElement("Отправиться (Waypoint)").On(() => {
                ped.GoToWaypoint(Core.Manager.DsStyle);
            });
            menu.AddElement("Очистить задачи").On(ped.ClearTasks);
            menu.AddElement("Следуй за мной").On(() => {
                ped.ChangeGroup(Game.Player.Character);
            });
            menu.AddElement("Отвали").On(ped.ResetGroup);
            menu.AddElement("Отцепить").On(() => {
                menu.Back();
                Core.Manager.Detach(ped);
            });

            return menu;
        }
    }
}
