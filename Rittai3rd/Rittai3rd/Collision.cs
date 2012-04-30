using System;
using System.Collections;
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
    public class ColState
    {
        public bool Left, Right, Top, Bottom, Top3D, Bottom3D;
        public ColState(bool Left, bool Right, bool Top, bool Bottom)
        {
            this.Left = Left;
            this.Right = Right;
            this.Top = this.Bottom3D = Top;
            this.Bottom = this.Top3D = Bottom;
        }
    }

    public class Collision
    {
        static public bool Intersect( RectangleD a, RectangleD b)
        {           

            if (a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top)
            {
                return true;
            }
            return false;
        }

        static public Hashtable GetColState(Entity me, ArrayList them)
        {
            Hashtable ht = new Hashtable();
            foreach (Object o in them)
            {
                if ( o == null ) continue;
                Entity he = (Entity)o;
                if (me == he) continue;
                RectangleD recMe = me.RecCollision;
                RectangleD recHe = he.RecCollision;

                Object obj = GetColState(recMe, recHe);
                if ( obj != null )
                {
                    ht.Add(o, obj);
                }
            }

            return ht;
        }

        public static Object GetColState(RectangleD recMe, RectangleD recHe)
        {
            bool left = false, right = false, top = false, bottom = false;

            if (recMe.Equal(RectangleD.Empty) || recHe.Equal(RectangleD.Empty)) return null;

            // x軸方向に重なってるとき
            // topとbottomを調べる
            // Heightが正なら、topはbottomより小さい。
            if (recMe.Left < recHe.Right && recMe.Right > recHe.Left)
            {
                if (recHe.Top < recMe.Top && recMe.Top < recHe.Bottom)
                {
                    top = true;
                }
                if (recHe.Top < recMe.Bottom && recMe.Bottom < recHe.Bottom)
                {
                    bottom = true;
                }
            }

            // y軸方向に重なってるとき
            // leftとrightを調べる
            // Heightが正なら、topはbottomより小さい。
            if (recMe.Top < recHe.Bottom && recMe.Bottom > recHe.Top)
            {
                if (recHe.Left < recMe.Left && recMe.Left < recHe.Right)
                {
                    left = true;
                }
                if (recHe.Left < recMe.Right && recMe.Right < recHe.Right)
                {
                    right = true;
                }
            }

            if (top || bottom || left || right)
            {
                return new ColState(left, right, top, bottom);
            }

            return null;
        }
    }
}
