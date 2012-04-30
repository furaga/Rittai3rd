using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace リッタイver3
{
    class MyParticleSystem : ParticleSystem
    {
        public MyParticleSystem(Game game, ContentManager content, Hashtable parameters)
            : base(game, content, parameters)
        { }
    }
}
