using System;
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

namespace リッタイver3
{

    /// <summary>
    /// ステージのパーツ
    /// </summary>
    class StagePart : Entity
    {
        int id;
        UpdateFuncs funcs;

        public StagePart(Fight scene, Hashtable parameters, int id)
            : base(scene, parameters)
        {
            this.id = id;
            Initialize();
        }

        public void Initialize()
        {
            string str = "Model" + (id + 1);
            MakeModel(str);
            ModelInfo mi = ((ModelInfo)model["model"]);
            {
                mi.recCollision = mi.recVisible;
            }
            model["model"] = mi;

            switch ((string)parameters[str + "_Update"])
            {
                case "Rotate90":
                    break;
                default:
                    funcs = null;
                    break;
            }
        }

        public bool Update(GameTime gameTime)
        {
            bool ans = true;
            if (funcs != null)
            {
                ans = funcs(gameTime);
            }
            return ans;
        }
    }
}
