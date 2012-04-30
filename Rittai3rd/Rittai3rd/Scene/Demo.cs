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
    class Demo : Scene
    {
        Video video;
        VideoPlayer videoPlayer;
        string message = "A(Z):タイトル画面へ";

        public Demo()
        {
            Initialize();
        }

        public void Initialize()
        {
            video = GLOBAL.game1.Content.Load<Video>("Demo");
            videoPlayer = new VideoPlayer();
            videoPlayer.IsLooped = true;
            videoPlayer.IsMuted = false;
            videoPlayer.Play(video);
            flag_exit = false;
        }

        bool flag_exit = false;
        
        public Scene Update(GameTime gameTime)
        {
            if (!flag_exit)
            {
                if (GLOBAL.inputManager.isDown(BUTTON.A) || GLOBAL.inputManager.isDown(BUTTON.START))
                {
                    flag_exit = true; 
                    Cue c = GLOBAL.soundBank.GetCue("back");
                    c.Play();
                }
            }
            else
            {
                videoPlayer.Stop();
                return new Title();
            }
            return this;
        }

        public void Draw()
        {
            Texture2D tex = videoPlayer.GetTexture();
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(tex, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);
            GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message,
                    new Vector2(GLOBAL.WindowWidth - 30, 30) - GLOBAL.fontArial.MeasureString(message), Color.Red);
            GLOBAL.spriteBatch.End();
        }
    }
}
