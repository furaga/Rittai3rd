using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace リッタイver3
{
    class Debug
    {
        SpriteFont debugfont;
        string[][] message;
        const int MAX_MESSAGE = 500;
        int cnt;

        public Debug()
        {
            message = new string[MAX_MESSAGE][];
            debugfont = GLOBAL.game1.Content.Load<SpriteFont>("debugfont");
            cnt = 0;
        }

        public int AddMessage(string mes, string val, int id = -1)
        {
            cnt++;
            cnt %= MAX_MESSAGE;
            message[cnt] = new string[]{ mes, val };
            return id;
        }

        public void Clear()
        {
            cnt = 0;
        }

        public void NewLine()
        {
            int n = cnt / 60 + 1;
            n *= 60;
            n = Math.Min(n, MAX_MESSAGE);
            cnt = n;
        }

        public void Draw()
        {
            int x = 10;
            int max = 0;
            GLOBAL.spriteBatch.Begin();
            for (int i = 0; i < cnt; i++)
            {
                if (message[i] != null && message[i].Length == 2)
                {
                    int y = 10 * (i % 60);
                    Vector2 v = debugfont.MeasureString(message[i][0]);
                    Vector2 v2 = debugfont.MeasureString("= " + message[i][1]);
                    max = (int)Math.Max(v.X + 150 + v2.X, max);
                    GLOBAL.spriteBatch.DrawString(debugfont, message[i][0], new Vector2(x, y), new Color(255, 0, 0));
                    GLOBAL.spriteBatch.DrawString(debugfont, "= " + message[i][1], new Vector2(x + 150, y), new Color(255, 0, 0));
                }
                
                if (i % 60 == 59)
                {
                    x += 400;
                    max = 0;
                }
            }
            GLOBAL.spriteBatch.End();
        }
    }
}
