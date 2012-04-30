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
    class Exit : Scene
    {
        Texture2D imBG;
        string message = "See You Next Time";
        Color color;
        Vector2 pos;
        TimeSpan time;
        const double time_exit = 3.0;
        const double time_appear = 1.0;

        public Exit()
        {
            Initialize();
        }

        // 800 * 500
        public void Initialize()
        {
            imBG = GLOBAL.game1.Content.Load<Texture2D>("BG");
            color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            time = TimeSpan.Zero;
            pos = GLOBAL.fontArial.MeasureString(message);
            pos = new Vector2(GLOBAL.WindowWidth - 30, GLOBAL.WindowHeight - 30) - pos;
        }

        public Scene Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime;

            if ( time < TimeSpan.FromSeconds( time_appear ) )
            {
                color.A = (byte)(Color.White.A * time.TotalSeconds / time_appear);
            }
            else
            {
                color.A = Color.White.A;
                if (time > TimeSpan.FromSeconds(time_exit))
                {
                    return null;
                }
            }
            return this;
        }

        public void Draw()
        {
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(imBG, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);
            GLOBAL.spriteBatch.End();
            GLOBAL.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message, pos, color);
            GLOBAL.spriteBatch.End();
        }
    }
}
