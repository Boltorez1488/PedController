using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI.Elements;
using LemonUI.Menus;

namespace PedController.GUI {
    public class MenuElement : NativeMenu {
        public MenuElement(string title) : base(title) {
        }

        public MenuElement(string title, string subtitle) : base(title, subtitle) {
        }

        public MenuElement(string title, string subtitle, string description) : base(title, subtitle, description) {
        }

        public MenuElement(string title, string subtitle, string description, I2Dimensional banner) : base(title, subtitle, description, banner) {
        }

        public ItemElement AddElement(string title) {
            var item = new ItemElement(title);
            Add(item);
            return item;
        }
    }
}
