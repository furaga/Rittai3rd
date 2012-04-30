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
    class SelectStage : Scene
    {
        Texture2D imCube, imSphere, imTexts, imBG;
        Texture2D[] imStage;
        Rectangle[] srcRec, distRec;
        string[] message = new string[]{
            "A/START(Z/Enter):決定(説明画面へ)",
            "BACK(Backspace):キャラ選択画面へ",
            "ステージを選んでください"
        };
        Color[] color;
        Cue scursor, sselect;

        PLAYER_INFO[] player;
        int cnt = 0;
        const int MAX_CNT = 15, SPEED = 80;
        int cursor = 0;
        int select = -1;
        Texture2D[] tex;

        public SelectStage(PLAYER_INFO[] p)
        {
            player = p;
            Initialize();
        }

        public void Initialize()
        {
            cursor = 0;
            select = 0;

            // load images
            imBG = GLOBAL.game1.Content.Load<Texture2D>("BG");
            imCube = GLOBAL.game1.Content.Load<Texture2D>("Cube");
            imSphere = GLOBAL.game1.Content.Load<Texture2D>("Sphere");
            imTexts = GLOBAL.game1.Content.Load<Texture2D>("SelectStageTexts");
            imStage = new Texture2D[]{
                        GLOBAL.game1.Content.Load<Texture2D>("Stage1"),
                        GLOBAL.game1.Content.Load<Texture2D>("Stage2"),
                        GLOBAL.game1.Content.Load<Texture2D>("Stage3")
            };

            srcRec = new Rectangle[] {
                new Rectangle( 0,0,418,61 ),
                new Rectangle( 0,61,268,46 ),
                new Rectangle( 0,111,268,46 ),
                new Rectangle( 0,158,268,46 )
            };

            double ratio_sphere = imSphere.Height / (double)imSphere.Width;
            double ratio_cube = imCube.Height / (double)imCube.Width;
            double ratio_stage = imStage[cursor].Height / (double)imStage[cursor].Width;
            distRec = new Rectangle[] {
                new Rectangle( 20,30,(int)(418 * 48 / 61.0), 48 ),    // Select Stage
                new Rectangle( 800-20 - 268,400-20 - 47,268,46 ),       // STAGE1
                new Rectangle( 800-20 - 268,500-20 - 47,268,46 ),       // STAGE2
                new Rectangle( 800-20 - 268,600-20 - 47,268,46 ),       // STAGE3
                new Rectangle(20,100,180,(int)(180*ratio_sphere)),      // 球の画像
                new Rectangle(220,100,180,(int)(180*ratio_cube)),       // 立法体の画像
                new Rectangle(30, 580 - (int)(430*ratio_stage), 430, (int)(430*ratio_stage))     // ステージの画像
            };

            color = new Color[] {
                Color.Silver,       // Select Stage
                Color.White,        // STAGE1
                Color.White,        // STAGE2
                Color.White,        // STAGE3
                Color.Red,          // 球の画像
                Color.Blue,         // 立法体の画像
                Color.White,        // ステージの画像
                Color.Yellow        // フォーカスがあってるときのSTAGE＊
            };

            tex = new Texture2D[]{
                player[ 0 ].character == CHARACTER.CUBE ? imCube :
                player[ 0 ].character == CHARACTER.SPHERE ? imSphere : null,
                player[ 1 ].character == CHARACTER.CUBE ? imCube :
                player[ 1 ].character == CHARACTER.SPHERE ? imSphere : null,
                imStage[ 0 ]
            };

            for (int i = 0; i < distRec.Length; i++)
            {
                distRec[i].Offset(-MAX_CNT * SPEED, 0);
            }
        }

        Scene nextScene = null;

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

            if (0 <= cnt && cnt < MAX_CNT)
            {
                cnt++;
                for (int i = 0; i < distRec.Length; i++)
                {
                    distRec[i].Offset(SPEED, 0);
                }
                return this;
            }

            if (0 > cnt && cnt >= -MAX_CNT)
            {
                cnt--;
                for (int i = 0; i < distRec.Length; i++)
                {
                    distRec[i].Offset(-SPEED, 0);
                }
                if (cnt == -MAX_CNT - 1)
                {
                    if (nextScene.GetType() == typeof(Loading))
                    {
                        GLOBAL.bgm.Stop(AudioStopOptions.AsAuthored);
                    }
                    else
                    {
                        GLOBAL.bgm.Pause();
                    }
                    return nextScene;
                }
                return this;
            }

            InputManager input = GLOBAL.inputManager;

            //-----------------------
            // 共通の操作
            //-----------------------
            if (input.isDown(BUTTON.BACK))
            {
                nextScene = new SelectChar(player);
                cnt = -1;
                Cue c = GLOBAL.soundBank.GetCue("back");
                c.Play();
                return this;
            }
            if (input.isDown(BUTTON.A) || input.isDown(BUTTON.START))
            {
                nextScene = new Loading(player, select);
                cnt = -1;
                sselect = GLOBAL.soundBank.GetCue("enter");
                sselect.Play();
                return this;
            }
            if (input.isDown(BUTTON.DOWN))
            {
                cursor = (cursor + 1) % imStage.Length;
                scursor = GLOBAL.soundBank.GetCue("cursor");
                scursor.Play();
            }
            if (input.isDown(BUTTON.UP))
            {
                cursor = (cursor + imStage.Length - 1) % imStage.Length;
                scursor = GLOBAL.soundBank.GetCue("cursor");
                scursor.Play();
            }
            select = cursor;
            return this;
        }

        public void Draw()
        {
            tex[2] = imStage[cursor];

            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(imBG, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);

            // 球などの絵の表示
            for (int i = 0; i < tex.Length; i++)
            {
                GLOBAL.spriteBatch.Draw(tex[i], distRec[i + 4], color[i + 4]);
            }

            // 文字
            for (int i = 0; i < srcRec.Length; i++)
            {
                if (1 <= i && i - 1 == cursor)
                {
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, distRec[i], new Color(255, 255, 255, 80));
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin();
                }
                Color c = color[i];
                if (1 <= i && i - 1 == select) c = color[color.Length - 1];
                GLOBAL.spriteBatch.Draw(imTexts, distRec[i], srcRec[i], c);
            }

            // メッセージ
            int offset = (cnt >= 0 ? -(cnt - MAX_CNT) * SPEED : -cnt * SPEED);
            for (int i = 0; i < 2; i++)
            {
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message[i],
                    new Vector2(offset + GLOBAL.WindowWidth - 20 - GLOBAL.fontArial.MeasureString(message[i]).X, GLOBAL.WindowHeight * 0.5f - 30 + 50 * (i - 2)),
                    Color.White);
            }

            GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message[2],
                new Vector2(offset + GLOBAL.WindowWidth - 10 - GLOBAL.fontArial.MeasureString(message[2]).X,
                    5 - GLOBAL.fontArial.MeasureString(message[2]).Y + distRec[0].Y + distRec[0].Height),
                Color.White);

            GLOBAL.spriteBatch.End();
        }
    }
}
