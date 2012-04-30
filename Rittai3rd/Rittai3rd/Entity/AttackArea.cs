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
    public class AttackArea : Entity
    {
        ModelInfo mi;
        RectangleD normalRectangle;

        public AttackArea(Fight scene, Hashtable parameters, RectangleD rect)
            : base(scene, parameters)
        {
            model.Add("model", new ModelInfo("", Vector3.Zero, RectangleD.Empty, 0, rect, Vector3.Zero, Vector3.Zero));
            normalRectangle = new RectangleD();
            normalRectangle.Copy(RecCollision);
            Object o = model["recCollision"];
            if (o != null)
            {
                mi = (ModelInfo)o;
                mi.emissiveColor = new Vector3(1, 0, 1);
            }
            force = Vector2.Zero;
            damage = 0;
            expandSpeedX = expandSpeedY = 0;
            w = rect.Width;
            h = rect.Height;
        }

        TimeSpan time;
        double timeLimit = 0;
        double w, h;

        public bool manual = false;

        public override bool Update(GameTime gameTime)
        {            
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (expandSpeedX != 0 && expandSpeedY != 0)
            {
                w += expandSpeedX * dt;
                h += expandSpeedY * dt;
            }
            if (timeLimit > 0)
            {
                time += gameTime.ElapsedGameTime;
                if (time >= TimeSpan.FromSeconds(timeLimit))
                {
                    return false;
                }
            }
            Object o = model["model"];
            if (o == null)
            {
                return true;
            }
            mi = (ModelInfo)o;

            Vector2 c = normalRectangle.Center;
            if (manual == false) mi.recCollision = new RectangleD(c.X - w / 2, c.Y - h / 2, w, h);

            // targetに追従する
            Chase();

            return base.Update(gameTime);
        }
                
        public void SetTime(double time)
        {
            timeLimit = time;
        }

        double eX = 1, eY = 1;

        public void SetExpandSpeed(double ex, double ey)
        {
            expandSpeedX = ex;
            expandSpeedY = ey;
        }

        double expandSpeedX = 0;
        double expandSpeedY = 0;

        bool[] isTarget = new bool[] { true, true };
        public bool[] IsTarget
        {
            get
            {
                return isTarget;
            }
        }

        public void NotAttack(int id)
        {
            isTarget[id] = false;
        }

        Vector2 force;
        double damage = 0, power;
        public double Damage
        {
            get
            {
                return damage;
            }
            set
            {
                damage = value;
            }
        }
        public Vector2 Force
        {
            get
            {
                return force;
            }
            set
            {
                force = value;
            }
        }
        public double Power
        {
            get
            {
                return power;
            }
            set
            {
                power = value;
            }
        }
    }
}
