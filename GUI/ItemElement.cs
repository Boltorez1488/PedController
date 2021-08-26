using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI.Menus;

namespace PedController.GUI {
    public class ItemElement : NativeItem {
        public ItemElement(string title) : base(title) {
        }

        public ItemElement(string title, string description) : base(title, description) {
        }

        public ItemElement(string title, string description, string altTitle) : base(title, description, altTitle) {
        }

        public ItemElement On(Action action) {
            Activated += (sender, args) => {
                action();
            };
            return this;
        }
    }
}
