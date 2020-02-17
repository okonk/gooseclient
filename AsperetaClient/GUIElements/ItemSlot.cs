using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class ItemSlot : GuiElement
    {
        public int SlotNumber { get; set; }

        Texture itemGraphic;
        Colour colour;
        int stackSize;

        int itemId;
        public string itemName;

        Tooltip tooltip;

        public event Action<GuiElement> DoubleClicked;

        public ItemSlot(int slotNumber, int x, int y, int w, int h) : base(x, y, w, h)
        {
            this.SlotNumber = slotNumber;
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (itemGraphic == null) return;

            itemGraphic.Render(X + xOffset, Y + yOffset, colour);

            if (stackSize > 1)
                GameClient.FontRenderer.RenderText(stackSize.ToString(), X + xOffset + 6, Y + yOffset + 2, Colour.White);
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

                            if (data is ItemSlot)
                            {
                                var fromSlot = data as ItemSlot;
                                // TODO: Dragging/dropping with character window sends "USE"
                                GameClient.NetworkClient.Change(fromSlot.SlotNumber, this.SlotNumber);
                            }

                            return true;
                        }
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (itemGraphic != null && ev.button.button == SDL.SDL_BUTTON_LEFT && Contains(xOffset, yOffset, ev.button.x, ev.button.y))
                    {
                        UiRoot.StartDragDrop(ev.button.x, ev.button.y, itemGraphic, this);

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
                    
                    if (contains && itemGraphic != null)
                    {
                        int x = ev.motion.x;
                        int y = ev.motion.y - GameClient.FontRenderer.CharHeight - 10;

                        if (tooltip == null)
                        {
                            tooltip = new Tooltip(x, y, Colour.Black, Colour.White, itemName);
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
            itemGraphic = null;
        }

        public void SetSlot(int itemId, string itemName, int stackSize, int graphicId, Colour colour)
        {
            this.itemId = itemId;
            this.itemName = itemName;
            this.stackSize = stackSize;
            this.itemGraphic = GameClient.ResourceManager.GetTexture(graphicId);
            this.colour = colour;
        }
    }
}
