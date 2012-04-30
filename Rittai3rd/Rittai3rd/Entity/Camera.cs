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
    public class Camera : Entity
    {
        Vector3 cameraPosition, cameraTarget, cameraUpVector;
        float fieldOfView, aspectRatio, nearPlaneDistanse, farPlaneDistance;

        double maxX;
        double maxY;
        double minZ, maxZ;

        Matrix view, projection;
        Character[] character;

        public Vector3 Position
        {
            get
            {
                return cameraPosition;
            }
            set
            {
                cameraPosition = value;
            }
        }
        public Vector3 Target
        {
            get
            {
                return cameraTarget;
            }
            set
            {
                cameraTarget = value;
            }
        }
        public Matrix View
        {
            get
            {
                return view;
            }
        }
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        public Camera(Fight scene, Hashtable parameters)
            : base(scene, parameters)
        {
            Initialize();
        }

        double angle;
        public RectangleD aliveZone;

        void Initialize()
        {
            character = new Character[2];
            character[0] = scene.Character1P;
            character[1] = scene.Character2P;
            
            Vector2 v = character[0].RecVisible.Center;
            v += character[1].RecVisible.Center;
            v.X *= 0.5f;
            v.Y *= 0.5f;
            cameraTarget = new Vector3(v, 0.0f);
            
            angle = double.Parse((string)(parameters["angle"]));            
            
            fieldOfView = float.Parse((string)(parameters["fieldOfView"]));
            
            aspectRatio = (float)GLOBAL.game1.GraphicsDevice.Viewport.Width / GLOBAL.game1.GraphicsDevice.Viewport.Height;
            
            nearPlaneDistanse = float.Parse((string)(parameters["nearPlaneDistanse"]));
            
            farPlaneDistance = float.Parse((string)(parameters["farPlaneDistance"]));

            aliveZone = new RectangleD();

            Object o = scene.Parameters[scene.Stage];
            if ( o != null )
            {
                Hashtable param = (Hashtable)o;                    
                aliveZone.Y = double.Parse((o = param["aliveZoneY"]) != null ? (string)o : "0");
                aliveZone.Height = double.Parse((o = param["aliveZoneH"]) != null ? (string)o : "0");
                aliveZone.Width = aliveZone.Height * GLOBAL.WindowWidth / GLOBAL.WindowHeight;
                aliveZone.X = -0.5 * aliveZone.Width;
                param["aliveZoneW"] = "" + aliveZone.Width;
                param["aliveZoneX"] = "" + aliveZone.X;
            }
            
            minZ = float.Parse((string)(parameters["minZ"]));
            double t1 = fieldOfView * 0.5 - angle;
            double t2 = fieldOfView * 0.5 + angle;
            maxZ = aliveZone.Height / (Math.Tan(MathHelper.ToRadians((float)t1)) + Math.Tan(MathHelper.ToRadians((float)t2)));
            maxX = 0;
            maxY = aliveZone.Top3D - maxZ * Math.Tan((MathHelper.ToRadians((float)t1)));
            maxY = aliveZone.Bottom3D + maxZ * Math.Tan((MathHelper.ToRadians((float)t2)));

            cameraUpVector = new Vector3(
                float.Parse((string)(parameters["cameraUpVectorX"])),
                float.Parse((string)(parameters["cameraUpVectorY"])),
                float.Parse((string)(parameters["cameraUpVectorZ"]))
                );

            RectangleD rec1, rec2;
            rec1 = character[0].RecVisible;
            rec2 = character[1].RecVisible;

            // カメラのZ座標
            double t = Math.Max(Math.Abs(rec1.X - rec2.X) / aliveZone.Width, Math.Abs(rec1.Y - rec2.Y) / aliveZone.Height);
            double z = Math.Max(minZ, maxZ * 2 * t);

            // カメラの注視点と位置
            RectangleD rect3 = new RectangleD(
                aliveZone.X * (1 - t),
                aliveZone.Y * (1 - t) + t * maxY,
                aliveZone.Width * (1 - t),
                aliveZone.Height * (1 - t)
                );

            rect3 = RectangleD.ExtendRect(rect3, 2, 2);

            double dy = z * Math.Tan(MathHelper.ToRadians((float)angle));
            rect3.Offset(0, -dy);

            v = rec1.Center;
            v += rec2.Center;
            v.X *= 0.5f; v.X = (float)Math.Max(v.X, rect3.Left); v.X = (float)Math.Min(v.X, rect3.Right);
            v.Y *= 0.5f; v.Y = (float)Math.Max(v.Y, rect3.Bottom3D); v.Y = (float)Math.Min(v.Y, rect3.Top3D);
            Position = initTarget = new Vector3(v, 0.0f);
            Target = initPos = new Vector3(cameraTarget.X, cameraTarget.Y + (float)dy, (float)z); 

            Flush();
        }

        public Vector3 initPos, initTarget;

        public override bool Update(GameTime gameTime)
        {
            RectangleD rec1, rec2;
            rec1 = character[0].RecVisible;
            rec2 = character[1].RecVisible;
            
            // カメラのZ座標
            double t = Math.Max(Math.Abs(rec1.X - rec2.X) / aliveZone.Width, Math.Abs(rec1.Y - rec2.Y) / aliveZone.Height);
            double z = Math.Max(minZ, maxZ * 2 * t);
            
            // カメラの注視点と位置
            RectangleD rect3 = new RectangleD(
                aliveZone.X * (1 - t),
                aliveZone.Y * (1 - t) + t * maxY,
                aliveZone.Width * (1 - t),
                aliveZone.Height * (1 -t)
                );

            rect3 = RectangleD.ExtendRect(rect3, 2,2);

            double dy = z * Math.Tan(MathHelper.ToRadians((float)angle));
            rect3.Offset(0, -dy);

            Vector2 v = rec1.Center;
            v += rec2.Center;
            v.X *= 0.5f; v.X = (float)Math.Max(v.X, rect3.Left); v.X = (float)Math.Min(v.X, rect3.Right);
            v.Y *= 0.5f; v.Y = (float)Math.Max(v.Y, rect3.Bottom3D); v.Y = (float)Math.Min(v.Y, rect3.Top3D);
            cameraTarget = new Vector3(v, 0.0f);

            cameraPosition = new Vector3(cameraTarget.X, cameraTarget.Y + (float)dy, (float)z); 

            Flush();
            return true;
        }

        /// <summary>
        /// カメラの位置などの変更を反映させる
        /// </summary>
        public void Flush()
        {
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, nearPlaneDistanse, farPlaneDistance);
        }

        public override void Draw()
        {
            // なにもしない
        }
    }
}
