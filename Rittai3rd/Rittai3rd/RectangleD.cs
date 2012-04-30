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
    public class RectangleD
    {
        double x, y, width, height;
        static public RectangleD Empty
        {
            get
            {
                return new RectangleD(0.0f,0.0f,0.0f,0.0f);
            }
        }
        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
               y = value;
            }
        }
        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        public double Left
        {
            get
            {
                return x;
            }
            set
            {
                width = Right - value;
                x = value;
            }
        }
        public double Right
        {
            get
            {
                return x + width;
            }
            set
            {
                width = value - Left;
            }
        }
        public double Top
        {
            get
            {
                return y;
            }
            set
            {
                height = Bottom - value;
                y = value;
            }
        }
        public double Bottom
        {
            get
            {
                return y + Height;
            }
            set
            {
                height = value - Top;
            }
        }
        // 3D空間だと上下が2D空間の逆になるので、専用のプロパティを作る
        public double Top3D
        {
            get
            {
                return Bottom;
            }
            set
            {
                Bottom = value;
            }
        }
        public double Bottom3D
        {
            get
            {
                return Top;
            }
            set
            {
                Top = value;
            }
        }
        public Vector2 Center
        {
            get
            {
                return new Vector2((float)(x + Width * 0.5f), (float)(y + Height * 0.5f));
            }
        }

        public RectangleD()
        {

        }

        public RectangleD(RectangleD rec)
        {
            x = rec.x;
            y = rec.y;
            width = rec.width;
            height = rec.height;
        }

        public RectangleD(Rectangle rec)
        {
            x = rec.X;
            y = rec.Y;
            width = rec.Width;
            height = rec.Height;
        }

        public RectangleD(double X, double Y, double Width, double Height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        public void Offset(Vector2 offset)
        {
            x += offset.X;
            y += offset.Y;
        }

        public void Offset(double X, double Y)
        {
            x += X;
            y += Y;
        }

        /// <summary>
        /// 同じ矩形を表しているか（各要素が等しいか）
        /// </summary>
        /// <param name="he"></param>
        /// <returns></returns>
        public bool Equal(RectangleD he)
        {
            return X == he.X && Y == he.Y && Width == he.Width && Height == he.Height;
        }

        public void Locate(Vector2 v)
        {
            this.x = v.X;
            this.y = v.Y;
        }

        public void Locate(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 Position()
        {
            return new Vector2((float)x, (float)y);
        }

        /// <summary>
        /// r.Centerを起点として横にex, 縦にeyだけrを伸ばした矩形を返す
        /// </summary>
        /// <param name="r"></param>
        /// <param name="ex"></param>
        /// <param name="ey"></param>
        /// <returns></returns>
        static public RectangleD ExtendRect(RectangleD src, double ex, double ey, RectangleD dist = null)
        {
            double x, y, w, h;
            Vector2 v = src.Center;

            x = v.X - src.Width * 0.5 * ex;
            y = v.Y - src.Height * 0.5 * ey;
            w = src.Width * ex;
            h = src.Height * ey;

            if (dist != null)
            {
                dist.X = x;
                dist.Y = y;
                dist.Width = w;
                dist.Height = h;
            }

            return new RectangleD(x, y, w, h);
        }

        public void Copy(RectangleD rect)
        {
            x = rect.X;
            y = rect.Y;
            height = rect.Height;
            width = rect.Width;
        }
    }
}
