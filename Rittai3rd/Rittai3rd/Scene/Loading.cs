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
using System.Threading;

namespace リッタイver3
{
    class Loading : Scene
    {
        string stage;
        PLAYER_INFO[] player;
        Scene nextScene;
        Thread thread = null;
        Texture2D imRokkaku;
        Vector2[] rokupos = new Vector2[36];
        double[] rokualp = new double[36];
        double width;
        double height;
        double tan30;

        string[] message = {        
                               "ゲームパッド",                  "キーボード",                      "動き",
                               "→/←",                         "→/←",                           "移動",
                               "A",                             "Z",                               "通常攻撃",
                               "↑/↓/→/← + A",               "↑/↓/→/← + Z",                 "強攻撃",
                               "↑/↓/→/← + A + LBまたはRB" , "↑/↓/→/← + Z + C",             "スマッシュ",
                               "X" ,                            "X",                               "通常必殺技",
                               "↑/↓ + X" ,                    "↑/↓ + X",                       "上/下必殺技",
                               "(ゲージMAX)+X",                  "(ゲージMAX)+X",                  "超必殺技",
                               "B/Y" ,                          "Space",                           "ジャンプ",
                               "LB/RB+↑" ,                     " ",                               "ジャンプ",
                               "(地上で)LT/RT" ,                "(地上で)V",                       "シールド",
                               "(地上で)↓ + LT/RT" ,           "(地上で)↓ + V",                  "その場緊急回避",
                               "(地上で)→/← + LT/RT" ,        "(地上で)→/← + V" ,              "横緊急回避",
                               "(空中で)LT/RT" ,                "(空中で)V" ,                      "空中緊急回避",
                               "START" ,                        "Enter",                           "ポーズ",
        };
        Vector2[] mespos = new Vector2[20];

        public Loading(PLAYER_INFO[] p, int s)
        {
            player = p;
            stage = "Stage" + (s + 1);
            Initialize();
        }

