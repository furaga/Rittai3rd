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
    /// <summary>
    /// CPUの動きの計算
    /// </summary>
    public partial class Character : Entity
    {
        Character enemy = null;
        TimeSpan cpuTime = TimeSpan.Zero;
        double cpuNextTime = 0;
        Random rand = new Random();
        double cpuMinX, cpuMaxX;

        void InitializeCPU()
        {
            cpuTime = TimeSpan.Zero;
            enemy = null;
            rand = new Random((int)(DateTime.Now.Ticks));
            cpuNextTime = rand.NextDouble() * 3;

            Object o = scene.Parameters[scene.Stage];
            if (o != null)
            {
                Hashtable ht = (Hashtable)o;
                Object o1;
                cpuMinX = double.Parse((o1 = ht["cpuMinX"]) != null ? (string)o1 : "-10");
                cpuMaxX = double.Parse((o1 = ht["cpuMaxX"]) != null ? (string)o1 : "10");
            }
        }

        void cpuInput(GameTime gameTime)
        {
            cpuTime += gameTime.ElapsedGameTime;

            if (player.id == 0) enemy = scene.Character2P;
            if (player.id == 1) enemy = scene.Character1P;

            CPUInputState state = new CPUInputState();

            Vector2 myPos = RecVisible.Center;
            Vector2 hisPos = enemy.RecVisible.Center;
            Vector2 v = -myPos + hisPos;

            if (myPos.X < cpuMinX)
            {
                state.stick.X = 1;
                if (jumpFlg <= 0)
                {
                    state.stick.Y = 1;
                    state.button[(int)BUTTON.LB] = true;
                }
                else
                {
                    if (CheckFloat(gameTime) && velocity.Y < 0)
                    {
                        state.stick.Y = 1;
                        state.button[(int)BUTTON.LB] = true;
                    }
                }
            }
            else if (myPos.X > cpuMaxX)
            {
                state.stick.X = -1;
                if (jumpFlg <= 0)
                {
                    state.stick.Y = 1;
                    state.button[(int)BUTTON.LB] = true;
                }
                else
                {
                    if (CheckFloat(gameTime) && velocity.Y < 0)
                    {
                        state.stick.Y = 1;
                        state.button[(int)BUTTON.LB] = true;
                    }
                }
            }
            else
            {
                if (v.X > 0)
                {
                    state.stick.X = 1;
                }
                else
                {
                    state.stick.X = -1;
                }

                if (v.Y > 0.5)
                {
                    if (jumpFlg <= 0)
                    {
                        state.stick.Y = 1;
                        state.button[(int)BUTTON.LB] = true;
                    }
                    else
                    {
                        if (CheckFloat(gameTime) && velocity.Y < 0)
                        {
                            state.stick.Y = 1;
                            state.button[(int)BUTTON.LB] = true;
                        }
                    }
                }
            }

            if (v.Length() < 2.0f || cpuTime >= TimeSpan.FromSeconds(cpuNextTime))
            {
                if (cpuNextTime >= 2.0 && input.PrevCPU.button[(int)BUTTON.B] == false)
                {
                    state.button[(int)BUTTON.B] = true;
                }
                else if (input.PrevCPU.button[(int)BUTTON.A] == false)
                {
                    state.button[(int)BUTTON.A] = true;
                }
                cpuTime = TimeSpan.Zero;
                cpuNextTime = rand.NextDouble() * 3;
            }

            input.CurCPU = state;
        }
    }
}
