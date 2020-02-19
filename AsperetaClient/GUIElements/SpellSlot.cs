using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class SpellSlot : GuiElement
    {
        public int SlotNumber { get; set; }

        public bool IsEmpty { get { return name == null; } }

        public bool Targetable { get { return targetable; } }

        private Texture graphic;

        public string name;

        private bool targetable;

        private Tooltip tooltip;

        public event Action<GuiElement> DoubleClicked;

        public SpellSlot(int slotNumber, int x, int y, int w, int h) : base(x, y, w, h)
        {
            this.SlotNumber = slotNumber;
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (graphic == null) return;

            graphic.Render(X + xOffset, Y + yOffset);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_USEREVENT:
                    if (ev.user.type == GameClient.DRAG_DROP_EVENT_ID)
                    {
                        int x = ev.user.code >> 16;
                        int y = ev.user.code & 0xFFFF;
                        if (Contains(xOffset, yOffset, x, y))
                        {
                            object data = UiRoot.GetDragDropEventData(ev);

                            HandleDrop(data);

                            return true;
                        }
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (graphic != null && ev.button.button == SDL.SDL_BUTTON_LEFT && Contains(xOffset, yOffset, ev.button.x, ev.button.y))
                    {
                        UiRoot.StartDragDrop(ev.button.x, ev.button.y, graphic, this);

                        return true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (!Contains(xOffset, yOffset, ev.button.x, ev.button.y))
                        return false;

                    if (ev.button.button == SDL.SDL_BUTTON_LEFT && ev.button.clicks == 2)
                    {
                        DoubleClicked?.Invoke(this);
                    }

                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    bool contains = Contains(xOffset, yOffset, ev.motion.x, ev.motion.y);
                    
                    if (contains && graphic != null)
                    {
                        int x = ev.motion.x;
                        int y = ev.motion.y - GameClient.FontRenderer.CharHeight - 10;

                        if (tooltip == null)
                        {
                            tooltip = new Tooltip(x, y, Colour.Black, Colour.White, name);
                            this.Parent.AddChild(tooltip);
                        }
                        else
                        {
                            tooltip.SetPosition(x, y);
                        }
                    }
                    else if (tooltip != null && !contains)
                    {
                        this.Parent.RemoveChild(tooltip);
                        tooltip = null;
                    }

                    break;
            }

            return false;
        }

        public void Clear()
        {
            graphic = null;
        }

        public void SetSlot(string name, int graphicId, bool targetable)
        {
            this.name = name;
            this.graphic = GameClient.ResourceManager.GetTexture(graphicId);
            this.targetable = targetable;
        }

        public void HandleDrop(object data)
        {

        }
    }
}
