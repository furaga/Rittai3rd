using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace リッタイver3
{
    public enum BUTTON
    {
        A, B, X, Y, START, BACK, LB, RB, LTRIGGER, RTRIGGER, LEFT, UP, RIGHT, DOWN, SIZE, NONE
    };

    public enum DevType
    {
        KEYBOARD = 0, GAMEPAD1, GAMEPAD2, CPU, NONE
    };

    public class CPUInputState
    {
        public bool[] button = null;
        public Vector2 stick;

        public CPUInputState()
        {
            button = new bool[(int)BUTTON.SIZE];
            for (int i = 0; i < button.Length; i++)
            {
                button[i] = false;
            }
            stick = Vector2.Zero;
        }
    }

    public class InputManager
    {
        static readonly int MAX_KEY = 120;
        int cur = 0, pre = MAX_KEY;
        int connectn = 0;
        KeyboardState[] ks = new KeyboardState[MAX_KEY];
        GamePadState[] gs1 = new GamePadState[ MAX_KEY ];
        GamePadState[] gs2 = new GamePadState[ MAX_KEY ];
        GameTime[] times = new GameTime[ MAX_KEY ];
        CPUInputState[] cpu = new CPUInputState[MAX_KEY];

        public CPUInputState CurCPU
        {
            get
            {
                return cpu[cur];
            }
            set
            {
                cpu[cur] = value;
            }
        }
        public CPUInputState PrevCPU
        {
            get
            {
                return cpu[pre];
            }
        }
        
        struct KEY_BUTTON
        {
            public Keys key;
            public Buttons but;
            public KEY_BUTTON(Keys k, Buttons b = 0)
            {
                key = k; but = b;
            }
        }

        KEY_BUTTON[] kbinfo = new KEY_BUTTON[]{
             new KEY_BUTTON( Keys.Z, Buttons.A ),//A
             new KEY_BUTTON( Keys.X, Buttons.X ),//B
             new KEY_BUTTON( Keys.Space, Buttons.B ),//X
             new KEY_BUTTON( Keys.S, Buttons.Y ),//Y
             new KEY_BUTTON( Keys.Enter, Buttons.Start ),//START
             new KEY_BUTTON( Keys.Back, Buttons.Back ),//BACK
             new KEY_BUTTON( Keys.C, Buttons.LeftShoulder ),//LB
             new KEY_BUTTON( Keys.D, Buttons.RightShoulder ),//RB
             new KEY_BUTTON( Keys.V, Buttons.LeftTrigger ),//LTRIGGER
             new KEY_BUTTON( Keys.F, Buttons.RightTrigger ),//RTRIGGER//
             new KEY_BUTTON( Keys.Left),//LEFT
             new KEY_BUTTON( Keys.Up),//UP
             new KEY_BUTTON( Keys.Right), //RIGHT
             new KEY_BUTTON( Keys.Down), //DOWN
        };

        public InputManager()
        {         
            ks[ cur ] = Keyboard.GetState();
            gs1[ cur ] = GamePad.GetState(PlayerIndex.One);
            gs2[ cur ] = GamePad.GetState(PlayerIndex.Two);

            for (int i = 0; i < cpu.Length; i++)
            {
                cpu[i] = new CPUInputState();
            }

            if ( gs1[ cur ].IsConnected ) connectn++;
            if ( gs2[ cur ].IsConnected ) connectn++;
        }

        public void Update(GameTime gameTime)
        {
            pre = cur;
            cur = (cur + 1) % MAX_KEY;
            ks[cur] = Keyboard.GetState();
            gs1[cur] = GamePad.GetState(PlayerIndex.One);
            gs2[cur] = GamePad.GetState(PlayerIndex.Two);
            connectn = 0; 
            if (gs1[cur].IsConnected) connectn++;
            if (gs2[cur].IsConnected) connectn++;
            times[cur] = gameTime;
        }

        public int NumOfGamePad() { return connectn; }

        public bool isUp(BUTTON b, DevType[] dev = null)
        {
            if (dev == null) dev = all;
            return !isOn(cur, b, dev) && isOn(pre, b, dev);
        }

        public bool isDown(BUTTON b, DevType[] dev = null)
        {
            if (dev == null) dev = all;
            return isOn(cur, b, dev) && !isOn(pre, b, dev);
        }

        public bool isOn(BUTTON b, DevType[] dev = null)
        {
            if (dev == null) dev = all;
            return isOn(cur, b, dev);
        }
        
        DevType[] all = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2 };

        public bool isOn(int c, BUTTON b, DevType[] dev = null)
        {
            if (dev == null) dev = all;
            int n = (int)b;
            if ((int)BUTTON.LEFT <= (int)b && (int)b <= (int)BUTTON.DOWN) return isStick(c, 0.5f, b, dev);
            foreach (DevType d in dev)
            {
                switch (d)
                {
                    case DevType.KEYBOARD:
                        if (ks[c].IsKeyDown(kbinfo[n].key))
                            return true;
                        break;
                    case DevType.GAMEPAD1:
                        if (gs1[c].IsButtonDown(kbinfo[n].but)) 
                            return true; 
                        break;
                    case DevType.GAMEPAD2:
                        if (gs2[c].IsButtonDown(kbinfo[n].but))
                            return true;
                        break;
                    case DevType.CPU:
                        if (cpu[c].button[n])
                            return true;
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        public bool isStick(int c, float th, BUTTON b, DevType[] dev = null)
        {
            if (dev == null) dev = all;
            float stick;
            foreach (DevType d in dev)
            {
                switch (d)
                {
                    case DevType.KEYBOARD: 
                        if (ks[c].IsKeyDown(kbinfo[(int)b].key)) 
                            return true;
                        break;
                    case DevType.GAMEPAD1:
                        stick = 0.0f;
                        if (gs1[c].IsConnected)
                        {
                            switch (b)
                            {
                                case BUTTON.LEFT: stick = -gs1[c].ThumbSticks.Left.X; break;
                                case BUTTON.RIGHT: stick = gs1[c].ThumbSticks.Left.X; break;
                                case BUTTON.UP: stick = gs1[c].ThumbSticks.Left.Y; break;
                                case BUTTON.DOWN: stick = -gs1[c].ThumbSticks.Left.Y; break;
                            }
                            if (stick > th) return true;
                            switch (b)
                            {
                                case BUTTON.LEFT: if (gs1[c].IsButtonDown(Buttons.DPadLeft)) return true; break;
                                case BUTTON.RIGHT: if (gs1[c].IsButtonDown(Buttons.DPadRight)) return true; break;
                                case BUTTON.UP: if (gs1[c].IsButtonDown(Buttons.DPadUp)) return true; break;
                                case BUTTON.DOWN: if (gs1[c].IsButtonDown(Buttons.DPadDown)) return true; break;
                            }                           
                        }
                        break;
                    case DevType.GAMEPAD2:
                        stick = 0.0f;
                        if (gs2[c].IsConnected)
                        {
                            switch (b)
                            {
                                case BUTTON.LEFT: stick = -gs2[c].ThumbSticks.Left.X; break;
                                case BUTTON.RIGHT:  stick = gs2[c].ThumbSticks.Left.X; break;
                                case BUTTON.UP: stick = gs2[c].ThumbSticks.Left.Y; break;
                                case BUTTON.DOWN: stick = -gs2[c].ThumbSticks.Left.Y; break;
                            }
                            if (stick > th) return true;
                            switch (b)
                            {
                                case BUTTON.LEFT: if (gs2[c].IsButtonDown(Buttons.DPadLeft)) return true; break;
                                case BUTTON.RIGHT: if (gs2[c].IsButtonDown(Buttons.DPadRight)) return true; break;
                                case BUTTON.UP: if (gs2[c].IsButtonDown(Buttons.DPadUp)) return true; break;
                                case BUTTON.DOWN: if (gs2[c].IsButtonDown(Buttons.DPadDown)) return true; break;
                            }                           
                        }
                        break;
                    case DevType.CPU:
                        stick = 0.0f;
                        switch (b)
                        {
                            case BUTTON.LEFT: stick = -cpu[c].stick.X; break;
                            case BUTTON.RIGHT: stick = cpu[c].stick.X; break;
                            case BUTTON.UP: stick = cpu[c].stick.Y; break;
                            case BUTTON.DOWN: stick = -cpu[c].stick.Y; break;
                        }
                        if (stick > th) return true;
                        break;
                    default: break;
                }
            }
            return false;
        }

        public Vector2 Stick(DevType[] dev = null)
        {
            if (dev == null) dev = all;
            return Stick(cur, dev);
        }

        public Vector2 Stick(int c, DevType[] dev = null)
        {
            Vector2 ans = new Vector2( 0.0f, 0.0f );
            foreach (DevType d in dev)
            {
                Vector2 tmp = new Vector2( 0.0f, 0.0f );
                switch (d)
                {
                    case DevType.KEYBOARD:
                        if (isOn(BUTTON.LEFT, dev))     tmp.X -= 1.0f;
                        if (isOn(BUTTON.RIGHT, dev))    tmp.X += 1.0f;
                        if (isOn(BUTTON.UP, dev))       tmp.Y -= 1.0f;
                        if (isOn(BUTTON.DOWN, dev))     tmp.Y += 1.0f;
                        break;
                    case DevType.GAMEPAD1:
                        if (gs1[c].IsConnected)
                        {
                            tmp = gs1[c].ThumbSticks.Left;
                            tmp.Y = -tmp.Y;
                            if (gs1[c].IsButtonDown(Buttons.DPadLeft))
                            {
                                tmp.X = -1;
                            }
                            if (gs1[c].IsButtonDown(Buttons.DPadRight))
                            {
                                tmp.X = 1;
                            }
                            if (gs1[c].IsButtonDown(Buttons.DPadUp))
                            {
                                tmp.Y = 1;
                            }
                            if (gs1[c].IsButtonDown(Buttons.DPadDown))
                            {
                                tmp.Y = -1;
                            }
                        }
                        break;
                    case DevType.GAMEPAD2:
                        if (gs2[c].IsConnected)
                        {
                            tmp = gs2[c].ThumbSticks.Left;
                            tmp.Y = -tmp.Y;
                            if (gs2[c].IsButtonDown(Buttons.DPadLeft))
                            {
                                tmp.X = -1;
                            }
                            if (gs2[c].IsButtonDown(Buttons.DPadRight))
                            {
                                tmp.X = 1;
                            }
                            if (gs2[c].IsButtonDown(Buttons.DPadUp))
                            {
                                tmp.Y = 1;
                            }
                            if (gs2[c].IsButtonDown(Buttons.DPadDown))
                            {
                                tmp.Y = -1;
                            }
                        }
                        break;
                    case DevType.CPU:
                        tmp = cpu[c].stick;
                        tmp.Y = -tmp.Y;
                        break;
                    default:
                        break;
                }
                if (ans.LengthSquared() < tmp.LengthSquared()) ans = tmp;
            }
            return ans;
        }
    }
}
