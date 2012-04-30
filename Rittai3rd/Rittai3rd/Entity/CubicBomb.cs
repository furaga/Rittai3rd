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
    public class CubicBomb : Entity
    {
        double minSize;
        double maxSize;
        double expandSpeed;
        Vector3 minColor, maxColor, dColor, color;
        Vector2 velocity;
        double speed;
        TimeSpan time;
        RectangleD rect;
        int owner;
        double attackPower;
        double attackDamage;
        double attackTime;

        public CubicBomb(Fight scene, Hashtable parameters, RectangleD rect, int owner)
            : base(scene, parameters)
        {
            this.rect = rect;
            this.owner = owner;
            Initialize();
        }

        public void Initialize()
        {
            Object o;
            minSize = double.Parse((o = parameters["minSize"]) != null ? (string)o : "0.2");
            maxSize = double.Parse((o = parameters["maxSize"]) != null ? (string)o : "0.2");
            double expandTime = double.Parse((o = parameters["expandTime"]) != null ? (string)o : "0.2");
            expandSpeed = (maxSize - minSize) / expandTime;
            double explosionTime = double.Parse((o = parameters["explosionTime"]) != null ? (string)o : "0.2");
            minColor = new Vector3(
                float.Parse((o = parameters["minColorX"]) != null ? (string)o : "1"),
                float.Parse((o = parameters["minColorY"]) != null ? (string)o : "1"),
                float.Parse((o = parameters["minColorZ"]) != null ? (string)o : "1")
                );
            maxColor = new Vector3(
               float.Parse((o = parameters["maxColorX"]) != null ? (string)o : "1"),
               float.Parse((o = parameters["maxColorY"]) != null ? (string)o : "1"),
               float.Parse((o = parameters["maxColorZ"]) != null ? (string)o : "1")
               );
            dColor = new Vector3(
                (float)((maxColor.X - minColor.X) / explosionTime),
                (float)((maxColor.Y - minColor.Y) / explosionTime),
                (float)((maxColor.Z - minColor.Z) / explosionTime)
                );
            color = minColor;
            speed = double.Parse((o = parameters["speed"]) != null ? (string)o : "5");

            velocity = Vector2.Zero;
            time = TimeSpan.Zero;

            MakeModel("");

            double ex = minSize / rect.Width;
            double ey = minSize / rect.Height;
            RectangleD.ExtendRect(rect, ex, ey, RecVisible);
            Depth = RecVisible.Width;
            RecCollision = RecVisible;

            attackPower = double.Parse((o = parameters["attackPower"]) != null ? (string)o : "0.2");
            attackDamage = double.Parse((o = parameters["attackDamage"]) != null ? (string)o : "0.2");
            attackMinSizeX = double.Parse((o = parameters["attackMinSizeX"]) != null ? (string)o : "1");
            attackMinSizeY = double.Parse((o = parameters["attackMinSizeY"]) != null ? (string)o : "1");
            attackMaxSizeX = double.Parse((o = parameters["attackMaxSizeX"]) != null ? (string)o : "1");
            attackMaxSizeY = double.Parse((o = parameters["attackMaxSizeY"]) != null ? (string)o : "1");
            attackTime = double.Parse((o = parameters["attackTime"]) != null ? (string)o : "1");
            attackExpandSpeedX = (attackMaxSizeX - attackMinSizeX) / attackTime;
            attackExpandSpeedY = (attackMaxSizeY - attackMinSizeY) / attackTime;
            
            EmissiveColor = minColor;
        }

        Entity tmp = null;
        public override bool Update(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            bool flg = true;

            if (RecVisible.Width < maxSize)
            {
                double ex = Math.Min(maxSize, RecVisible.Width + expandSpeed * dt) / RecVisible.Width;
                RectangleD.ExtendRect(RecVisible, ex, ex, RecVisible);
                time = TimeSpan.Zero;
                Chase();
                flg = false;
            }
            else if (color != maxColor)
            {
                color = new Vector3(
                    (float)Math.Min(color.X + dColor.X * dt, 1),
                    (float)Math.Min(color.Y + dColor.Y * dt, 1),
                    (float)Math.Min(color.Z + dColor.Z * dt, 1)
                    );
                EmissiveColor = color;

                if (velocity == Vector2.Zero)
                {
                    flg = false;
                    Hashtable ht = Collision.GetColState(this, scene.AttackerList);
                    foreach (DictionaryEntry de in ht)
                    {
                        Entity he = (Entity)(de.Key);
                        if (he.GetType() == typeof(AttackArea) && ((AttackArea)he).IsTarget[owner] == true)
                        {
                            flg = true;
                            break;
                        }
                        else
                        {
                            velocity = RecCollision.Center - he.RecCollision.Center;
                            if (velocity == Vector2.Zero)
                            {
                                velocity.X = 1;
                            }
                            velocity.Normalize();
                            velocity.X *= (float)speed;
                            velocity.Y *= (float)speed;
                            tmp = he;
                            Cue c = GLOBAL.soundBank.GetCue("B2");
                            c.Play();
                            break;
                        }
                    }
                }
                else
                {
                    RecVisible.Offset(velocity.X * dt, velocity.Y * dt);
                    flg = false;
                    ArrayList ls = new ArrayList(scene.SolidList);
                    if (owner != 0) ls.Add(scene.Character1P);
                    if (owner != 1) ls.Add(scene.Character2P);
                    Hashtable ht = Collision.GetColState(this, ls);
                    foreach (DictionaryEntry de in ht)
                    {
                        velocity = Vector2.Zero;
                        flg = true;
                        break;
                    }
                    if (flg == false)
                    {
                        ht = Collision.GetColState(this, scene.AttackerList);
                        foreach (DictionaryEntry de in ht)
                        {
                            if (de.Key == tmp)
                            {
                                continue;
                            }
                            velocity = Vector2.Zero;
                            flg = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                flg = true;
            }

            Depth = RecVisible.Width;

            if (flg)
            {
                Object o = scene.Parameters["AttackArea"];
                AttackArea attackArea = null;
                if (o != null)
                {
                    Vector2 v = RecVisible.Center;
                    attackArea = new AttackArea(scene, (Hashtable)o, new RectangleD(v.X - attackMinSizeX * 0.5, v.Y - attackMinSizeY * 0.5, attackMinSizeX, attackMinSizeY));
                    attackArea.NotAttack(owner);
                    attackArea.SetExpandSpeed(attackExpandSpeedX, attackExpandSpeedY);
                    attackArea.SetTime(attackTime);
                    attackArea.Power = attackPower;
                    attackArea.Damage = attackDamage;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                    
                    Cue sound = GLOBAL.soundBank.GetCue("bomb");
                    sound.Play();
                }

                scene.EntityList.Add(new ParticleManager(scene, "ExplosionParticles", RecCollision.Center));
                
                Dispose();
            }
            
            return base.Update(gameTime);
        }

        double attackMinSizeX, attackMinSizeY;
        double attackMaxSizeX, attackMaxSizeY;
        double attackExpandSpeedX = 0;
        double attackExpandSpeedY = 0;
    }
}
