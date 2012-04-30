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
    public enum PLAYER_TYPE { PLAYER = 0, CPU };
    public enum CHARACTER { SPHERE = 0, CUBE };
    delegate bool UpdateFuncs(GameTime gameTime);

    public struct PLAYER_INFO
    {
        public int id;
        public PLAYER_TYPE type;
        public CHARACTER character;
        public DevType[] dev;
        public PLAYER_INFO(int i, PLAYER_TYPE t, CHARACTER c, DevType[] _dev = null)
        {
            id = i;
            type = t;
            character = c;
            dev = _dev;
        }
    }

    public interface Scene
    {
        void Initialize();
        Scene Update(GameTime gameTime);
        void Draw();
    }
}
