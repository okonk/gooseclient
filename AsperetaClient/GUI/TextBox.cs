using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    // TODO: Right now this is only a basic textbox. Still need to support:
    // * Scrolling text in textbox when text is too large
    // * Enter event callback
    // * arrow keys for moving the cursor
    // * clicking in textbox to change cursor position
    // * Selecting text and associated functions (copy/overwrite/delete)
    // * Esc to lose focus
    // * Somehow disable 'SDL_StartTextInput' if no textboxes have focus (no clue if it actually matters though)

    class TextBox : GuiElement
    {
        public char PasswordMask { get; set; }

        public string Value { get; set; }

        public int CursorPosition { get; set; }

        private double cursorFlashTime = 0;
        private bool cursorVisible = true;

        public TextBox(int x, int y, int w, int h, SDL.SDL_Color backgroundColour, SDL.SDL_Color foregroundColour) : base(x, y, w, h, backgroundColour, foregroundColour)
        {
            this.Value = "";
            this.CursorPosition = 0;
            this.Padding = 5;
        }

        public override void Update(double dt)
        {
            cursorFlashTime += dt;

            if (cursorFlashTime >= 1)
            {
                cursorFlashTime -= 1;
                cursorVisible = !cursorVisible;
            }
        }

        public override void Render(double dt)
        {
            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, BackgroundColour.r, BackgroundColour.g, BackgroundColour.b, BackgroundColour.a);
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref Rect);

            int centeredY = (H - GameClient.FontRenderer.CharHeight) / 2;

            string renderText = Value;
            if (PasswordMask != 0)
                renderText = new string(PasswordMask, Value.Length);

            GameClient.FontRenderer.RenderText(renderText, this.X + this.Padding, this.Y + centeredY, ForegroundColour.r, ForegroundColour.g, ForegroundColour.b, ForegroundColour.a, this.X + this.W - this.Padding * 2);

            if (this.HasFocus && cursorVisible)
            {
                int cursorX = this.X + this.Padding + this.CursorPosition * GameClient.FontRenderer.CharWidth;

                SDL.SDL_SetRenderDrawColor(GameClient.Renderer, ForegroundColour.r, ForegroundColour.g, ForegroundColour.b, ForegroundColour.a);
                SDL.SDL_RenderDrawLine(GameClient.Renderer, 
                    cursorX, 
                    this.Y + centeredY - 1, 
                    cursorX, 
                    this.Y + centeredY + GameClient.FontRenderer.CharHeight);
            }
        }

        public override bool HandleEvent(SDL.SDL_Event ev)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (ev.button.x >= this.X && ev.button.x <= this.X + this.W &&
                        ev.button.y >= this.Y && ev.button.y <= this.Y + this.H)
                    {
                        this.HasFocus = true;
                        SDL.SDL_StartTextInput();
                    }
                    else
                    {
                        this.HasFocus = false;
                        // TODO: Would be nice to be able to end text input if no other textboxes have focus
                    }
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (!this.HasFocus) break;

                    // Handle backspace
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_BACKSPACE && this.CursorPosition > 0)
                    {
                        string newValue = this.Value.Substring(0, this.CursorPosition - 1);
                        if (this.CursorPosition < this.Value.Length)
                            newValue += this.Value.Substring(this.CursorPosition + 1);

                        this.Value = newValue;

                        this.CursorPosition--;
                    }
                    // Handle copy
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_c && (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != SDL.SDL_Keymod.KMOD_NONE)
                    {
                        SDL.SDL_SetClipboardText(this.Value);
                    }
                    // Handle paste
                    else if(ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_v && (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != SDL.SDL_Keymod.KMOD_NONE)
                    {
                        var pastedText = SDL.SDL_GetClipboardText();
                        this.Value = this.Value.Substring(0, this.CursorPosition) + pastedText + this.Value.Substring(this.CursorPosition);
                        this.CursorPosition += pastedText.Length;
                    }
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    if (!this.HasFocus) break;

                    byte[] rawBytes = new byte[SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE];

                    unsafe { Marshal.Copy((IntPtr)ev.text.text, rawBytes, 0, SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE); }

                    int length = Array.IndexOf(rawBytes, (byte)0);
                    string text = System.Text.Encoding.UTF8.GetString(rawBytes, 0, length);

                    // Not copy or pasting
                    if(!((SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != SDL.SDL_Keymod.KMOD_NONE && (text[0] == 'c' || text[0] == 'C' || text[0] == 'v' || text[0] == 'V')))
                    {
                        // Append character
                        this.Value = this.Value.Substring(0, this.CursorPosition) + text + this.Value.Substring(this.CursorPosition);
                        this.CursorPosition += text.Length;
                    }
                    break;
            }

            return false;
        }
    }
}