        public void Initialize()
        {
            gagefont = GLOBAL.game1.Content.Load<SpriteFont>("GageFont");
            nextScene = new Fight( player, stage );
            thread = new Thread(new ThreadStart(nextScene.Initialize));
            thread.Start();

            mespos = new Vector2[message.Length];
            for (int i = 0; i < message.Length / 3; i++)
            {
                mespos[i * 3] = new Vector2(20 - (int)(v * (0.1f + 0.02f * i)), 30 * i + (i == 0 ? -10 : 30) + 50);
                mespos[i * 3 + 1] = new Vector2(300 - (int)(v * (0.1f + 0.02f * i)), 30 * i + (i == 0 ? -10 : 30) + 50);
                mespos[i * 3 + 2] = new Vector2(550 + (int)(v * (0.1f + 0.02f * i)), 30 * i + (i == 0 ? -10 : 30) + 50);
            }

            imRokkaku = GLOBAL.game1.Content.Load<Texture2D>("rokkakukei");
            TimeSpan time = TimeSpan.FromSeconds(-1.0);

            // 六角形を敷き詰める
            width = 150.0;
            height = imRokkaku.Height * width / imRokkaku.Width;
            tan30 = Math.Tan(MathHelper.ToRadians(30.0f));

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    double x = width * j - (i % 2 == 0 ? width * 0.5 : 0.0);
                    double y = (i - 1) * 0.75 * height;
                    rokupos[ i * 6 + j ] = new Vector2((float)x, (float)y);
                }
            }
            for (int i = 0; i < rokualp.Length; i++) rokualp[i] = 32;
        }

        double v = 1600.0;
        bool flg = true;
        TimeSpan time = TimeSpan.Zero;
        int lighting = 0;
        int lx = -1, ly = -1;

        public Scene Update(GameTime gameTime)
        {
            double t = 1.5;
            time += gameTime.ElapsedGameTime;

            if (thread.IsAlive)
            {
                for (int i = 0; i < rokualp.Length; i++) rokualp[i] = 32;
                if (time > TimeSpan.FromSeconds(t + 1.0))
                {
                    lighting = -1;
                    if (time > TimeSpan.FromSeconds(t + 2.0))
                    {
                        time = TimeSpan.Zero;
                    }
                }
                else if (time > TimeSpan.FromSeconds( 1.0 ))
                {
                    lighting = (int)(12.0 * ( time.TotalSeconds - 1.0 )/ t);
                    lx = lighting / 2;
                    ly = lighting % 2 == 0 ? 4 : 3;
                    rokualp[lx + ly * 6] = 64;
                }
            }
            else
            {
                if (rokualp[0] < 100.0)
                {
                    rokualp[0] += 1.0;
                    for (int i = 1; i < rokualp.Length; i++) rokualp[i] = rokualp[0];
                }
            }                    

            InputManager input = GLOBAL.inputManager;

            if (input.isDown(BUTTON.BACK))
            {
                Cue c = GLOBAL.soundBank.GetCue("back");
                c.Play();
                return new SelectStage(player);
            }
            if (!thread.IsAlive && input.isDown(BUTTON.START))
            {
                Cue sound = GLOBAL.soundBank.GetCue("enter");
                sound.Play();
                return nextScene;
            }

            double d = v * gameTime.ElapsedGameTime.TotalSeconds;

            if (flg)
            {
                flg = false;
                for (int i = 0; i < message.Length; i++)
                {
                    switch (i % 3)
                    {
                        case 0:
                            mespos[i].X += (float)d;
                            if (mespos[i].X >= 20)
                            {
                                mespos[i].X = 20;
                            }
                            else
                            {
                                flg |= true;
                            }
                            break;
                        case 1:
                            mespos[i].X += (float)d;
                            if (mespos[i].X >= 300)
                            {
                                mespos[i].X = 300;
                            }
                            else
                            {
                                flg |= true;
                            }
                            break;
                        case 2:
                            mespos[i].X -= (float)d;
                            if (mespos[i].X <= 550)
                            {
                                mespos[i].X = 550;
                            }
                            else
                            {
                                flg |= true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return this;
        }

        public void Draw()
        {
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.Black);
            GLOBAL.spriteBatch.End();
            GLOBAL.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
//            GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, new Rectangle(0, 100, GLOBAL.WindowWidth, GLOBAL.WindowHeight - 200), new Color(255,255,255,200));

            // 六角形を敷き詰める
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int x = (int)rokupos[ i * 6 + j].X;
                    int y = (int)rokupos[ i * 6 + j].Y;
                    int w = (int)width;
                    int h = (int)height;
//                    GLOBAL.spriteBatch.Draw(imRokkaku, new Rectangle(x,y,w,h), new Color(50, 255, 50, 32));
                    GLOBAL.spriteBatch.Draw(imRokkaku, new Rectangle(x+2,y+2,w-4,h-4), new Color(50, 255, 50, (int)rokualp[i * 6 + j]));
                }
            }

            GLOBAL.spriteBatch.End();
            GLOBAL.spriteBatch.Begin();
            
            string s;
            if (thread.IsAlive)
            {
                s = "Now Loading";
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, new Vector2(780, 580) - GLOBAL.fontArial.MeasureString(s), Color.White);
            }
            else
            {
                s = "スタートボタンを押してください";
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, new Vector2(780, 580) - GLOBAL.fontArial.MeasureString(s), Color.White);
            }

            for (int i = 0; i < message.Length; i++)
            {
                GLOBAL.spriteBatch.DrawString((i / 3 == 0 ? gagefont : GLOBAL.fontArial), message[i], mespos[i], (i / 3 == -1 ? Color.Red : Color.White));
            }
            s = "Back:ステージ選択画面にもどります";
            GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, new Vector2(20,580 - GLOBAL.fontArial.MeasureString(s).Y), Color.White);
            GLOBAL.spriteBatch.End();
        }
        SpriteFont gagefont;
    }
  
}
