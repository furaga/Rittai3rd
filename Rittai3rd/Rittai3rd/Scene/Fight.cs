using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
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
    public class Fight : Scene
    {
        string stage;
        public string Stage
        {
            get
            {
                return stage;
            }
        }

        PLAYER_INFO[] player;
        ArrayList entities;
        public ArrayList EntityList
        {
            get
            {
                return entities;
            }
        }

        ArrayList attackerList, solidList, deadBlockList;

        public ArrayList AttackerList
        {
            get
            {
                return attackerList;
            }
        }
        public ArrayList SolidList
        {
            get
            {
                return solidList;
            }
        }
        public ArrayList DeadBlockList
        {
            get
            {
                return deadBlockList;
            }
        }

       
        // Pause
        Texture2D imButton, imScreen;

        Camera camera;
        public Camera Camera
        {
            get
            {
                return camera;
            }
        }
        Character[] character;
        public Character Character1P
        {
            get
            {
                
                return character[0];
            }
        }
        public Character Character2P
        {
            get
            {
                return character[1];
            }
        }
        
        Hashtable parameters;
        public Hashtable Parameters
        {
            get
            {
                return parameters;
            }
        }
       
        TimeSpan time = TimeSpan.Zero;
        UpdateFuncs[] funcs;
        enum UpdateIndex
        {
            READY = 0, FIGHT, PAUSE, RESULT, Size
        }

        UpdateIndex ui = UpdateIndex.READY;

        #region Initialize

        public Fight(PLAYER_INFO[] p, string s)
        {
            player = p;
            stage = s;
            // Initialize()は他のところ(Loading.Initialize)で呼ばれている
        }


        Texture2D imGage, imCube, imSphere;


        public void Initialize()
        {
            imGage = GLOBAL.game1.Content.Load<Texture2D>("Gage");
            imCube = GLOBAL.game1.Content.Load<Texture2D>("Cube");
            imSphere = GLOBAL.game1.Content.Load<Texture2D>("Sphere");
            imTexts = GLOBAL.game1.Content.Load<Texture2D>("FightTexts");

            gageFont = GLOBAL.game1.Content.Load<SpriteFont>("GageFont");

            // パラメータを読み込む
            parameters = new Hashtable();
            LoadParameters();

            Object o = parameters[stage];
            if (o != null)
            {
                Hashtable ht = (Hashtable)o;
                imBG = GLOBAL.game1.Content.Load<Texture2D>((o = ht["backGround"]) != null ? (string)o : "imStageModel1");
            }
            // Construct our particle system components.
            GLOBAL.explosionParticles = new MyParticleSystem(GLOBAL.game1, GLOBAL.game1.Content, (Hashtable)parameters["ExplosionParticles"]);
            GLOBAL.attackedParticles = new MyParticleSystem(GLOBAL.game1, GLOBAL.game1.Content, (Hashtable)parameters["AttackedParticles"]);
            GLOBAL.deadParticles = new MyParticleSystem(GLOBAL.game1, GLOBAL.game1.Content, (Hashtable)parameters["DeadParticles"]);
            GLOBAL.smokeParticles = new MyParticleSystem(GLOBAL.game1, GLOBAL.game1.Content, (Hashtable)parameters["SmokeParticles"]);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            GLOBAL.explosionParticles.DrawOrder = 100;
            GLOBAL.attackedParticles.DrawOrder = 200;
            GLOBAL.deadParticles.DrawOrder = 300;
            GLOBAL.smokeParticles.DrawOrder = 400;

            // Register the particle system components.
            GLOBAL.game1.Components.Add(GLOBAL.explosionParticles);
            GLOBAL.game1.Components.Add(GLOBAL.attackedParticles);
            GLOBAL.game1.Components.Add(GLOBAL.deadParticles);
            GLOBAL.game1.Components.Add(GLOBAL.smokeParticles);


            // 各エンティティの初期化
            entities = new ArrayList();
            attackerList = new ArrayList();
            solidList = new ArrayList();
            deadBlockList = new ArrayList();

            // キャラクター
            character = new Character[] {
                new Character(this, (Hashtable)parameters["Character"], player[0]),
                new Character(this, (Hashtable)parameters["Character"], player[1])
            };
            entities.Add(character[0]);
            entities.Add(character[1]);
            // カメラ
            camera = new Camera(this, (Hashtable)parameters["Camera"]);

            // ステージ
            o = parameters[stage];
            if (o != null)
            {
                Hashtable param = (Hashtable)o;
                Object o1 = param["Count"];
                if (o1 != null)
                {
                    int cnt = int.Parse((string)o1);
                    StagePart sp;
                    for (int i = 0; i < cnt; i++)
                    {
                        sp = new StagePart(this, (Hashtable)parameters[stage], i);
                        entities.Add(sp);
                        solidList.Add(sp);
                    }

                    double x = double.Parse((o = param["aliveZoneX"]) != null ? (string)o : "0");
                    double y = double.Parse((o = param["aliveZoneY"]) != null ? (string)o : "0");
                    double h = double.Parse((o = param["aliveZoneH"]) != null ? (string)o : "0");
                    double w = double.Parse((o = param["aliveZoneW"]) != null ? (string)o : "0");
                    double d = 100;
                    sp = new StagePart(this, (Hashtable)parameters[stage], cnt);
                    sp.RecVisible.Copy(new RectangleD(x - d, y + h, w + 2 * d, d));     // 上
                    sp.Depth = 200;
                    entities.Add(sp);
                    deadBlockList.Add(sp);

                    sp = new StagePart(this, (Hashtable)parameters[stage], cnt + 4);
                    sp.RecVisible.Copy(new RectangleD(x - d, y + h, w + 2 * d, d));     // 上
                    sp.Depth = 1;
                    entities.Add(sp);
                    deadBlockList.Add(sp);
                    
                    sp = new StagePart(this, (Hashtable)parameters[stage], cnt+1);
                    sp.RecVisible.Copy(new RectangleD(x - d, y - d, w + 2 * d, d));     // 下
                    sp.Depth = 200;
                    entities.Add(sp);
                    deadBlockList.Add(sp);

                    sp = new StagePart(this, (Hashtable)parameters[stage], cnt+2);
                    sp.RecVisible.Copy(new RectangleD(x - d, y, d, h + 2 * d));     // 左
                    sp.Depth = 200;
                    entities.Add(sp);
                    deadBlockList.Add(sp);

                    sp = new StagePart(this, (Hashtable)parameters[stage], cnt + 3);
                    sp.RecVisible.Copy(new RectangleD(x + w, y, d, h + 2 * d));     // 右
                    sp.Depth = 200;
                    entities.Add(sp);
                    deadBlockList.Add(sp);

                }
                else
                {
                    MessageBox.Show("parameter[\"" + stage + "\"][\"Count\"]が存在しません");
                }
            }
            else
            {
                MessageBox.Show("parameter[\"" + stage + "\"]が存在しません");
            }

            // Pause
            imButton = GLOBAL.game1.Content.Load<Texture2D>("PauseButton");
            imScreen = GLOBAL.game1.Content.Load<Texture2D>("PauseScreen");

            nextScene = this;

            time = TimeSpan.Zero;
            ui = UpdateIndex.READY;
            funcs = new UpdateFuncs[(int)UpdateIndex.Size + 1];
            funcs[(int)UpdateIndex.READY] = UpdateReady;
            funcs[(int)UpdateIndex.FIGHT] = UpdateFight;
            funcs[(int)UpdateIndex.PAUSE] = UpdatePause;
            funcs[(int)UpdateIndex.RESULT] = UpdateResult;
            funcs[(int)ui](new GameTime(TimeSpan.Zero, TimeSpan.Zero));
        }
        
        public void LoadParameters()
        {
            string filePath = Path.Combine(GLOBAL.game1.Content.RootDirectory, "Parameters.txt");

            try
            {
                // ファイルを開く
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // 文字列の読み込み
                    string line;
                    string name = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                       

                        line = line.Replace(" ", "");
                        if (line.Length <= 0) continue;
                        if (line[0] == '#') continue;

                        line = line.Replace(";", "");

                        // =で分割
                        string[] data = line.Split(new char[] { '=' });

                        // 値セット
                        switch (data.Length)
                        {
                            case 1: // [name]の形のとき
                                if (data[0][0] != '[') break;
                                name = data[0].Substring(1, data[0].Length - 2);
                                parameters.Add(name, new Hashtable());
                                break;
                            case 2: // var = val の形のとき
                                ((Hashtable)parameters[name]).Add(data[0], data[1]);
                                break;
                            default:
                                break;
                        }
                    }

                    // StreamReader を閉じる
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // 何らかの理由で読み込みエラー
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        #endregion
        
        InputManager input = GLOBAL.inputManager;
        Scene nextScene;

        public Scene Update(GameTime gameTime)
        {
            if (GLOBAL.bgm.IsStopped)
            {
                GLOBAL.bgm = GLOBAL.soundBank.GetCue("bgm2");
                GLOBAL.bgm.Play();
            }
            else if (GLOBAL.bgm.IsPaused)
            {
                GLOBAL.bgm.Resume();
            }

            time += gameTime.ElapsedGameTime;

#if __DEBUG
            if (input.isDown(BUTTON.BACK)) return new Loading(player, (stage[5] - '1'));
#endif
            // 更新処理
            if (funcs[(int)ui] != null) funcs[(int)ui](gameTime);

            if (nextScene != this)
            {
                GLOBAL.bgm.Stop(AudioStopOptions.AsAuthored);
                GLOBAL.bgm = GLOBAL.soundBank.GetCue("bgm1");
                GLOBAL.bgm.Play();
                GLOBAL.bgm.Pause();
            }

            return nextScene;
        }

        double dt1 = 100, dt2 = 100, dt3 = 100;
        #region UpdateReady

        public bool UpdateReady(GameTime gameTime)
        {
            Vector3 v1, v2;
            dt1 = double.Parse((string)((Hashtable)parameters["UpdateReady"])["dt1"]);
            dt2 = double.Parse((string)((Hashtable)parameters["UpdateReady"])["dt2"]);
            dt3 = double.Parse((string)((Hashtable)parameters["UpdateReady"])["dt3"]);

            if (time < TimeSpan.FromSeconds(dt1))
            {
                v1 = new Vector3(character[0].RecVisible.Center,0.0f);
                camera.Target = v1;
                v2 = v1 + new Vector3(
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition1X"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition1Y"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition1Z"])
                    );
                camera.Position = v2;
            }
            else if (time < TimeSpan.FromSeconds(dt1 + dt2))
            {
                v1 = new Vector3(character[1].RecVisible.Center, 0.0f);
                camera.Target = v1;
                v2 = v1 + new Vector3(
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition2X"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition2Y"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition2Z"])
                    );
                camera.Position = v2;
            }
            else if (time < TimeSpan.FromSeconds(dt1 + dt2 + dt3))
            {
                v1 = camera.initTarget;//new Vector3(character[0].RecVisible.Center + character[1].RecVisible.Center, 0.0f);
                v1.X *= 0.5f;
                v1.Y *= 0.5f;
                camera.Target = v1;

                Vector3 s = new Vector3(
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3StartX"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3StartY"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3StartZ"])
                );

                Vector3 e = camera.initPos - v1;/*new Vector3(
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3EndX"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3EndY"]),
                    float.Parse((string)((Hashtable)parameters["UpdateReady"])["cameraPosition3EndZ"])
                );*/

                double t = time.TotalSeconds - dt1 - dt2;
                t /= dt3;
                e -= s;
                s += new Vector3(e.X * (float)t, e.Y * (float)t, e.Z * (float)t);
 
                camera.Position = v1 + s;
            }
            else
            {
                time = TimeSpan.Zero;
                ui = UpdateIndex.FIGHT;
            }
            camera.Flush();

            return true;
        }
        #endregion
        #region UpdateFight
        int winner = -1;

        public void GameSet(int winner)
        {
            ui = UpdateIndex.RESULT;
            this.winner = winner;
            time = TimeSpan.Zero;
        }

        Cue pause;
        public bool UpdateFight(GameTime gameTime)
        {
            if (input.isDown(BUTTON.START))
            {
                time = TimeSpan.Zero; 
                ui = UpdateIndex.PAUSE;
                pause = GLOBAL.soundBank.GetCue("pause");
                pause.Play();
                return true;
            }

            if (time <= gameTime.ElapsedGameTime)
            {
                dt1 = double.Parse((string)((Hashtable)parameters["UpdateFight"])["dt1"]);
            }

            // エンティティの更新
            /*
            for (int i = 0; i < character.Length; i++)
            {
                character[i].Update(gameTime);
            }
             * */
            for (int i = 0; i < entities.Count; i++)
            {
                bool res = ((Entity)entities[i]).Update(gameTime);
                if (res == false)
                {
                    attackerList.Remove(entities[i]);
                    solidList.Remove(entities[i]);
                    entities.RemoveAt(i);
                    i--;
                }
            }
            camera.Update(gameTime);

            return true;
        }
        #endregion
        #region UpdatePause

        double[] offset = new double[]{ 400, 400, 400, 400 };
        int cursor = 0;
        double cursor_time = 0.0;
        const int MAX_CURSOR = 3;
        int flg = -1;

        Cue scursor;
        public bool UpdatePause(GameTime gameTime)
        {
            if (flg != 0)
            {
                double v = double.Parse((string)((Hashtable)parameters["UpdatePause"])["v"]);
                for (int i = 0; i < offset.Length; i++)
                {
                    offset[i] += flg * v * gameTime.ElapsedGameTime.TotalSeconds;
                }
                if (flg < 0)
                {
                    if (offset[3] <= 0)
                    {
                        for (int i = 0; i < offset.Length; i++)
                        {
                            offset[i] = 0;
                        }
                        flg = 0;
                    }
                }
                else
                {
                    if (offset[3] > 400)
                    {
                        for (int i = 0; i < offset.Length; i++)
                        {
                            offset[i] = 400;
                        }
                        switch (cursor)
                        {
                            case 0:
                                ui = UpdateIndex.FIGHT;
                                break;
                            case 1:
                                nextScene = new Loading(player, stage[5] - '1');
                                break;
                            case 2:
                                nextScene = new Title();
                                break;
                        }
                        flg = -1;
                        time = TimeSpan.Zero;
                        return true;
                    }
                }
            }
            else
            {
                if (input.isDown(BUTTON.A) || input.isDown(BUTTON.START))
                {
                    flg = 1;
                    pause = GLOBAL.soundBank.GetCue(cursor == 0 ? "pause" : "enter");
                    pause.Play();
                }
                if (input.isDown(BUTTON.UP))
                {
                    offset[cursor] = 0;
                    time = TimeSpan.Zero;
                    cursor--;
                    cursor += MAX_CURSOR;
                    cursor %= MAX_CURSOR;
                    scursor = GLOBAL.soundBank.GetCue("cursor");
                    scursor.Play();
                }
                if (input.isDown(BUTTON.DOWN))
                {
                    offset[cursor] = 0;
                    time = TimeSpan.Zero;
                    cursor++;
                    cursor %= MAX_CURSOR;
                    scursor = GLOBAL.soundBank.GetCue("cursor");
                    scursor.Play();
                }

                offset[cursor] =
                    -double.Parse((string)((Hashtable)parameters["UpdatePause"])["A"]) *
                    Math.Cos(double.Parse((string)((Hashtable)parameters["UpdatePause"])["w"]) * time.TotalSeconds);
            }

            return true;
        }
        #endregion
        #region UpdateResult

        Vector3 startPos = Vector3.Zero, endPos = Vector3.Zero;
        Vector3 startTarget = Vector3.Zero, endTarget = Vector3.Zero;
        double startAngle = 0, endAngle = 1, angleV;
        double startR = 1, endR = 2;
        double angle;

        public bool UpdateResult(GameTime gameTime)
        {
            if (time <= gameTime.ElapsedGameTime)
            {
                dt1 = double.Parse((string)((Hashtable)parameters["UpdateResult"])["dt1"]); // カメラの移動時間
                dt2 = double.Parse((string)((Hashtable)parameters["UpdateResult"])["dt2"]); // カメラが移動し終わってからキー入力を受け付け始めるまでの時間

                startPos = camera.Position;
                startTarget = camera.Target;
                if (winner == 0)
                {
                    startPos = camera.Position - new Vector3(Character1P.RecVisible.Center, 0);
                    endTarget = new Vector3(Character1P.RecVisible.Center, 0);
                }
                else
                {
                    startPos = camera.Position - new Vector3(Character2P.RecVisible.Center, 0);
                    endTarget = new Vector3(Character2P.RecVisible.Center, 0);
                }
                endPos.X = float.Parse((string)((Hashtable)parameters["UpdateResult"])["endPosX"]);
                endPos.Y = float.Parse((string)((Hashtable)parameters["UpdateResult"])["endPosY"]);
                endPos.Z = float.Parse((string)((Hashtable)parameters["UpdateResult"])["endPosZ"]);

                startR = startPos.Length();
                endR = endPos.Length();

                startAngle = Math.Atan2(startPos.X, startPos.Z);
                endAngle = Math.Atan2(startPos.X, startPos.Z);

                angleV = endAngle - startAngle;
                if (Math.Abs(angleV) < MathHelper.Pi)
                {
                    angleV = angleV - 2 * MathHelper.Pi;
                }
                angleV /= dt1;
                angle = startAngle;
            }
            
            if ( time <= TimeSpan.FromSeconds(dt1))
            {
                double t = time.TotalSeconds / dt1;
                camera.Target = new Vector3(
                    (float)((1 - t) * startTarget.X + t * endTarget.X),
                    (float)((1 - t) * startTarget.X + t * endTarget.Y),
                    (float)((1 - t) * startTarget.X + t * endTarget.Z)
                    );
                angle += angleV * gameTime.ElapsedGameTime.TotalSeconds;
                t = -Math.Cos(MathHelper.Pi * t) + 1;
                t /= 2;
                double r = (1 - t) * startR + t * endR;
                double y = (1 - t) * startPos.Y + t * endPos.Y;
                camera.Position = new Vector3((float)(r * Math.Sin(angle)), (float)y, (float)(r * Math.Cos(angle)));

                if (winner == 0)
                {
                    camera.Position += new Vector3(Character1P.RecVisible.Center, 0);
                }
                else
                {
                    camera.Position += new Vector3(Character2P.RecVisible.Center, 0);
                }
            }
            else if (time <= TimeSpan.FromSeconds(dt1 + dt2))
            {

            }
            else
            {
                if (input.isDown(BUTTON.START))
                {
                    nextScene = new Title();
                }
            }

            camera.Flush();

            return true;
        }
        #endregion

        VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[4];

        Color c1 = Color.White;
        Color c2 = Color.White;

        SpriteFont gageFont;

        Texture2D imTexts;
        Texture2D imBG;

        public void Draw()
        {
            GLOBAL.spriteBatch.Begin();
            GLOBAL.spriteBatch.Draw(imBG, new Rectangle(0, 0, 800, 600), Color.White);
            GLOBAL.spriteBatch.End();
            GLOBAL.game1.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // エンティティの描画
            for (int i = 0; i < character.Length/2; i++)
            {
                character[i].Draw();
            }

            for (int i = 0; i < entities.Count; i++)
            {
                ((Entity)entities[i]).Draw();
            }

            string s;
            Vector2 v;
            switch(ui)
            {
                case UpdateIndex.READY:
                    GLOBAL.spriteBatch.Begin();
                    if (TimeSpan.FromSeconds(dt1 + dt2) <= time )
                    {
                        GLOBAL.spriteBatch.Draw(imTexts, new Rectangle(0, 225, 800, 150), new Rectangle(0, 0, 800, 150), Color.White);
                    }
                    GLOBAL.spriteBatch.End();
                    break;
                case UpdateIndex.FIGHT:
                    GLOBAL.spriteBatch.Begin();
                    int start = 115, wid = 282;
                    int gage = character[0].Gage;
                    double maxGage = (int)character[0].MAX_GAGE;
                    if (gage >= (int)maxGage)
                    {
                        c1 = new Color(
                            255, 
                            255, 
                            Math.Max(0,c1.B - 4), 
                            255);
                    }
                    else
                    {
                        c1 = new Color(255, 255, 255, 255);
                    }
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, new Rectangle(start, 540, (int)(wid * gage / maxGage), 45), c1);

                    start = 685;
                    wid = 279;
                    gage = character[1].Gage;
                    if (gage >= (int)maxGage)
                    {
                        c2 = new Color(
                            255,
                            255,
                            Math.Max(0, c2.B - 4), 
                            255);
                    }
                    else
                    {
                        c2 = new Color(255, 255, 255, 255);
                    }
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, new Rectangle(start - (int)(wid * gage / maxGage), 540, (int)(wid * gage / maxGage), 43), c2);
                    GLOBAL.spriteBatch.Draw(imGage, new Rectangle(0, 500, 800, 100), new Color(255,255,255,255));
                    

                    int damage = (int)character[0].Damage;
                    v = gageFont.MeasureString(damage + "%");
                    float x = 60 - v.X / 2;
                    float y = 550 - v.Y / 2;
                    GLOBAL.spriteBatch.DrawString(gageFont, damage + "%", new Vector2(x, y), Color.Black);
                    damage = (int)character[1].Damage;
                    v = gageFont.MeasureString(damage + "%");
                    x = 740 - v.X / 2;
                    y = 550 - v.Y / 2;
                    GLOBAL.spriteBatch.DrawString(gageFont, damage + "%", new Vector2(x, y), Color.Black);

                    for (int i = 0; i < character[0].life; i++)
                    {
                        Texture2D image = player[0].character == CHARACTER.CUBE ? imCube : imSphere;
                        Rectangle r = new Rectangle();
                        r.X = 130 + 40 * i;
                        r.Y = 510;
                        r.Width = 40;
                        r.Height = (int)(r.Width * image.Height / (double)image.Width);
                        GLOBAL.spriteBatch.Draw(image, r, Color.Red);
                    }

                    for (int i = 0; i < character[1].life; i++)
                    {
                        Texture2D image = player[1].character == CHARACTER.CUBE ? imCube : imSphere;
                        Rectangle r = new Rectangle();
                        r.X = 630 - 40 * i;
                        r.Y = 510;
                        r.Width = 40;
                        r.Height = (int)(r.Width * image.Height / (double)image.Width);
                        GLOBAL.spriteBatch.Draw(image, r, Color.Blue);
                    }

                    if (time < TimeSpan.FromSeconds(dt1))
                    {
                        GLOBAL.spriteBatch.Draw(imTexts, new Rectangle(0, 225, 800, 150), new Rectangle(0, 150, 800, 150), Color.White);
                    }
                    
                    GLOBAL.spriteBatch.End();
                    break;
                case UpdateIndex.PAUSE:
                    GLOBAL.spriteBatch.Begin();
                    GLOBAL.spriteBatch.Draw(GLOBAL.imWhite, new Rectangle(0, 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), new Color(0, 0, 0, 128));
                    GLOBAL.spriteBatch.Draw(imScreen, new Rectangle((int)offset[3], 0, GLOBAL.WindowWidth, GLOBAL.WindowHeight), Color.White);
                    int w = imButton.Width;
                    int h = imButton.Height / 3;
                    for (int i = 0; i < 3; i++)
                    {
                        GLOBAL.spriteBatch.Draw(imButton,
                            new Rectangle(800 - w + 20 + (int)offset[i], 600 - 50 + (i - 3) * h, w, h),
                            new Rectangle(0, i * h, w, h),
                            Color.White);
                    }
                    s = "";
                    switch (cursor)
                    {
                        case 0: s = "対戦を再開します";
                            break;
                        case 1: s = "対戦をやりなおします";
                            break;
                        case 2: s = "タイトル画面へもどります";
                            break;
                    }
                    if ( flg == 0 )
                        GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, new Vector2(650 - GLOBAL.fontArial.MeasureString(s).X / 2, 300 - GLOBAL.fontArial.MeasureString(s).Y), Color.Black);
                    GLOBAL.spriteBatch.End();
                    break;
                case UpdateIndex.RESULT:
                    GLOBAL.spriteBatch.Begin();
                    if (TimeSpan.FromSeconds(dt1) <= time)
                    {
                        GLOBAL.spriteBatch.Draw(imTexts, new Rectangle(0, 225, 800, 150), new Rectangle(0, 300 + 150 * winner, 800, 150), Color.White);
                    }
                    if (TimeSpan.FromSeconds(dt1 + dt2) <= time)
                    {
                        s = "START(Enter)を押してください";
                        v = GLOBAL.fontArial.MeasureString(s);
                        GLOBAL.spriteBatch.DrawString(GLOBAL.fontArial, s, new Vector2(780, 580) - v, Color.White);
                    }
                    GLOBAL.spriteBatch.End();
                    break;
                default:
                    camera.Position = new Vector3(0, 40 * (float)Math.Tan(MathHelper.ToRadians(15)), 40);
                    camera.Flush();
                    break;
            }            
        }
    }
}
