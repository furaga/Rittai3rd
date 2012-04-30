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
    class SelectChar : Scene
    {
        int[] index = new int[]{0, 1};

        Texture2D imCube, imSphere, imCursor, imTexts, imBG;
        Rectangle[] srcRec, distRec;
         
        Rectangle[][] dist_rec = new Rectangle[][] {
                            new Rectangle[] {
                                 new Rectangle(200 - 126,520,252,49), // "PLAYER"(1P)
                                 new Rectangle(200 - 65,520,130,49), // "CPU"(1P)
                             },
                             new Rectangle[] {
                                 new Rectangle(600 - 126,520,252,49), // "PLAYER" (2P)
                                 new Rectangle(600 - 65,520,130,49), // "CPU" (2P)
                             }
        };
        Rectangle[] src_rec = new Rectangle[] {
                            new Rectangle(0,88,252,49),    // "PLAYER"
                            new Rectangle(253, 88, 130, 49)   // "CPU"
        };        
        bool[,] isFocus, prevIsFocus;
        Vector2[] curpos;
        Color[] color;
        string[] message = new string[]{
            "START(Space):決定(ステージ選択画面へ)",
            "BACK(Backspace):タイトル画面へ",
            "A(Z):選択",
            "キャラクターを選んでください"
        };

        PLAYER_INFO[] player = null;

        public SelectChar()
        {
            Initialize();
        }

        public SelectChar(PLAYER_INFO[] p)
        {
            player = p;
            Initialize();
        }

        Cue select;
        Cue[] change = new Cue[2];
        Cue[] cursor = new Cue[2];

        public void Initialize()
        {
            // load images
            imBG = GLOBAL.game1.Content.Load<Texture2D>("BG");
            imCube = GLOBAL.game1.Content.Load<Texture2D>("Cube");
            imSphere = GLOBAL.game1.Content.Load<Texture2D>("Sphere");
            imCursor = GLOBAL.game1.Content.Load<Texture2D>("Cursor");
            imTexts = GLOBAL.game1.Content.Load<Texture2D>("SelectCharasTexts");

            // initialize players
            if (player == null)
            {
                player = new PLAYER_INFO[] { 
                    new PLAYER_INFO( 0, PLAYER_TYPE.PLAYER, CHARACTER.CUBE, new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1 } ),
                    GLOBAL.inputManager.NumOfGamePad() >= 2 ? new PLAYER_INFO( 1, PLAYER_TYPE.PLAYER, CHARACTER.CUBE, new DevType[] { DevType.GAMEPAD2 } ) : new PLAYER_INFO( 1, PLAYER_TYPE.CPU, CHARACTER.CUBE, new DevType[] { DevType.NONE } ) };
            }
            tex = new Texture2D[] { imSphere, imCube, 
                    player[0].character == CHARACTER.CUBE ? imCube : imSphere,
                    player[1].character == CHARACTER.CUBE ? imCube : imSphere };
 
            srcRec = new Rectangle[] {
                new Rectangle(0,0,445,40),     // "Select Charactor"
                new Rectangle(0,41,80,46),      // "1P"
                new Rectangle(82,41,80,46),     // "2P"
                src_rec[(int)player[0].type],     // "PLAYER"
                src_rec[(int)player[1].type],   // "CPU"
            };

            double ratio_sphere = imSphere.Height / (double)imSphere.Width;
            double ratio_cube = imCube.Height / (double)imCube.Width;
            distRec = new Rectangle[] {
                new Rectangle(20,20,(int)(445 * 1.2),(int)(40 * 1.2)),  // "Select Charactor"
                new Rectangle(20,320,80,50),  // "1P"
                new Rectangle(420,320,80,50),  // "2P"
                dist_rec[0][(int)player[0].type], // "PLYER" or "CPU" (1P)
                dist_rec[1][(int)player[1].type], // "PLYER" or "CPU" (2P)
                new Rectangle(20,120,180,(int)(180*ratio_sphere)),    // 球の画像
                new Rectangle(220,120,180,(int)(180*ratio_cube)),    // 立法体の画像
                new Rectangle(20, GLOBAL.WindowHeight - 20 -(int)(360 * ratio_sphere),360,(int)(360 * ratio_sphere)),    // １Pのキャラの画像
                new Rectangle(420, GLOBAL.WindowHeight - 20 -(int)(360 * ratio_sphere),360,(int)(360 * ratio_sphere))    // ２Pのキャラの画像
            };
            for (int i = 0; i < distRec.Length; i++)
            {
                distRec[i].Offset(-MAX_CNT * SPEED, 0);
            }
            color = new Color[] {
                Color.Silver,   // "Select Charactor"
                Color.Yellow,   // "1P"
                Color.Cyan,     // "2P"
                Color.Yellow,   // "PLYER" or "CPU" (1P)
                Color.Cyan,     // "PLYER" or "CPU" (2P)
                Color.Yellow,   // 球の画像
                Color.Green,    // 立法体の画像
                Color.Red,      // １Pのキャラの画像
                Color.Blue      // ２Pのキャラの画像
            };

            isFocus = new bool[2, distRec.Length];
            for (int i = 0; i < isFocus.Length / 2; i++)
            {
                isFocus[0, i] = false;
                isFocus[1, i] = false;
            }
            prevIsFocus = new bool[2, distRec.Length];
            for (int i = 0; i < prevIsFocus.Length / 2; i++)
            {
                prevIsFocus[0, i] = false;
                prevIsFocus[1, i] = false;
            }

            curpos = new Vector2[] {
                    new Vector2(200, 450),
                    new Vector2(600, 450)
            };
        }

        double vcur = 400.0f;
        Vector2 offset_cur = new Vector2(10, 0);
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
                if (cnt == -MAX_CNT - 1) return nextScene;
                return this;
            }

            InputManager input = GLOBAL.inputManager;
            
            //-----------------------
            // 共通の操作
            //-----------------------
            if (input.isDown(BUTTON.BACK))
            {
                nextScene = new Title();
                cnt = -1; 
                Cue c = GLOBAL.soundBank.GetCue("back");
                c.Play();
                return this;
            }
            if (input.isDown(BUTTON.START))
            {
                if (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.PLAYER)
                {
                    player[0].dev = new DevType[] { DevType.CPU };
                    player[1].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2 };
                }
                if (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.CPU)
                {
                    player[0].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2, DevType.CPU };
                    player[1].dev = new DevType[] { DevType.CPU };
                }
                nextScene = new SelectStage(player);
                cnt = -1;
                select = GLOBAL.soundBank.GetCue("enter");
                select.Play();
                return this;
            }

            isFocus = new bool[2, distRec.Length];
            for (int i = 0; i < isFocus.Length / 2; i++)
            {
                isFocus[0, i] = false;
                isFocus[1, i] = false;
            }

            //-----------------------
            // 1Pの操作
            //-----------------------
            Control(0, gameTime);

            //-----------------------
            // 2Pの操作
            //-----------------------
            if (player[1].type == PLAYER_TYPE.PLAYER)
            {
                Control(1, gameTime);
            }

            //-----------------------
            // 共通の操作
            //-----------------------
            if (player[0].type == PLAYER_TYPE.PLAYER && player[1].type == PLAYER_TYPE.PLAYER)
            {
                switch (input.NumOfGamePad())
                {
                    case 0: case 1:
                        player[0].dev = new DevType[] { DevType.KEYBOARD };
                        player[1].dev = new DevType[] { DevType.GAMEPAD1 };
                        break;
                    case 2:
                        player[0].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1 };
                        player[1].dev = new DevType[] { DevType.GAMEPAD2 };
                        break;
                    default:
                        break;
                }
            }
            if (player[0].type == PLAYER_TYPE.PLAYER && player[1].type == PLAYER_TYPE.CPU)
            {
                player[0].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2 };
                player[1].dev = new DevType[] { DevType.CPU };
            }
            if (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.PLAYER)
            {
                player[0].dev = new DevType[] { /*DevType.CPU*/ };
                player[1].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2 };
            }
            if (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.CPU)
            {
                player[0].dev = new DevType[] { DevType.KEYBOARD, DevType.GAMEPAD1, DevType.GAMEPAD2 };
                player[1].dev = new DevType[] { DevType.CPU };
            }
            
            prevIsFocus = isFocus;
            return this;
        }

        public void Control(int idx, GameTime gameTime)
        {
            InputManager input = GLOBAL.inputManager;
            Vector2 v;
            double dd = vcur * gameTime.ElapsedGameTime.TotalSeconds;
            // カーソルの移動
            v = input.Stick(player[idx].dev);
            v.X *= (float)dd; v.Y *= (float)dd;
            curpos[idx] += v;
            // 境界処理
            if (curpos[idx].X < -offset_cur.X)                        curpos[idx].X = -offset_cur.X;
            if (curpos[idx].X > -offset_cur.X + GLOBAL.WindowWidth)   curpos[idx].X = -offset_cur.X + GLOBAL.WindowWidth;
            if (curpos[idx].Y < -offset_cur.Y)                        curpos[idx].Y = -offset_cur.Y;
            if (curpos[idx].Y > -offset_cur.Y + GLOBAL.WindowHeight)   curpos[idx].Y = -offset_cur.X + GLOBAL.WindowHeight;
            
            if (isInRect(curpos[idx], new Rectangle( 0, distRec[3].Y, GLOBAL.WindowWidth / 2, GLOBAL.WindowHeight - distRec[ 3 ].Y)))         // "PLYER" or "CPU" (1P)
            {
                isFocus[idx, 3 ] = true;
                if (prevIsFocus[idx, 3] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else if (isInRect(curpos[idx], new Rectangle( GLOBAL.WindowWidth / 2, distRec[3].Y, GLOBAL.WindowWidth / 2, GLOBAL.WindowHeight - distRec[ 3 ].Y)))      // "PLYER" or "CPU" (2P)
            {
                isFocus[idx, 4] = true;
                if (prevIsFocus[idx, 4] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else if (isInRect(curpos[idx], distRec[5]))       // 球の画像
            {
                isFocus[idx, 5] = true;
                if (prevIsFocus[idx, 5] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else if (isInRect(curpos[idx], distRec[6]))       // 立方体の画像
            {
                isFocus[idx, 6] = true;
                if (prevIsFocus[idx, 6] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else if (isInRect(curpos[idx], distRec[7]))        // 1Pのキャラの画像
            {
                isFocus[idx, 7] = true;
                if (prevIsFocus[idx, 7] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else if (isInRect(curpos[idx], distRec[8]))        // 2Pのキャラの画像
            {
                isFocus[idx, 8] = true;
                if (prevIsFocus[idx, 8] == false)
                {
                    cursor[idx] = GLOBAL.soundBank.GetCue("cursor");
                    cursor[idx].Play();
                }
            }
            else
            {

            }

            if (GLOBAL.inputManager.isDown(BUTTON.A, player[idx].dev))
            {
                change[idx] = GLOBAL.soundBank.GetCue("change");
                change[idx].Play();

                if (isFocus[idx, 3])         // "PLYER" or "CPU" (1P)
                {
                    player[0].type = (PLAYER_TYPE)(1 - (int)player[0].type);
                    srcRec[3] = src_rec[(int)player[0].type];
                    distRec[3] = dist_rec[0][(int)player[0].type];
                }
                else if (isFocus[idx, 4])      // "PLYER" or "CPU" (2P)
                {
                    player[1].type = (PLAYER_TYPE)(1 - (int)player[1].type);
                    srcRec[4] = src_rec[(int)player[1].type];
                    distRec[4] = dist_rec[1][(int)player[1].type];
                }
                else if (isFocus[idx, 5])       // 球の画像
                {
                    player[index[idx]].character = CHARACTER.SPHERE;
                    tex[2 + index[idx]] = imSphere;
                }
                else if (isFocus[idx, 6])       // 立方体の画像
                {
                    player[index[idx]].character = CHARACTER.CUBE;
                    tex[2 + index[idx]] = imCube;
                }
                else if (isFocus[idx, 7])        // 1Pのキャラの画像
                {
                    if (index[idx] == 1) index[idx] = 0;
                    else
                    {
                        player[index[idx]].character = (CHARACTER)( 1 - (int)player[index[idx]].character );
                        tex[2 + index[idx]] = player[index[idx]].character == CHARACTER.CUBE ? imCube : imSphere;
                    }
                }
                else if (isFocus[idx, 8])        // 2Pのキャラの画像
                {
                    if (index[idx] == 0) index[idx] = 1;
                    else
                    {
                        player[index[idx]].character = (CHARACTER)(1 - (int)player[index[idx]].character);
                        tex[2 + index[idx]] = player[index[idx]].character == CHARACTER.CUBE ? imCube : imSphere;
                    }
                }
                else
                {
                    change[idx].Pause();
                }
            }
        }

        Texture2D[] tex;

        public bool isInRect(Vector2 pos, Rectangle rec)
        {
            bool ans = rec.X <= pos.X && pos.X <= rec.X + rec.Width && rec.Y <= pos.Y && pos.Y <= rec.Y + rec.Height;
            return ans;
        }

        int cnt = 0;
        const int MAX_CNT = 15, SPEED = 80;

        public void Draw()
        {
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(imBG, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);
            
            // 球などの絵の表示
            for (int i = 0; i < tex.Length; i++)
            {
                if (    ((player[0].type == PLAYER_TYPE.PLAYER || (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.CPU))
                        && isFocus[0, i + 5] ) || 
                        (player[1].type == PLAYER_TYPE.PLAYER && isFocus[1, i + 5] ) )
                {
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, distRec[i + 5], new Color(255, 255, 255, 64) );
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin();
                }
                GLOBAL.spriteBatch.Draw(tex[i], distRec[i + 5], color[ i + 5 ]);
            }
            // 文字
            for (int i = 0; i < srcRec.Length; i++)
            {
                if (    ((player[0].type == PLAYER_TYPE.PLAYER || (player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.CPU))
                        && isFocus[0, i]) ||
                        (player[1].type == PLAYER_TYPE.PLAYER && isFocus[1, i]))
                {
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive );
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, distRec[i], new Color(255,255,255,64) );
                    GLOBAL.spriteBatch.End();
                    GLOBAL.spriteBatch.Begin();
                }
                GLOBAL.spriteBatch.Draw(imTexts, distRec[i], srcRec[i], color[i]);
            }

            int offset = ( cnt >= 0 ? -(cnt - MAX_CNT) * SPEED : -cnt * SPEED );

            for (int i = 0; i < 3; i++)
            {
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message[i],
                    new Vector2(offset + GLOBAL.WindowWidth - 20 - GLOBAL.fontArial.MeasureString(message[i]).X, GLOBAL.WindowHeight * 0.5f - 30 + 50 * (i - 2)),
                    Color.White);
            }

            GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, message[3],
                new Vector2(offset + GLOBAL.WindowWidth - 10 - GLOBAL.fontArial.MeasureString(message[3]).X,
                    5 - GLOBAL.fontArial.MeasureString(message[3]).Y + distRec[0].Y + distRec[0].Height),
                Color.White);

            // カーソル
            for (int i = 0; i < curpos.Length; i++)
            {
                if (i == 0 && player[0].type == PLAYER_TYPE.CPU && player[1].type == PLAYER_TYPE.PLAYER) continue;
                if (i == 1 && player[i].type == PLAYER_TYPE.CPU) continue;
                GLOBAL.spriteBatch.Draw(imCursor, curpos[i], (i == 0 ? Color.Red : Color.Blue));
                string s = ( 1 + index[ i ] ) + "P";
                GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, curpos[i] + new Vector2( 0, imCursor.Height), Color.White);
            }


            GLOBAL.spriteBatch.End();
        }
    }
}
