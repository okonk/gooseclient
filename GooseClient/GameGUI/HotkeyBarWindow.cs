using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace GooseClient
{
    class HotkeyBarWindow : BaseWindow
    {
        private HotkeySlot[] slots;

        public HotkeyBarWindow() : base("HotButtons")
        {
            hideShortcutKey = SDL.SDL_Keycode.SDLK_F2;

            slots = new HotkeySlot[rows * columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int x = objoffX + c * objW;
                    int y = objoffY + r * objH;

                    var slot = new HotkeySlot(x, y, objW, objH);
                    this.AddChild(slot);

                    slots[r * columns + c] = slot;
                }
            }

            var inventoryWindow = UiRoot.Children.FirstOrDefault(c => c is InventoryWindow) as InventoryWindow;
            var characterWindow = UiRoot.Children.FirstOrDefault(c => c is CharacterWindow) as CharacterWindow;
            var spellbookWindow = UiRoot.Children.FirstOrDefault(c => c is SpellbookWindow) as SpellbookWindow;

            var hotButtons = GameClient.UserSettings["HotButtonLocations"];
            for (int i = 0; i <= 9; i++)
            {
                if (hotButtons.TryGetValue(i.ToString(), out string value))
                {
                    if (!int.TryParse(value, out int targetSlotNumber) || targetSlotNumber == 0)
                        continue;

                    int hotkeySlotNumber = (i + 9) % 10;
                    targetSlotNumber--; // Saved data is 1-indexed, client is 0-indexed
                    if (targetSlotNumber < inventoryWindow.slots.Length)
                        this.slots[hotkeySlotNumber].SetSlot(inventoryWindow.slots[targetSlotNumber]);
                    else if (targetSlotNumber < inventoryWindow.slots.Length + characterWindow.slots.Length + 1)
                        this.slots[hotkeySlotNumber].SetSlot(characterWindow.slots[targetSlotNumber - inventoryWindow.slots.Length - 1]);
                    else if (targetSlotNumber >= 100 && targetSlotNumber - 100 < spellbookWindow.slots.Length)
                        this.slots[hotkeySlotNumber].SetSlot(spellbookWindow.slots[targetSlotNumber - 100]);
                }
            }
        }

        public override void SaveState()
        {
            base.SaveState();

            var inventoryWindow = UiRoot.Children.FirstOrDefault(c => c is InventoryWindow) as InventoryWindow;

            var hotButtons = GameClient.UserSettings["HotButtonLocations"];
            for (int i = 0; i <= 9; i++)
            {
                int hotkeySlotNumber = (i + 9) % 10;
                var slot = this.slots[hotkeySlotNumber].LinkedSlot;
                if (slot == null)
                    hotButtons[i.ToString()] = "0";
                else if (slot is ItemSlot)
                    hotButtons[i.ToString()] = (slot.SlotNumber + 1).ToString();
                else if (slot is SpellSlot)
                    hotButtons[i.ToString()] = (slot.SlotNumber + 101).ToString();
            }
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            if (UiRoot.FocusedTextBox != null) return false;

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if ((SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_SHIFT) != SDL.SDL_Keymod.KMOD_NONE)
                        return false;

                    if (ev.key.keysym.sym >= SDL.SDL_Keycode.SDLK_0 && ev.key.keysym.sym <= SDL.SDL_Keycode.SDLK_9)
                    {
                        this.slots[(ev.key.keysym.sym - SDL.SDL_Keycode.SDLK_0 + 9) % 10].Use();
                        return true;
                    }
                    else if (ev.key.keysym.sym >= SDL.SDL_Keycode.SDLK_KP_1 && ev.key.keysym.sym <= SDL.SDL_Keycode.SDLK_KP_0)
                    {
                        this.slots[ev.key.keysym.sym - SDL.SDL_Keycode.SDLK_KP_1].Use();
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }
    }
}
