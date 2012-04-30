using System;
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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
  //      GraphicsDeviceManager graphics;
  //      SpriteBatch spriteBatch;
  //      SpriteFont fontArial;
  //      Scene.Scene scene;

        double fps;
        double cnt;
        TimeSpan time;

        public Game1()
        {
            GLOBAL.graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GLOBAL.graphics.PreferredBackBufferWidth = 800;
            GLOBAL.graphics.PreferredBackBufferHeight = 600;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            GLOBAL.game1 = this;
            GLOBAL.debug = new Debug();
            GLOBAL.engine = new AudioEngine("Content\\sounds.xgs");
            GLOBAL.soundBank = new SoundBank(GLOBAL.engine, "Content\\Sound Bank.xsb");
            GLOBAL.waveBank = new WaveBank(GLOBAL.engine, "Content\\Wave Bank.xwb");
            GLOBAL.bgm = GLOBAL.soundBank.GetCue("bgm1");
            GLOBAL.bgm.Play();
            GLOBAL.bgm.Pause();
            GLOBAL.scene = new Title();
            GLOBAL.inputManager = new InputManager();
            fps = 60.0;
            cnt = 0;
            time = TimeSpan.Zero;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            GLOBAL.spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            GLOBAL.fontArial = Content.Load<SpriteFont>("Arial");
            GLOBAL.imWhite = Content.Load<Texture2D>("white");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        bool flg = false;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //               this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F2) && !flg)
            {
                // ウインドウモードとフルスクリーンモードを切り替える
                GLOBAL.graphics.ToggleFullScreen();
            }

            flg = Keyboard.GetState().IsKeyDown(Keys.Escape);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

#if _DEBUG
            GLOBAL.debug.Clear();
#endif
            
            GLOBAL.inputManager.Update(gameTime);
            GLOBAL.scene = GLOBAL.scene.Update(gameTime);
            if (GLOBAL.scene == null) this.Exit();

//            cnt++;
            time += gameTime.ElapsedGameTime;
            if (time.TotalSeconds >= 1.0)
            {
                fps = cnt / time.TotalSeconds;
                cnt = 0.0;
                time = TimeSpan.Zero;
                ccnt++;
            }

            GLOBAL.engine.Update();

            base.Update(gameTime);
        }

        int ccnt = 0;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            cnt++;
            GraphicsDevice.Clear(Color.LightBlue);

            // TODO: Add your drawing code here
            GLOBAL.scene.Draw();
            GLOBAL.debug.Draw();
            Window.Title = "FPS:" + fps + "(" + ccnt + ")";

            base.Draw(gameTime);
        }
    }
}
