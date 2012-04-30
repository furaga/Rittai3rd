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
    public class Shield : Entity
    {
        ModelInfo mi;
        RectangleD normalRectangle;
        DevType[] dev;
        double expandSpeed = 0;
        int owner;
        Hashtable prevHit = null;

        public Shield(Fight scene, Hashtable parameters, RectangleD rect, int owner, DevType[] dev)
            : base(scene, parameters)
        {
            this.owner = owner;
            this.dev = dev;
            MakeModel("");
            RecVisible.Copy(rect);
            RecCollision = RecVisible;
        }

        public void Initialize()
        {
            Object o;
            prevHit = null;

            t1 = double.Parse((o = parameters["expandTime"]) != null ? (string)o : "128");
            maxH = double.Parse((o = parameters["maxH"]) != null ? (string)o : "128");
            expandSpeed = maxH / t1;
            maxW = double.Parse((o = parameters["maxW"]) != null ? (string)o : "128");
            maxDamage = double.Parse((o = parameters["maxDamage"]) != null ? (string)o : "128");
            maxTime = double.Parse((o = parameters["maxTime"]) != null ? (string)o : "128");

            o = model["model"];
            if (o != null)
            {
                mi = (ModelInfo)o;
                mi.emissiveColor = new Vector3(
                    float.Parse((o = parameters["emissiveColorX"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["emissiveColorY"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["emissiveColorZ"]) != null ? (string)o : "1.0")
                    );
                mi.addAlpha = int.Parse((o = parameters["addAlpha"]) != null ? (string)o : "128");
                double x = RecVisible.Center.X;
                double y = RecVisible.Center.Y;
                mi.recVisible = new RectangleD(x - maxW * 0.5, y - maxH * 0.5, maxW, 0);
                RecCollision = RecVisible;
            }
            flg = true;
        }
        TimeSpan time;
        double t1, t2, t3;
        double maxH, maxW;
        double maxDamage, maxTime;
        bool flg = true;

        public override bool Update(GameTime gameTime)
        {           
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (flg && RecVisible.Height < maxH)
            {
                RecVisible.Top3D += expandSpeed * dt;
                RecVisible.Top3D = Math.Min(RecVisible.Top3D, RecVisible.Bottom3D + maxH);
                if (RecVisible.Height >= maxH)
                {
                    flg = false;
                }
            }
            else
            {
                if (RecVisible.Width > 0.01)
                {

                    double dh = 0, dw = 0;
                    double ex, ey;
                    Hashtable ht = Collision.GetColState(this, scene.AttackerList);
                    foreach (DictionaryEntry de in ht)
                    {
                        if (prevHit != null)
                        {
                            if (prevHit.Contains(de.Key))
                            {
                                continue;
                            }
                        }
                        AttackArea he = (AttackArea)(de.Key);
                        if (he.IsTarget[owner] == false)
                        {
                            continue;
                        }
                        dh = maxH * he.Damage / maxDamage;
                        dw = maxW * he.Damage / maxDamage;
                    }
                    prevHit = ht;

                    dh += maxH * dt / maxTime;
                    dw += maxW * dt / maxTime;

                    ex = 1 - dw / RecVisible.Width;
                    ey = 1 - dh / RecVisible.Height;

                    ex = Math.Max(ex, 0.01);
                    ey = Math.Max(ey, 0.01);

                    RectangleD.ExtendRect(RecVisible, ex, ey, RecVisible);
                }
            }

            // targetに追従する
            ChaseX();

            return base.Update(gameTime);
        }

        public void ChaseX()
        {
            Object o = model["model"];
            if (o == null)
            {
                return;
            }

            ModelInfo mi = (ModelInfo)o;

            if (target != null)
            {
                Vector2 v = target.RecCollision.Center;
                RecVisible.Locate(v.X - RecCollision.Width * 0.5f, RecCollision.Y);
            }
        }

    }
}
