#define _DEBUG

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

    class GLOBAL
    {
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static SpriteFont fontArial;
        public static Debug debug;
        public static Scene scene;
        public static Game game1;
        public static int WindowHeight = 600;
        public static int WindowWidth = 800;
        public static InputManager inputManager;
        public static Texture2D imWhite;
        public static Vector3 vec3Zero = Vector3.Zero, vec3AllOne = new Vector3(1.0f, 1.0f, 1.0f);
        public static ParticleSystem explosionParticles;
        public static ParticleSystem attackedParticles;
        public static ParticleSystem deadParticles;
        public static ParticleSystem smokeParticles;
        public static AudioEngine engine;
        public static SoundBank soundBank;
        public static WaveBank waveBank;
        public static Cue bgm;
    }
}
