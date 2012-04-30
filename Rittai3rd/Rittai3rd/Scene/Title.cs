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
    class Title : Scene
    {
        const int BATTLE = 0, DEMO = 1, END = 2, BATTLE1 = 5, DEMO1 = 3, END1 = 4, TITLE = 6;
        Rectangle[] srcRec;
        Rectangle[] distRec;
        Color[] color;
        Texture2D imTitleTexts, imBG;
        string[] explain = {
                               "デモムービーを再生します",
                               "ゲームを終了します",
                               "対戦を開始します",
                               "A/START(Z/Enter):決定",
                               "↑↓:選択"
                           };
        int focus = 2;
        const int MAX_FOCUS = 3;
        TimeSpan time = TimeSpan.Zero;
        Cue cursor, select;

        public Title()
        {
            Initialize();
        }

        // 800 * 500
        public void Initialize()
        {
            imTitleTexts = GLOBAL.game1.Content.Load<Texture2D>("ImTitleTexts");
            srcRec = new Rectangle[]{
                            new Rectangle(0,0,300,47),          // BATTLE
                            new Rectangle(0,49,300,46),         // DEMO
                            new Rectangle(0,97,300,45),         // END
                            new Rectangle(0,145,300,49),        // DEMO1
                            new Rectangle(0,197,300,49),        // END1
                            new Rectangle(0,249,300,49),        // BATTLE1
                            new Rectangle(0,300,300,60)         // TITLE
                        };
            double r = 1.7;
            distRec = new Rectangle[]{
                            new Rectangle(800-320,100-48,300,48),    // BATTLE
                            new Rectangle(800-320,200-46,300,46),   // DEMO
                            new Rectangle(800-320,300-45,300,45),  // END
                            new Rectangle((int)(275-150*r),300-(int)(52*r),(int)(300*r),(int)(52*r)),   //DEMO1
                            new Rectangle((int)(275-150*r),300-(int)(52*r),(int)(300*r),(int)(52*r)), // END1
                            new Rectangle((int)(275-150*r),300-(int)(52*r),(int)(300*r),(int)(52*r)), // BATTLE1
                            new Rectangle(800-450,480,(int)(300 * 1.5),(int)(60 * 1.5))  // TITLE
                        };
            color = new Color[]{
                            Color.White,
                            Color.White,
                            Color.White,
                            Color.Blue,
                            Color.White,
                            Color.Red,
                            Color.Gold
                        };
            imBG = GLOBAL.game1.Content.Load<Texture2D>("BG");
            flag_exit = false;
        }

        const double time_exit = 0.3;
        const double v = 4000.0;
        bool flag_exit = false;
        TimeSpan t= TimeSpan.Zero;
        public Scene Update(GameTime gameTime)
        {
            if (GLOBAL.bgm.IsStopped)
            {
                GLOBAL.bgm = GLOBAL.soundBank.GetCue(GLOBAL.bgm.Name);
                GLOBAL.bgm.Play();
            }
            else if (GLOBAL.bgm.IsPaused)
            {
                GLOBAL.bgm.Resume();
            }

            t += gameTime.ElapsedGameTime;
            if (t > TimeSpan.FromSeconds(30))
            {
                flag_exit = true;
                focus = 0;
            }

            if (!flag_exit)
            {
                if (GLOBAL.inputManager.isDown(BUTTON.A) || GLOBAL.inputManager.isDown(BUTTON.START))
                {
                    flag_exit = true;
                    time = TimeSpan.Zero;
                    t = TimeSpan.Zero;
                    select = GLOBAL.soundBank.GetCue("enter");
                    select.Play();
                }
                else
                {
                    if (GLOBAL.inputManager.isDown(BUTTON.DOWN))
                    {
                        focus = (focus + 1) % MAX_FOCUS;
                        t = TimeSpan.Zero;
                        cursor = GLOBAL.soundBank.GetCue("cursor");
                        cursor.Play();
                    }
                    if (GLOBAL.inputManager.isDown(BUTTON.UP))
                    {
                        focus = (focus + MAX_FOCUS - 1) % MAX_FOCUS;
                        t = TimeSpan.Zero;
                        cursor = GLOBAL.soundBank.GetCue("cursor");
                        cursor.Play();
                    }
                }
            }
            else
            {
                time += gameTime.ElapsedGameTime;
                double dt = gameTime.ElapsedGameTime.TotalSeconds;
                distRec[BATTLE].X += time.TotalSeconds > 0.20 ? (int)(v * dt) : 0;
                distRec[DEMO].X += time.TotalSeconds > 0.10 ? (int)(v * dt) : 0;
                distRec[END].X += (int)(v * dt);
                distRec[TITLE].X += (int)(v * dt);
                distRec[BATTLE1].X -= (int)(v * dt);
                distRec[DEMO1].X -= (int)(v * dt);
                distRec[END1].X -= (int)(v * dt);

                if (time > TimeSpan.FromSeconds(time_exit))
                {
                    switch (focus)
                    {
                        case 0:     // DEMO
                            GLOBAL.bgm.Stop(AudioStopOptions.AsAuthored);
                            return new Demo();
                        case 1:     // END
                            GLOBAL.bgm.Stop(AudioStopOptions.AsAuthored);
                            return new Exit();
                        case 2:     // BATTLE
                            return new SelectChar();
                    }
                }
            }
            return this;
        }

        public void Draw()
        {
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(imBG, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);
            Color c;
            for (int i = 0; i <= TITLE; i++)
            {
                if (DEMO1 <= i && i <= BATTLE1)
                {
                    if (i != focus + DEMO1)
                    {
                        continue;
                    }
                }

                c = color[i];

                if (BATTLE <= i && i <= END)
                {
                    if (i == (focus + 1) % 3)
                    {
                        c = Color.OrangeRed;
                    }
                }

                GLOBAL.spriteBatch.Draw(imTitleTexts, distRec[ i ], srcRec[ i ], c);
            }
            if (!flag_exit)
            {
                int h = (distRec[3].Y + distRec[3].Height) + (distRec[TITLE].Y);
                h /= 2;
                h -= (int)GLOBAL.fontArial.MeasureString(explain[focus]).Y / 2;
                int w = 400 - (int)GLOBAL.fontArial.MeasureString(explain[focus]).X / 2;
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, explain[focus], new Vector2(w, h), Color.White);

                for (int i = 3; i < 5; i++)
                {
                    GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, explain[i],
                        new Vector2(30, 40 * ( i - 3 ) + GLOBAL.WindowHeight - 100), Color.White);
                }
            }
            GLOBAL.spriteBatch.End();
        }
    }
}
