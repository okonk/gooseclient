using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    public class ScriptsWindow : BaseWindow
    {
        private ScriptSlot[] slots;

        private int numberOfScripts = 0;

        public ScriptsWindow() : base("BlankMessage")
        {
            hideShortcutKey = GameClient.KeyMap.OpenScriptbook;

            titleLabel.Value = "Scripts";

            rows = 6;
            columns = 5;
            objW = 32;
            objH = 32;

            slots = new ScriptSlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int x = objoffX + c * objW;
                    int y = objoffY + r * objH;

                    var slot = new ScriptSlot(r * columns + c, x, y, objW, objH);
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }
        }

        public void AddScript(string name, int graphicId, Action<GuiElement> onUsed)
        {
            slots[numberOfScripts].SetScript(name, graphicId, onUsed);
            numberOfScripts++;
        }

        public override void OnCloseButtonClicked(Button b)
        {
            this.Parent.RemoveChild(this);
        }

        public override void SaveState()
        {
        }
    }
}