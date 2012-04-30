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
        #region 各種判定関数

        /// <summary>
        /// bool CheckFunc(GameTime gameTime)
        /// {
        ///    bool ans;
        ///
        ///    // Write Algorithms
        ///
        ///    checkResult &= ans;
        ///    return ans;
        /// }
        /// </summary>

        bool checkResult = false;

        bool __CheckDamage(GameTime gameTime)
        {
            bool ans = false;

            // Write Algorithms
            int cur = cnt_ColStateWithAttacker;
            if (colStateWithSolid[cur] == null) return false;

            foreach (DictionaryEntry de in colStateWithAttacker[cur])
            {
                if (de.Key != null && de.Value != null)
                {
                    ans = true;
                    break;
                }
            }

            return ans;
        }
        bool CheckDamage(GameTime gameTime)
        {
            bool ans;
            ans = __CheckDamage(gameTime);
            checkResult &= ans;
            return ans;
        }
        bool NotCheckDamage(GameTime gameTime)
        {
            bool ans;
            ans = !__CheckDamage(gameTime);
            checkResult &= ans;
            return ans;
        }

        bool __FullGage(GameTime gameTime)
        {
            bool ans = false;

            // Write Algorithms
            if (gage >= MAX_GAGE)
            {
                ans = true;
            }

            return ans;
        }

        bool FullGage(GameTime gameTime)
        {
            bool ans;
            ans = __FullGage(gameTime);
            checkResult &= ans;
            return ans;
        }

        bool NotFullGage(GameTime gameTime)
        {
            bool ans;
            ans = !__FullGage(gameTime);
            checkResult &= ans;
            return ans;
        }

        bool CheckDead(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            // aliveZoneの外にいれば
            Hashtable ht = Collision.GetColState(this, scene.DeadBlockList);
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    ans = true;
                    break;
                }
            }

            checkResult &= ans;
            return ans;
        }

        double damage = 0;
        double brownAngle = 20;
        double minAttackedRatio = 1;
        double maxAttackedRatio = 3;
        double maxDamage = 200;
        bool CheckAttacked(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            int cur = cnt_ColStateWithAttacker;
            int prev = cur - 1 + MAX_COLSTATEWithAttacker;
            prev %= MAX_COLSTATEWithAttacker;
            if (colStateWithAttacker[cur] == null) return false;
            ParticleManager pm;
            foreach (DictionaryEntry de in colStateWithAttacker[cur])
            {
                AttackArea a = (AttackArea)(de.Key);
                if (a.IsTarget[player.id] == false) continue;

                // 今回のフレームで初めてぶつかった
                if (colStateWithAttacker[prev][de.Key] == null)
                {
                    Character he;
                    if (player.id == 0) he = scene.Character2P;
                    else he = scene.Character1P;

                    // キャラクターの直接攻撃は食らう
                    if (attackedAbsorb == true && Collision.Intersect(mi.recCollision, he.RecCollision) == false)
                    {
                        // ダメージを吸収する
                        damage -= a.Damage * 0.5;
                        damage = Math.Max(0, damage);

                        pm = new ParticleManager(scene, "AbsorbParticles", RecVisible.Center);
                        pm.Follow(this);
                        scene.EntityList.Add(pm);

                        colStateWithAttacker[cur].Remove(de);
                        Cue sound = GLOBAL.soundBank.GetCue("absorb");
                        sound.Play();
                        break;
                    }

                    ans = true;

                    Vector2 v;
                    if (a.Force == Vector2.Zero)
                    {
                        v = mi.recCollision.Center - a.RecCollision.Center;
                        if (v.Length() <= 0.00001) v = new Vector2(1, 0); 
                        v.Normalize();
                        v.X *= (float)a.Power;
                        v.Y *= (float)a.Power;
                    }
                    else
                    {
                        v = a.Force;
                    }

                    if (v.X == 0) v.X = 0.0001f;
                    double angle = MathHelper.ToDegrees((float)Math.Atan2(v.Y, v.X));
                    if (0 <= angle && angle < brownAngle)
                    {
                        angle = brownAngle;
                    }
                    else if (-brownAngle < angle && angle < 0)
                    {
                        angle = -brownAngle;
                    }
                    else if (180 - brownAngle < angle && angle <= 180)
                    {
                        angle = 180 - brownAngle;
                    }
                    else if (-180 < angle && angle < -180 + brownAngle)
                    {
                        angle = -180 + brownAngle;
                    }

                    double x = damage / maxDamage;
                    double ratio = minAttackedRatio * (1 - x) + maxAttackedRatio * x;
                    double pow = v.Length() * ratio;
                    velocity += new Vector2(
                        (float)(pow * Math.Cos(MathHelper.ToRadians((float)angle))),
                        (float)(pow * Math.Sin((MathHelper.ToRadians((float)angle)))));

                    damage += a.Damage;
                    damage = Math.Min(damage, maxDamage);

                    x = damage / maxGetDamage;
                    brownTime = minBrownTime * (1 - x) + maxBrownTime * x;

                    pm = new ParticleManager(scene, "AttackedParticles", RecVisible.Center);
                    pm.Follow(this);
                    scene.EntityList.Add(pm);

                    Cue s = GLOBAL.soundBank.GetCue("attacked");
                    s.Play();

                    break;
                }
            }

            checkResult &= ans;
            return ans;
        }
        double maxBrownTime = 1.5, minBrownTime = 0.2, maxGetDamage = 20;

        bool CheckJump(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            if (jumpFlg < MAX_JUMP)
            {
                ans = true;
            }

            checkResult &= ans;
            return ans;
        }

        bool ExistAbove()
        {
            bool ans = false;

            double e = 0.01;

            // ちょっと上がる
            Offset(0, e);

            // 衝突判定
            Hashtable ht = Collision.GetColState(this, scene.SolidList);

            // htが上面で衝突していれば上になにかある
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    if (((ColState)de.Value).Top3D)
                    {
                        ans = true;
                        break;
                    }
                }
            }

            // ちょっと上がった分を戻す
            Offset(0, -e);

            return ans;
        }

        bool ExistUnder()
        {
            bool ans = false;

            double e = -0.01;

            // ちょっと下がる
            Offset(0, e);

            // 衝突判定
            Hashtable ht = Collision.GetColState(this, scene.SolidList);

            // htが下面で衝突していれば下になにかある
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    if (((ColState)de.Value).Bottom3D)
                    {
                        ans = true;
                        break;
                    }
                }
            }

            // ちょっと下がった分を戻す
            Offset(0, -e);

            return ans;
        }

        bool ExistLeft()
        {
            bool ans = false;

            double e = -0.01;

            // ちょっと下がる
            Offset(e, 0);

            // 衝突判定
            Hashtable ht = Collision.GetColState(this, scene.SolidList);

            // htが下面で衝突していれば下になにかある
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    if (((ColState)de.Value).Left)
                    {
                        ans = true;
                        break;
                    }
                }
            }

            // ちょっと下がった分を戻す
            Offset(-e, 0);

            return ans;
        }

        bool ExistRight()
        {
            bool ans = false;

            double e = 0.01;

            // ちょっと下がる
            Offset(e, 0);

            // 衝突判定
            Hashtable ht = Collision.GetColState(this, scene.SolidList);

            // htが下面で衝突していれば下になにかある
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    if (((ColState)de.Value).Right)
                    {
                        ans = true;
                        break;
                    }
                }
            }

            // ちょっと下がった分を戻す
            Offset(-e, 0);

            return ans;
        }

        bool CheckFloat(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            if (!ExistUnder())
            {
                ans = true;
            }

            checkResult &= ans;
            return ans;
        }

        bool CheckClimb(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            if (AdjustToLeft(adjustTo) || AdjustToRight(adjustTo))
            {
                ans = true;
            }

            checkResult &= ans;
            return ans;
        }

        bool CheckGround(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            if (AdjustToBottom3D(adjustTo))
            {
                ans = true;
            }

            checkResult &= ans;
            return ans;
        }

        double checkBreakFallTime = 0.1;
        ColState checkBreakFallColState;

        bool CheckBreakFall(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            double dt = checkBreakFallTime;
            double dx = velocity.X * dt;
            double dy = velocity.Y * dt;

            Offset(dx, dy);

            Hashtable ht = Collision.GetColState(this, scene.SolidList);
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    checkBreakFallColState = (ColState)de.Value;
                    ans = true;
                    break;
                }
            }

            Offset(-dx, -dy);

            checkResult &= ans;
            return ans;
        }

        bool CheckBreakFallR(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            double dt = checkBreakFallTime;
            double dx = velocity.X * dt;
            double dy = velocity.Y * dt;

            Offset(dx, dy);

            Hashtable ht = Collision.GetColState(this, scene.SolidList);
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key != null && de.Value != null)
                {
                    if (((ColState)de.Value).Bottom3D)
                    {
                        ans = true;
                        break;
                    }
                }
            }

            Offset(-dx, -dy);

            checkResult &= ans;
            return ans;
        }

        bool CheckDown(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            // 一定速度内の時に下面で
            if (velocity.LengthSquared() < checkDownSpeedSquared)
            {
                if (AdjustToBottom3D(adjustTo))
                {
                    ans = true;
                }
            }

            checkResult &= ans;
            return ans;
        }

        bool CheckBump(GameTime gameTime)
        {
            bool ans = false;

            // Write your algorithm
            if (AdjustToLeft(adjustTo) || AdjustToRight(adjustTo) || AdjustToTop3D(adjustTo))
            {
                ans = true;
            }
            if (velocity.LengthSquared() >= checkDownSpeedSquared && AdjustToBottom3D(adjustTo))
            {
                ans = true;
            }

            checkResult &= ans;
            return ans;
        }


        #endregion

        #region 状態遷移の補助関数

        class NextAnimation
        {
            public Animation anim;
            public bool useCommand;
            public bool flg;
            public UpdateFuncs checkFuncs;
            public NextAnimation(Animation a, bool u = false, UpdateFuncs c = null, bool f = true)
            {
                anim = a;
                useCommand = u;
                checkFuncs = c;
                flg = f;
            }
        }

        bool[] commandFlg;

        /// <summary>
        /// 1: flgがtrue
        /// 2: useCommandがFALSEか、useCommandがTRUEでcommandState[na[i].anim]がTRUE
        /// 3: checkFuncsがnullか、nullでなくてcheckFuncs(gameTime)の結果checkResultがtrueのなった
        /// １，２，３をすべて満たすとき、na[i].animを返す
        /// </summary>
        Animation FindNextAnimation(NextAnimation[] na, GameTime gameTime)
        {
            if (checkedColState == false) GetColState();
            if (adjustTo < 0 || 3 < adjustTo) Adjust(velocity.X, velocity.Y);

            for (int i = 0; i < na.Length; i++)
            {
                /// 1: flgがtrue
                bool res = na[i].flg;
                if (res == false) continue;

                /// 2: useCommandがFALSEか、useCommandがTRUEでcommandState[na[i].anim]がTRUE
                if (commandFlg[(int)na[i].anim] == false) continue;
                res = na[i].useCommand == false || commandState[(int)na[i].anim];
                if (res == false) continue;

                /// 3: checkFuncsがnullか、nullでなくてcheckFuncs(gameTime)の結果checkResultがtrueのなった
                if (na[i].checkFuncs == null) return na[i].anim;
                checkResult = true;
                na[i].checkFuncs(gameTime);
                if (checkResult) return na[i].anim;
            }
            return Animation.None;
        }
        #endregion

        #region アニメーションの補助関数

        /// <summary>
        /// モデルの移動(＋αの処理あり)
        /// </summary>
        /// <param name="gameTime"></param>
        void Offset(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            double dx = velocity.X * dt;
            double dy = velocity.Y * dt;
            Offset(dx, dy);
        }

        void Offset(double x, double y)
        {
            mi.recVisible.Offset(x, y);
            normalRectangle.Offset(x, y);
        }

        void Locate(double x, double y)
        {
            mi.recVisible.Locate(x, y);
            normalRectangle.Locate(x, y);
        }

        Hashtable[] colStateWithAttacker;
        int cnt_ColStateWithAttacker = 0;
        int MAX_COLSTATEWithAttacker = 60;
        Hashtable[] colStateWithSolid;
        int cnt_ColStateWithSolid = 0;
        int MAX_COLSTATEWithSolid = 60;
        bool checkedColState = false;

        /// <summary>
        /// 衝突情報の更新
        /// colState[entity]の要素がLeft,Right,Top3D,Bottom3Dが真であることはそれぞれ
        /// 「三次元空間において、自分はオブジェクトentityと左側面、右側面、上面、下面で衝突している」といことと同値
        /// </summary>
        void GetColState()
        {
            cnt_ColStateWithAttacker++;
            cnt_ColStateWithAttacker %= MAX_COLSTATEWithAttacker;
            colStateWithAttacker[cnt_ColStateWithAttacker] = Collision.GetColState(this, scene.AttackerList);

            cnt_ColStateWithSolid++;
            cnt_ColStateWithSolid %= MAX_COLSTATEWithSolid;
            colStateWithSolid[cnt_ColStateWithSolid] = Collision.GetColState(this, scene.SolidList);

            checkedColState = true;
        }

        // TODO 初期化
        int[] adjustToHist;
        int cnt_adjustToHist = 0;
        int MAX_ADJUSTTOHIST = 60;
        int adjustTo = -1;
        bool AdjustToLeft(int a)
        {
            return a == 0;
        }
        bool AdjustToRight(int a)
        {
            return a == 1;
        }
        bool AdjustToTop3D(int a)
        {
            return a == 2;
        }
        bool AdjustToBottom3D(int a)
        {
            return a == 3;
        }

        /// <summary>
        /// 物体と衝突していたときの位置の調整
        /// 
        /// TODO:
        /// 着地したときにステージの端に追いやられるバグあり。
        /// flgの値を決める方法がおかしい？
        /// </summary>
        /// <param name="gameTime"></param>
        Vector2 adjustVec;
        int Adjust(double vx, double vy, bool updateAdjustTo = true)
        {
            Hashtable colState = null;

            if (updateAdjustTo)
            {
                if (checkedColState == false)
                {
                    GetColState();
                }
                int cur = cnt_ColStateWithSolid;
                if (colStateWithSolid[cur] == null) return -1;
                colState = colStateWithSolid[cur];
            }
            else
            {
                colState = Collision.GetColState(this, scene.SolidList);
            }

            double dx = 0.0, dy = 0.0;

            int flg = -1;
            adjustVec = Vector2.Zero;

            foreach (DictionaryEntry de in colState)
            {
                Object key = de.Key, value = de.Value;
                if (key == null || value == null) continue;
                Entity he = (Entity)key;
                if (he == shield)
                {
                    continue;
                }
                ColState col = (ColState)value;
                double min = 10000.0, t, t1 = 0;
                RectangleD recMe = mi.recCollision;
                RectangleD recHe = he.RecCollision;
                Vector2 v;
                if (vx * vx + vy * vy <= 0.0f)
                {
                    v = recMe.Center - recHe.Center;
                }
                else
                {
                    v = new Vector2(-(float)vx, -(float)vy);
                }

                if (col.Left && col.Right && col.Top3D && !col.Bottom3D)
                {
                    flg = 2;        // Top3Dを合わせる
                }
                else if (col.Left && col.Right && !col.Top3D && col.Bottom3D)
                {
                    flg = 3;        // Bottom3Dを合わせる
                }
                else if (col.Left && !col.Right && col.Top3D && col.Bottom3D)
                {
                    flg = 0;        // Leftを合わせる
                }
                else if (!col.Left && col.Right && col.Top3D && col.Bottom3D)
                {
                    flg = 1;        // Rightを合わせる
                }
                else if (col.Left && !col.Right && !col.Top3D && !col.Bottom3D)
                {
                    flg = 0;        // Leftを合わせる
                }
                else if (!col.Left && col.Right && !col.Top3D && !col.Bottom3D)
                {
                    flg = 1;        // Rightを合わせる
                }
                else if (!col.Left && !col.Right && col.Top3D && !col.Bottom3D)
                {
                    flg = 2;        // Top3Dを合わせる
                }
                else if (!col.Left && !col.Right && !col.Top3D && col.Bottom3D)
                {
                    flg = 3;        // Bottom3Dを合わせる
                }
                else if (!col.Left && !col.Right && col.Top3D && col.Bottom3D)
                {
                    if (Math.Abs(recMe.Top3D - recHe.Bottom3D) > Math.Abs(recMe.Bottom3D - recHe.Top3D))
                    {
                        flg = 3;
                    }
                    else
                    {
                        flg = 2;
                    }
                }
                else if (col.Left && col.Right && !col.Top3D && !col.Bottom3D)
                {
                    if (Math.Abs(recMe.Left - recHe.Right) > Math.Abs(recMe.Right - recHe.Left))
                    {
                        flg = 0;
                    }
                    else
                    {
                        flg = 1;
                    }
                }
                else
                {
                    if (v.X > 0.0f && col.Left)
                    {
                        if (min > (t = Math.Abs((recMe.Left - recHe.Right) / v.X)))
                        {
                            flg = 0;
                            min = t;
                        }
                    }
                    if (v.X < 0.0f && col.Right)
                    {
                        if (min > (t = Math.Abs((recMe.Right - recHe.Left) / v.X)))
                        {
                            flg = 1;
                            min = t;
                        }
                    }
                    if (v.Y < 0.0f && col.Top3D)
                    {
                        if (min > (t = Math.Abs((recMe.Top3D - recHe.Bottom3D) / v.Y)))
                        {
                            flg = 2;
                            min = t;
                        }
                    }
                    if (v.Y > 0.0f && col.Bottom3D)
                    {
                        if (min > (t = Math.Abs((recMe.Bottom3D - recHe.Top3D) / v.Y)))
                        {
                            flg = 3;
                            min = t;
                        }
                    }
                }

                switch (flg)
                {
                    case 0:     // Left
                        dx = -recMe.Left + recHe.Right;
                        break;
                    case 1:     // Right
                        dx = -recMe.Right + recHe.Left;
                        break;
                    case 2:     // Top3D
                        dy = -recMe.Top3D + recHe.Bottom3D;
                        break;
                    case 3:     // Bottom3D
                        dy = -recMe.Bottom3D + recHe.Top3D;
                        break;
                    default:
                        break;
                }

                if (updateAdjustTo)
                {
                    adjustTo = flg;
                    cnt_adjustToHist++;
                    cnt_adjustToHist %= MAX_ADJUSTTOHIST;
                    adjustToHist[cnt_adjustToHist] = adjustTo;
                }

                if (dx != 0 || dy != 0)
                {
                    adjustVec = new Vector2((float)dx, (float)dy);
                    break;
                }
            }
            
            Offset(adjustVec.X, adjustVec.Y);

            return flg;
        }

        void OffsetWithAdjust(GameTime gameTime)
        {
            Offset(gameTime);
            GetColState();
            Adjust(velocity.X, velocity.Y, true);
        }

        #endregion

        #region 通常動作

        // 1、初期化
        // 2.アニメーション
        // 3、次のアニメーションへの遷移判定
        // 4、アニメーションが遷移するときの処理

        void AnimStop(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // 速度のｙ方向は０に
            velocity.Y = 0;

            // X方向は０なるように加速度をかける
            if (velocity.X > 0)
            {
                velocity.X = (float)Math.Max(0.0, velocity.X - stopAccel * dt);
            }
            if (velocity.X < 0)
            {
                velocity.X = (float)Math.Min(0.0, velocity.X + stopAccel * dt);
            }

            // 速度*時間だけ動かす
            OffsetWithAdjust(gameTime);
        }

        bool StopAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {

            }

            #endregion

            #region 2.アニメーション

            AnimStop(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg),
                new NextAnimation(Animation.DownB,true,null,true),
                new NextAnimation(Animation.B,true,NotFullGage,true),
                new NextAnimation(Animation.SpecialB,true,FullGage,true),
                new NextAnimation(Animation.UpSmash,true,null,true),
                new NextAnimation(Animation.DownSmash,true,null,true),
                new NextAnimation(Animation.SideSmash,true,null,true),
                new NextAnimation(Animation.UpA,true,null,true),
                new NextAnimation(Animation.DownA,true,null,true),
                new NextAnimation(Animation.SideA,true,null,true),
                new NextAnimation(Animation.A,true,null,true),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,true),
                new NextAnimation(Animation.AvoidG,true,null,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Shield,true,null,true),
                new NextAnimation(Animation.Sit,true,null,true),
                new NextAnimation(Animation.Run,true,null,true),
                new NextAnimation(Animation.Walk,true,null,true),
                new NextAnimation(Animation.Stop,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool WalkAnimation(GameTime gameTime)
        {

            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                if (input.isOn(BUTTON.LEFT, player.dev))
                {
                    direction = new Vector3(-1, 0, 0);
                }
                if (input.isOn(BUTTON.RIGHT, player.dev))
                {
                    direction = new Vector3(1, 0, 0);
                }
                Vector2 v = Vector2.Zero;
                v.Y = (float)RecVisible.Bottom3D;
                v.X = (float)(direction.X > 0 ? RecVisible.Left : RecVisible.Right);
                ParticleManager pm = new ParticleManager(scene, "SmokeParticles", v);
                pm.Follow(this);
                scene.EntityList.Add(pm);
            }

            #endregion

            #region 2.アニメーション
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // 速度のｙ方向は０に
            velocity.Y = 0;

            // ｘ方向には適度に加速度をかける
            if (input.isOn(BUTTON.LEFT, player.dev))
            {
                velocity.X = (float)Math.Max(velocity.X - walkAccel * gameTime.ElapsedGameTime.TotalSeconds, -maxWalkSpeed);
                direction = new Vector3(-1, 0, 0);
            }
            if (input.isOn(BUTTON.RIGHT, player.dev))
            {
                velocity.X = (float)Math.Min(velocity.X + walkAccel * gameTime.ElapsedGameTime.TotalSeconds, maxWalkSpeed);
                direction = new Vector3(1, 0, 0);
            }

            // 速度*時間だけ動かす
            OffsetWithAdjust(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg),
                new NextAnimation(Animation.DownB,true,null,true),
                new NextAnimation(Animation.B,true,NotFullGage,true),
                new NextAnimation(Animation.SpecialB,true,FullGage,true),
                new NextAnimation(Animation.UpSmash,true,null,true),
                new NextAnimation(Animation.DownSmash,true,null,true),
                new NextAnimation(Animation.SideSmash,true,null,true),
                new NextAnimation(Animation.UpA,true,null,true),
                new NextAnimation(Animation.DownA,true,null,true),
                new NextAnimation(Animation.SideA,true,null,true),
                new NextAnimation(Animation.A,true,null,true),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,true),
                new NextAnimation(Animation.AvoidG,true,null,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Shield,true,null,true),
                new NextAnimation(Animation.Sit,true,null,true),
                new NextAnimation(Animation.Run,true,null,true),
                new NextAnimation(Animation.Walk,true,null,true),
                new NextAnimation(Animation.Stop,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool RunAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                if (input.isOn(BUTTON.LEFT, player.dev))
                {
                    direction = new Vector3(-1, 0, 0);
                }
                if (input.isOn(BUTTON.RIGHT, player.dev))
                {
                    direction = new Vector3(1, 0, 0);
                }
                Vector2 v = Vector2.Zero;
                v.Y = (float)RecVisible.Bottom3D;
                v.X = (float)(direction.X > 0 ? RecVisible.Left : RecVisible.Right);

                for (int i = 0; i < 2; i++)
                {
                    ParticleManager pm = new ParticleManager(scene, "SmokeParticles", v);
                    pm.Follow(this);
                    scene.EntityList.Add(pm);
                }
            }

            #endregion

            #region 2.アニメーション
            // 速度のｙ方向は０に
            velocity.Y = 0;

            // ｘ方向には適度に加速度をかける
            if (input.isOn(BUTTON.LEFT, player.dev))
            {
                velocity.X = (float)Math.Max(velocity.X - runAccel * gameTime.ElapsedGameTime.TotalSeconds, -maxRunSpeed);
                direction = new Vector3(-1, 0, 0);
            }
            if (input.isOn(BUTTON.RIGHT, player.dev))
            {
                velocity.X = (float)Math.Min(velocity.X + runAccel * gameTime.ElapsedGameTime.TotalSeconds, maxRunSpeed);
                direction = new Vector3(1, 0, 0);
            }

            // 速度*時間だけ動かす
            OffsetWithAdjust(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg),
                new NextAnimation(Animation.DownB,true,null,true),
                new NextAnimation(Animation.B,true,NotFullGage,true),
                new NextAnimation(Animation.SpecialB,true,FullGage,true),
                new NextAnimation(Animation.UpSmash,true,null,true),
                new NextAnimation(Animation.DownSmash,true,null,true),
                new NextAnimation(Animation.SideSmash,true,null,true),
                new NextAnimation(Animation.UpA,true,null,true),
                new NextAnimation(Animation.DownA,true,null,true),
                new NextAnimation(Animation.SideA,true,null,true),
                new NextAnimation(Animation.A,true,null,true),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,true),
                new NextAnimation(Animation.AvoidG,true,null,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Shield,true,null,true),
                new NextAnimation(Animation.Sit,true,null,true),
                new NextAnimation(Animation.Run,true,null,true),
                new NextAnimation(Animation.Walk,true,null,true),
                new NextAnimation(Animation.Stop,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        double maxJumpSpeedX;
        bool JumpAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);

                normalizeRectangle = false;
                jumpFlg++;

                velocity.Y = 0;

                mi.recCollision = normalRectangle;

                if (jumpFlg <= 1)
                {
                    // 1段目のジャンプなら
                    jumpSpeed = jumpSpeed1;

                    Vector2 v = Vector2.Zero;
                    v.Y = (float)RecVisible.Bottom3D;
                    v.X = (float)(RecVisible.Center.X);

                    ParticleManager pm = new ParticleManager(scene, "SmokeParticles", v);
                    Follow(this);
                    scene.EntityList.Add(pm);
                }
                else
                {
                    // 2段目以上のジャンプなら
                    jumpSpeed = jumpSpeed2;
                }

                Cue c = GLOBAL.soundBank.GetCue("jump");
                c.Play();
            }

            #endregion

            #region 2.アニメーション
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (time < TimeSpan.FromSeconds(jumpTime1))
            {
                mi.recVisible.Top3D = normalRectangle.Top3D - jumpA * Math.Sin(MathHelper.Pi * time.TotalSeconds / jumpTime1);
            }
            else
            {
                mi.recVisible.Top3D = normalRectangle.Top3D;
                velocity.Y = (float)jumpSpeed;
                velocity.X = (float)maxJumpSpeedX * input.Stick(player.dev).X;
                endAnimation = true;
            }

            OffsetWithAdjust(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Float,false,null,endAnimation),
                new NextAnimation(Animation.Jump,true,null,!endAnimation),
            };


            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
                mi.rotation = Vector3.Zero;
            }


            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.Jump] = false;
                normalizeRectangle = true;
                mi.recCollision = mi.recVisible;
                mi.recVisible.Copy(normalRectangle);
            }

            #endregion

            return true;
        }

        void AnimFloat(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (input.isOn(BUTTON.DOWN, player.dev))
            {
                velocity.Y = (float)Math.Max(velocity.Y - fallAccel * dt, -maxFallSpeed);
            }
            else
            {
                velocity.Y = (float)Math.Max(velocity.Y - gravity * dt, -maxFallSpeed);
            }

            if (input.isOn(BUTTON.LEFT, player.dev))
            {
                velocity.X = (float)Math.Max(velocity.X - slideAccel * dt, -maxSlideSpeed);
                direction = new Vector3(-1, 0, 0);
            }
            if (input.isOn(BUTTON.RIGHT, player.dev))
            {
                velocity.X = (float)Math.Min(velocity.X + slideAccel * dt, maxSlideSpeed);
                direction = new Vector3(1, 0, 0);
            }

            if (jumpFlg >= 2)
            {
                // 2段目以上のジャンプなら
                floatRot += floatRotSpeed * dt * (direction.X > 0 ? 1.0 : -1.0);
            }

            mi.rotation = new Vector3(0, 0, (float)floatRot);

            Offset(gameTime);
            int res = Adjust(velocity.X, velocity.Y, true);
            if (AdjustToTop3D(res))
            {
                velocity.Y = 0;
            }
            if (!AdjustToBottom3D(res))
            {
                //                Offset(0, -adjustVec.Y);
            }
            else
            {
                jumpFlg = 0;
                avoidSFlg = true;
                upBFlg = true;
            }
        }

        bool avoidSFlg = true;
        bool upBFlg = true;

        bool FloatAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                floatRot = 0;
                if (jumpFlg <= 0) jumpFlg = 1;
            }

            #endregion

            #region 2.アニメーション
            AnimFloat(gameTime);
            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                //new NextAnimation(Animation.Climb,true,CheckClimb,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg),
                new NextAnimation(Animation.DownB,true,null,true),
                new NextAnimation(Animation.B,true,NotFullGage,true),
                new NextAnimation(Animation.SpecialB,true,FullGage,true),
                new NextAnimation(Animation.UpSmash,true,null,true),
                new NextAnimation(Animation.DownSmash,true,null,true),
                new NextAnimation(Animation.SideSmash,true,null,true),
                new NextAnimation(Animation.UpA,true,null,true),
                new NextAnimation(Animation.DownA,true,null,true),
                new NextAnimation(Animation.SideA,true,null,true),
                new NextAnimation(Animation.A,true,null,true),
                new NextAnimation(Animation.Ground,false,CheckGround,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.AvoidS,true,null,avoidSFlg),
                new NextAnimation(Animation.Float,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.rotation = Vector3.Zero;
            }

            #endregion

            return true;
        }

        bool GroundAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                jumpFlg = 0;
                normalizeRectangle = false;
                velocity.Y = 0;
            }

            #endregion

            #region 2.アニメーション
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            velocity.Y = 0;
            if (velocity.X > 0)
            {
                velocity.X = (float)Math.Max(velocity.X - groundAccel * dt, 0);
            }
            else
            {
                velocity.X = (float)Math.Min(velocity.X + groundAccel * dt, 0);
            }

            if (time < TimeSpan.FromSeconds(groundTime))
            {
                mi.recVisible.Top3D = normalRectangle.Top3D - groundA * Math.Sin(MathHelper.Pi * time.TotalSeconds / groundTime);
            }
            else
            {
                mi.recVisible.Top3D = normalRectangle.Top3D;
                endAnimation = true;
            }

            OffsetWithAdjust(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg && endAnimation),
                new NextAnimation(Animation.DownB,true,null,endAnimation),
                new NextAnimation(Animation.B,true,NotFullGage,endAnimation),
                new NextAnimation(Animation.SpecialB,true,FullGage,endAnimation),
                new NextAnimation(Animation.UpSmash,true,null,endAnimation),
                new NextAnimation(Animation.DownSmash,true,null,endAnimation),
                new NextAnimation(Animation.SideSmash,true,null,endAnimation),
                new NextAnimation(Animation.UpA,true,null,endAnimation),
                new NextAnimation(Animation.DownA,true,null,endAnimation),
                new NextAnimation(Animation.SideA,true,null,endAnimation),
                new NextAnimation(Animation.A,true,null,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,endAnimation),
                new NextAnimation(Animation.AvoidG,true,null,endAnimation),
                new NextAnimation(Animation.Jump,true,CheckJump,endAnimation),
                new NextAnimation(Animation.Shield,true,null,endAnimation),
                new NextAnimation(Animation.Sit,true,null,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.Ground,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                normalizeRectangle = true;
            }

            #endregion

            return true;
        }

        bool ClimbAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                climbRot = 0;
                jumpFlg = 0;
            }

            #endregion

            #region 2.アニメーション
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            if (-360 < climbRot && climbRot < 360)
            {
                climbRot += climbRotSpeed * dt * (direction.X > 0 ? 1.0 : -1.0);
            }
            else
            {
                climbRot = 0;
                endAnimation = true;
            }

            mi.rotation = new Vector3(0, 0, (float)climbRot);

            Adjust(1, 0, false);
            Adjust(-1, 0, false);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Jump,false,null,endAnimation),
                new NextAnimation(Animation.Climb,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.rotation = Vector3.Zero;
            }
            #endregion

            return true;
        }

        bool SitAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                normalizeRectangle = false;
            }

            #endregion

            #region 2.アニメーション

            if (time < TimeSpan.FromSeconds(sitTime))
            {
                mi.recVisible.Top3D = normalRectangle.Top3D - sitA * Math.Sin(MathHelper.Pi * time.TotalSeconds * 0.5 / sitTime);
            }
            else
            {
                endAnimation = true;
            }

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.UpB,true,null,upBFlg),
                new NextAnimation(Animation.DownB,true,null,true),
                new NextAnimation(Animation.B,true,NotFullGage,true),
                new NextAnimation(Animation.SpecialB,true,FullGage,true),
                new NextAnimation(Animation.UpSmash,true,null,true),
                new NextAnimation(Animation.DownSmash,true,null,true),
                new NextAnimation(Animation.SideSmash,true,null,true),
                new NextAnimation(Animation.UpA,true,null,true),
                new NextAnimation(Animation.DownA,true,null,true),
                new NextAnimation(Animation.SideA,true,null,true),
                new NextAnimation(Animation.A,true,null,true),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,true),
                new NextAnimation(Animation.AvoidG,true,null,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Shield,true,null,true),
                new NextAnimation(Animation.Sit,true,null,true),
                new NextAnimation(Animation.Run,true,null,true),
                new NextAnimation(Animation.Walk,true,null,true),
                new NextAnimation(Animation.Stop,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                normalizeRectangle = true;
            }

            #endregion

            return true;
        }

        #endregion

        #region シールド・緊急回避

        Shield shield;
        double shieldeX, shieldeY;

        bool ShieldAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                shield = new Shield(scene, (Hashtable)scene.Parameters["Shield"], mi.recCollision, player.id, player.dev);
                shield.Follow(this);
                shield.Initialize();
                scene.EntityList.Add(shield);
                scene.SolidList.Add(shield);
                Cue c = GLOBAL.soundBank.GetCue("shield");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            AnimStop(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Float,false,CheckFloat,true),
                new NextAnimation(Animation.AvoidR,true,null,true),
                new NextAnimation(Animation.AvoidG,true,null,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Shield,true,null,true),
                new NextAnimation(Animation.Sit,true,null,true),
                new NextAnimation(Animation.Run,true,null,true),
                new NextAnimation(Animation.Walk,true,null,true),
                new NextAnimation(Animation.Stop,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                if (shield != null) shield.Dispose();
            }

            #endregion

            return true;
        }

        bool AvoidGAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                Object o;
                mi.addAlpha = int.Parse((o = parameters["avoidGAddAlpha"]) != null ? (string)o : "255");
                Cue c = GLOBAL.soundBank.GetCue("avoid");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            if (time >= TimeSpan.FromSeconds(avoidGTime))
            {
                endAnimation = true;
            }

            AnimStop(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.AvoidG,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.AvoidG] = false;
                commandFlg[(int)Animation.AvoidR] = false;
                commandFlg[(int)Animation.AvoidS] = false;
                commandFlg[(int)Animation.Shield] = false;
                mi.addAlpha = -1;
            }

            #endregion

            return true;
        }

        bool AvoidRAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                Object o;
                mi.addAlpha = int.Parse((o = parameters["avoidRAddAlpha"]) != null ? (string)o : "255");

                // 進行方向とは逆を向く
                if (input.isOn(BUTTON.LEFT, player.dev))
                {
                    direction = new Vector3(1, 0, 0);
                }
                if (input.isOn(BUTTON.RIGHT, player.dev))
                {
                    direction = new Vector3(-1, 0, 0);
                }
                Cue c = GLOBAL.soundBank.GetCue("avoid");
                c.Play();

            }
            #endregion

            #region 2.アニメーション

            velocity.Y = 0;
            if (direction.X < 0)
            {
                velocity.X = (float)avoidRSpeed;
            }
            else
            {
                velocity.X = -(float)avoidRSpeed;
            }

            if (time >= TimeSpan.FromSeconds(avoidRTime))
            {
                velocity = Vector2.Zero;
                endAnimation = true;
            }

            OffsetWithAdjust(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.AvoidR,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.AvoidG] = false;
                commandFlg[(int)Animation.AvoidR] = false;
                commandFlg[(int)Animation.AvoidS] = false;
                commandFlg[(int)Animation.Shield] = false;
                mi.addAlpha = -1;
            }

            #endregion

            return true;
        }

        bool AvoidSAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                Object o;
                mi.addAlpha = int.Parse((o = parameters["avoidSAddAlpha"]) != null ? (string)o : "255");
                Vector2 v = input.Stick(player.dev);
                if (v != Vector2.Zero)
                {
                    v.Normalize();
                    v.X *= (float)avoidSSpeed;
                    v.Y *= -(float)avoidSSpeed;
                }
                velocity = v;
                Cue c = GLOBAL.soundBank.GetCue("avoid");
                c.Play();
            }

            #endregion

            #region 2.アニメーション
            if (time >= TimeSpan.FromSeconds(avoidSTime))
            {
                velocity = Vector2.Zero;
                endAnimation = true;
            }

            OffsetWithAdjust(gameTime);
            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Ground,false,CheckGround,true),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.AvoidS,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.AvoidG] = false;
                commandFlg[(int)Animation.AvoidR] = false;
                commandFlg[(int)Animation.AvoidS] = false;
                commandFlg[(int)Animation.Shield] = false;
                if (AdjustToBottom3D(adjustTo))
                {
                    velocity.Y = 0;
                }
                else
                {
                    velocity = Vector2.Zero;
                }
                mi.addAlpha = -1;
                avoidSFlg = false;
            }

            #endregion

            return true;
        }

        #endregion

        #region 吹っ飛び・受身・攻撃された瞬間
        double brownTime = 3.0;
        double brownRotSpeed;
        double _brownRotSpeed;
        double brownRot;
        bool BrownAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                jumpFlg = 1;
                if (velocity.LengthSquared() > maxBrownSpeed * maxBrownSpeed)
                {
                    velocity.Normalize();
                    velocity.X *= (float)maxBrownSpeed;
                    velocity.Y *= (float)maxBrownSpeed;
                }
                brownRot = 0;
                brownRotSpeed = _brownRotSpeed * (direction.X > 0 ? 1 : -1);
            }
            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (input.isOn(BUTTON.DOWN, player.dev))
            {
                velocity.Y = (float)Math.Max(velocity.Y - brownFallAccel * dt, -maxFallSpeed);
            }
            else
            {
                velocity.Y = (float)Math.Max(velocity.Y - gravity * dt, -maxFallSpeed);
            }

            if (input.isOn(BUTTON.LEFT, player.dev))
            {
                velocity.X = (float)(velocity.X - brownSlideAccel * dt);
            }
            if (input.isOn(BUTTON.RIGHT, player.dev))
            {
                velocity.X = (float)(velocity.X + brownSlideAccel * dt);
            }

            if (velocity.LengthSquared() > maxBrownSpeed * maxBrownSpeed)
            {
                velocity.Normalize();
                velocity.X *= (float)maxBrownSpeed;
                velocity.Y *= (float)maxBrownSpeed;
            }

            brownRot += brownRotSpeed * dt;

            mi.rotation = new Vector3(0, 0, (float)brownRot);

            OffsetWithAdjust(gameTime);

            if (time >= TimeSpan.FromSeconds(brownTime))
            {
                endAnimation = true;
            }

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.BreakFall,true,CheckBreakFall,true),
                new NextAnimation(Animation.BreakFallR,true,CheckBreakFallR,true),
                new NextAnimation(Animation.Down,false,CheckDown,true),
                new NextAnimation(Animation.Bump,false,CheckBump,true),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Brown,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.rotation = Vector3.Zero;
            }

            #endregion

            return true;
        }

        bool BreakFallAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                if (AdjustToLeft(adjustTo))
                {
                    velocity.X = (float)breakFallSpeed;
                }
                if (AdjustToRight(adjustTo))
                {
                    velocity.X = -(float)breakFallSpeed;
                }
                if (AdjustToTop3D(adjustTo))
                {
                    velocity.Y = -(float)breakFallSpeed;
                }
                if (AdjustToBottom3D(adjustTo))
                {
                    velocity.Y = (float)breakFallSpeed;
                }
                velocity.X = (float)Math.Max(velocity.X, -maxBreakFallSpeed);
                velocity.X = (float)Math.Min(velocity.X, maxBreakFallSpeed);
                velocity.Y = (float)Math.Max(velocity.Y, -maxBreakFallSpeed);
                velocity.Y = (float)Math.Min(velocity.Y, maxBreakFallSpeed);
            }

            #endregion

            #region 2.アニメーション

            endAnimation = true;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Brown,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.BreakFall,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool BreakFallRAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {

            }

            #endregion

            #region 2.アニメーション

            endAnimation = true;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool DownAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {

            }

            #endregion

            #region 2.アニメーション

            AnimStop(gameTime);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;

            bool flg1 = input.isOn(BUTTON.LEFT, player.dev) || input.isOn(BUTTON.RIGHT, player.dev);
            bool flg2 = flg1 ||
                input.isOn(BUTTON.UP, player.dev) ||
                input.isOn(BUTTON.DOWN, player.dev) ||
                input.isOn(BUTTON.A, player.dev) ||
                input.isOn(BUTTON.B, player.dev);

            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.AvoidR,false,null,endAnimation && flg1),
                new NextAnimation(Animation.Stop,false,null,endAnimation && flg2),
                new NextAnimation(Animation.BreakFall,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool BumpAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                int prev = (cnt_adjustToHist - 1 + adjustToHist.Length) % adjustToHist.Length;
                if (AdjustToLeft(adjustToHist[prev]))
                {
                    velocity.X = (float)Math.Abs(velocity.X);
                }
                if (AdjustToRight(adjustToHist[prev]))
                {
                    velocity.X = -(float)Math.Abs(velocity.X);
                }
                if (AdjustToTop3D(adjustToHist[prev]))
                {
                    velocity.Y = -(float)Math.Abs(velocity.Y);
                }
                if (AdjustToBottom3D(adjustToHist[prev]))
                {
                    velocity.Y = (float)Math.Abs(velocity.Y);
                }
            }

            #endregion

            #region 2.アニメーション

            endAnimation = true;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Brown,false,null,endAnimation),
                new NextAnimation(Animation.Bump,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        bool AttackedAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                tmpEmissiveColor = mi.emissiveColor;
                mi.emissiveColor = new Vector3(1, 1, 1);
            }

            #endregion

            #region 2.アニメーション
            if (time >= TimeSpan.FromSeconds(attackedTime))
            {
                endAnimation = true;
            }
            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Brown,false,null,endAnimation),
                new NextAnimation(Animation.Attacked,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.emissiveColor = tmpEmissiveColor;
            }

            #endregion

            return true;
        }

        #endregion

        #region 死亡・復活

        bool DeadAnimation(GameTime gameTime)
        {
            #region 1、初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                scene.EntityList.Add(new ParticleManager(scene, "DeadParticles", mi.recCollision.Center));
                Cue sound = GLOBAL.soundBank.GetCue("dead");
                sound.Play();
            }
            #endregion

            #region 2.アニメーション

            if (time >= TimeSpan.FromSeconds(deadTime))
            {
                velocity = Vector2.Zero;
                endAnimation = true;
            }

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Reborn,false,null,endAnimation),
                new NextAnimation(Animation.Dead,false,null,true)
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                life--;
                gage = 0;
                if (life <= 0)
                {
                    scene.GameSet(1 - player.id);
                }
            }

            #endregion

            return true;
        }

        bool RebornAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                // scene.EntityList.Add(RebornParticle);
                Locate(rebornPos.X, rebornPos.Y);
                damage = 0;
            }

            #endregion

            #region 2.アニメーション
            endAnimation = true;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                // new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Reborn,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {

            }

            #endregion

            return true;
        }

        #endregion

        #region A技

        bool AAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                ARot = 0;

                attackArea = null;
                Object o = scene.Parameters["AttackArea"];
                if (o != null)
                {
                    attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, AeX, AeY));
                    attackArea.NotAttack(player.id);
                    attackArea.Follow(this);
                    attackArea.Power = APower;
                    attackArea.Damage = ADamage;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                }
                Cue c = GLOBAL.soundBank.GetCue("A");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            if (-360 < ARot && ARot < 360)
            {
                ARot += ARotSpeed * dt * (direction.X > 0 ? 1 : -1);
            }
            else
            {
                ARot = 0;
                endAnimation = true;
            }

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            mi.rotation = new Vector3(0, (float)ARot, 0);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.A,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.SideA] = false;
                commandFlg[(int)Animation.SideSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                if (attackArea != null)
                {
                    attackArea.Dispose();
                    attackArea = null;
                }
            }

            #endregion

            return true;
        }

        bool SideAAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);

                normalizeRectangle = false;

                attackArea = null;
                Object o = scene.Parameters["AttackArea"];
                if (o != null)
                {
                    attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, sideAeX, sideAeY));
                    attackArea.Follow(this);
                    attackArea.NotAttack(player.id);
                    attackArea.Power = sideAPower;
                    attackArea.Damage = sideADamage;
                    attackArea.manual = true;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                }

                if (input.isOn(BUTTON.LEFT, player.dev))
                {
                    direction = new Vector3(-1, 0, 0);
                    tmpVelocity = new Vector2(-(float)sideASpeed, 0);
                }
                else
                {
                    direction = new Vector3(1, 0, 0);
                    tmpVelocity = new Vector2((float)sideASpeed, 0);
                }
                velocity += tmpVelocity;

                ex = ey = 1.0;
                Cue c = GLOBAL.soundBank.GetCue("SA");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic

            // モデルを伸縮
            if (time < TimeSpan.FromSeconds(sideATime1))
            {
                ex += sideAExtendSpeed * dt;
                double s = 1 / Math.Sqrt(ex);
                mi.depth = mi.defaultDepth * s;
                ey = s;
            }
            else if (time < TimeSpan.FromSeconds(sideATime2 + sideATime1))
            {

            }
            else if (time < TimeSpan.FromSeconds(sideATime3 + sideATime2 + sideATime1))
            {
                ex -= sideAExtendSpeed * dt;
                double s = 1 / Math.Sqrt(ex);
                mi.depth = mi.defaultDepth * s;
                ey = s;
            }
            else
            {
                endAnimation = true;
            }

            RectangleD.ExtendRect(normalRectangle, ex, ey, mi.recVisible);
            RectangleD.ExtendRect(mi.recVisible, AeX, AeY, attackArea.RecCollision);

            mi.recCollision = new RectangleD(RecVisible.X, normalRectangle.Y, RecVisible.Width, normalRectangle.Height);

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                // 速度のｙ方向は０に
                velocity.Y = 0;

                // 速度*時間だけ動かす
                OffsetWithAdjust(gameTime);
            }

            mi.rotation = new Vector3(0, 0, 0);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.SideA,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.SideA] = false;
                commandFlg[(int)Animation.SideSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                mi.recVisible.Copy(normalRectangle);
                mi.recCollision = mi.recVisible;
                if (attackArea != null) attackArea.Dispose();
                velocity -= tmpVelocity;
                if (direction.X > 0)
                {
                    velocity.X = Math.Max(velocity.X, 0.0f);
                }
                else
                {
                    velocity.X = Math.Min(velocity.X, 0.0f);
                }
            }

            #endregion

            return true;
        }

        bool UpAAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);

                normalizeRectangle = false;

                attackArea = null;
                Object o = scene.Parameters["AttackArea"];
                if (o != null)
                {
                    attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, upAeX, upAeY));
                    attackArea.Follow(this);
                    attackArea.NotAttack(player.id);
                    attackArea.Power = upAPower;
                    attackArea.Damage = upADamage;
                    attackArea.manual = true;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                }

                ex = ey = 1.0;
                ox = oy = 0;
                Cue c = GLOBAL.soundBank.GetCue("SA");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic

            // モデルを伸縮
            if (time < TimeSpan.FromSeconds(upATime1))          // 伸ばす
            {
                ey += upAExtendSpeed * dt;
                double s = 1 / Math.Sqrt(ey);
                mi.depth = mi.defaultDepth * s;
                ex = s;
            }
            else if (time < TimeSpan.FromSeconds(upATime2 + upATime1))
            {

            }
            else if (time < TimeSpan.FromSeconds(upATime3 + upATime2 + upATime1))  // 縮める
            {
                ey -= upAExtendSpeed * dt;
                double s = 1 / Math.Sqrt(ex);
                mi.depth = mi.defaultDepth * s;
                ex = s;
            }
            else
            {
                endAnimation = true;
            }

            RectangleD.ExtendRect(normalRectangle, ex, ey, mi.recVisible);

            // ちょっと上下にずらす
            oy = mi.recVisible.Height - normalRectangle.Height;
            oy *= 0.5;
            mi.recVisible.Y += oy;
            RectangleD.ExtendRect(mi.recVisible, AeX, AeY, attackArea.RecCollision);

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            mi.rotation = new Vector3(0, 0, 0);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.UpA,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.recCollision = mi.recVisible;
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.SideA] = false;
                commandFlg[(int)Animation.SideSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                if (attackArea != null) attackArea.Dispose();
            }

            #endregion

            return true;
        }

        double tmpTime = 0;
        double downARot = 0;
        double downARotSpeed = 360;
        double _downARotSpeed = 360;

        bool DownAAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                //                mi.recCollision = normalRectangle;

                normalizeRectangle = false;

                attackArea = null;
                Object o = scene.Parameters["AttackArea"];
                if (o != null)
                {
                    attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, downAeX, downAeY));
                    attackArea.Follow(this);
                    attackArea.NotAttack(player.id);
                    attackArea.Power = downAPower;
                    attackArea.Damage = downADamage;
                    attackArea.manual = true;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                }

                ex = 1;
                ey = mi.recVisible.Height / normalRectangle.Height;
                ox = oy = 0;

                tmpTime = 0;

                downARotSpeed = _downARotSpeed * (direction.X > 0 ? 1 : -1);
                downARot = 0;
                Cue c = GLOBAL.soundBank.GetCue("SA");
                c.Play();
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic

            // モデルを伸縮
            if (tmpTime <= 0 && time < TimeSpan.FromSeconds(downATime1))
            {
                if (ey > 1 - downAExtendSpeed * downATime1)
                {
                    ey -= downAExtendSpeed * dt;
                }
                else
                {
                    tmpTime = time.TotalSeconds;
                    ey = 1 - downAExtendSpeed * downATime1;
                }
                double s = 1 / Math.Sqrt(ey);
                mi.depth = mi.defaultDepth * s;
                ex = s;
            }
            else if (time < TimeSpan.FromSeconds(downATime2 + downATime1))
            {

            }
            else if (time < TimeSpan.FromSeconds(downATime3 + downATime2 + downATime1))
            {
                ey += downAExtendSpeed * dt;
                double s = 1 / Math.Sqrt(ex);
                mi.depth = mi.defaultDepth * s;
                ex = s;
            }
            else
            {
                endAnimation = true;
            }

            RectangleD.ExtendRect(normalRectangle, ex, ey, mi.recVisible);
            // ちょっと上下にずらす
            oy = mi.recVisible.Height * ey - normalRectangle.Height;
            oy *= 0.5;
            Vector2 pos = normalRectangle.Position();

            mi.recVisible.Y += oy;
            RectangleD.ExtendRect(mi.recVisible, AeX, AeY, attackArea.RecCollision);


            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            downARot += downARotSpeed * dt;
            mi.rotation = new Vector3(0, (float)downARot, 0);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.DownA,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                mi.rotation = Vector3.Zero;
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.SideA] = false;
                commandFlg[(int)Animation.SideSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                if (attackArea != null) attackArea.Dispose();
            }

            #endregion

            return true;
        }

        #endregion

        #region スマッシュ
        double sideSmashTime1 = 0;

        double GetSmashPower(double minValue, double maxValue)
        {
            double d = time.TotalSeconds / maxPileTime;
            double p = (1 - d) * minValue + d * maxValue;
            return p;
        }

        double maxSideSmashTime = 0.4;
        double minSideSmashTime = 0.8;
        double sideSmashTime = 0;
        bool SideSmashAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);

                normalizeRectangle = false;

                pileFlg = piling;
                sideSmashRot = sideSmashRotSpeed = sideSmashSpeed = 0;
                sideSmashRotAccel = Math.Abs(sideSmashRotAccel) * (direction.X > 0 ? -1 : 1);

                tmpEmissiveColor = mi.emissiveColor;
                sideSmashTime1 = 0;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            switch (pileFlg)
            {
                case piling:
                    if (time.TotalSeconds >= maxPileTime || input.isOn(BUTTON.A, player.dev) == false)
                    {
                        pileFlg = finishPiling;
                    }

                    sideSmashSpeed = 0;
                    sideSmashRotSpeed += sideSmashRotAccel * dt;
                    break;
                case finishPiling:
                    pileFlg = afterPiling;
                    sideSmashTime = time.TotalSeconds + GetSmashPower(minSideSmashTime, maxSideSmashTime);

                    sideSmashSpeed = GetSmashPower(minSideSmashSpeed, maxSideSmashSpeed);
                    sideSmashSpeed *= direction.X > 0 ? 1 : -1;
                    velocity.X += (float)sideSmashSpeed;

                    Object o = scene.Parameters["AttackArea"];
                    if (o != null)
                    {
                        attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, sideSmasheX, sideSmasheY));
                        attackArea.Follow(this);
                        attackArea.NotAttack(player.id);
                        attackArea.Power = GetSmashPower(minSideSmashPower, maxSideSmashPower);
                        attackArea.Damage = GetSmashPower(minSideSmashDamage, maxSideSmashDamage);
                        scene.EntityList.Add(attackArea);
                        scene.AttackerList.Add(attackArea);
                    }
                    Cue c = GLOBAL.soundBank.GetCue("smash");
                    c.Play();
                    break;
                case afterPiling:
                    if (time.TotalSeconds >= sideSmashTime)
                    {
                        endAnimation = true;
                    }
                    break;
                default:
                    break;
            }

            sideSmashRot += sideSmashRotSpeed * dt;

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                if (pileFlg == piling)
                {
                    AnimStop(gameTime);
                }
                else
                {
                    // 速度のｙ方向は０に
                    velocity.Y = 0;

                    // 速度*時間だけ動かす
                    OffsetWithAdjust(gameTime);
                }
            }

            mi.rotation = new Vector3(0, 0, (float)sideSmashRot);
            mi.emissiveColor = new Vector3(
                (float)Math.Min(mi.emissiveColor.X + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Y + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Z + 1.0 * dt / maxPileTime, 1));

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Jump,true,CheckJump,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.SideSmash,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.SideA] = false;
                commandFlg[(int)Animation.SideSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                if (attackArea != null) attackArea.Dispose();
                mi.emissiveColor = tmpEmissiveColor;
                mi.rotation = Vector3.Zero;
                velocity.X -= (float)sideSmashSpeed;
                if (direction.X > 0)
                {
                    velocity.X = Math.Max(velocity.X, 0.0f);
                }
                else
                {
                    velocity.X = Math.Min(velocity.X, 0.0f);
                }
            }

            #endregion

            return true;
        }

        // 回りながら跳ね上がる
        double upSmashRot;
        double upSmashRotSpeed;
        double upSmashRotAccel;
        double maxUpSmashTime;
        double minUpSmashTime;
        double upSmashA;
        double upSmasheX, upSmasheY;
        double maxUpSmashA;
        double upSmashExtendSpeed;
        bool UpSmashAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;

                normalizeRectangle = false;

                pileFlg = piling;
                upSmashRot = upSmashRotSpeed = 0;
                upSmashRotAccel = Math.Abs(upSmashRotAccel) * (direction.X > 0 ? -1 : 1);
                upSmashA = 0;

                tmpEmissiveColor = mi.emissiveColor;
                upSmashTime = 0;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            switch (pileFlg)
            {
                case piling:
                    if (time.TotalSeconds >= maxPileTime || input.isOn(BUTTON.A, player.dev) == false)
                    {
                        pileFlg = finishPiling;
                    }

                    upSmashRotSpeed += upSmashRotAccel * dt;
                    upSmashRot += upSmashRotSpeed * dt;

                    mi.recVisible.Top3D = normalRectangle.Top3D - maxUpSmashA * Math.Sin((float)(0.5 * MathHelper.Pi * time.TotalSeconds / maxPileTime));
                    break;
                case finishPiling:
                    pileFlg = afterPiling;
                    upSmashTime = time.TotalSeconds + GetSmashPower(minUpSmashTime, maxUpSmashTime);
                    upSmashA = GetSmashPower(0, maxUpSmashA);

                    Object o = scene.Parameters["AttackArea"];
                    if (o != null)
                    {
                        attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, upSmasheX, upSmasheY));
                        attackArea.Follow(this);
                        attackArea.NotAttack(player.id);
                        attackArea.Power = GetSmashPower(minUpSmashPower, maxUpSmashPower);
                        attackArea.Damage = GetSmashPower(minUpSmashDamage, maxUpSmashDamage);
                        scene.EntityList.Add(attackArea);
                        scene.AttackerList.Add(attackArea);
                    }
                    Cue c = GLOBAL.soundBank.GetCue("smash");
                    c.Play();
                    break;
                case afterPiling:
                    if (mi.recVisible.Top3D < normalRectangle.Top3D + upSmashA)
                    {
                        mi.recVisible.Top3D += upSmashExtendSpeed * dt;
                    }
                    if (time.TotalSeconds > upSmashTime)
                    {
                        endAnimation = true;
                    }
                    break;
                default:
                    break;
            }

            upSmashRot += upSmashRotSpeed * dt;

            // 横方向の拡大
            ey = mi.recVisible.Height / normalRectangle.Height;
            ex = 1 / Math.Sqrt(ey);
            double t = mi.recVisible.Top3D;
            RectangleD.ExtendRect(normalRectangle, ex, ey, mi.recVisible);
            mi.depth = mi.defaultDepth * ex;
            mi.recVisible.Offset(0, (mi.recVisible.Height - normalRectangle.Height) * 0.5);
            if (attackArea != null)
            {
                RectangleD.ExtendRect(mi.recVisible, upSmasheX, upSmasheY, attackArea.RecCollision);
            }
            mi.depth = ex;

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                if (pileFlg == piling)
                {
                    AnimStop(gameTime);
                }
            }

            mi.rotation = new Vector3(0, (float)upSmashRot, 0);
            mi.emissiveColor = new Vector3(
                (float)Math.Min(mi.emissiveColor.X + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Y + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Z + 1.0 * dt / maxPileTime, 1));

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.UpSmash,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.UpA] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.UpSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                if (attackArea != null) attackArea.Dispose();
                mi.emissiveColor = tmpEmissiveColor;
                mi.rotation = Vector3.Zero;
            }

            #endregion

            return true;
        }

        double downSmashRot;
        double downSmashRotSpeed;
        double downSmashRotAccel;
        double downSmashA;
        double maxDownSmashA;
        double minDownSmashTime, maxDownSmashTime;
        double downSmasheX, downSmasheY;
        double downSmashExtendSpeed;
        bool DownSmashAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;

                normalizeRectangle = false;

                pileFlg = piling;
                downSmashRot = downSmashRotSpeed = 0;
                downSmashRotAccel = Math.Abs(downSmashRotAccel) * (direction.X > 0 ? -1 : 1);
                downSmashA = 0;

                tmpEmissiveColor = mi.emissiveColor;
                downSmashTime = 0;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            switch (pileFlg)
            {
                case piling:
                    if (time.TotalSeconds >= maxPileTime || input.isOn(BUTTON.A, player.dev) == false)
                    {
                        pileFlg = finishPiling;
                    }

                    downSmashRotSpeed += downSmashRotAccel * dt;
                    downSmashRot += downSmashRotSpeed * dt;

                    mi.recVisible.Top3D = normalRectangle.Top3D + maxDownSmashA * Math.Sin((float)(0.5 * MathHelper.Pi * time.TotalSeconds / maxPileTime));
                    break;
                case finishPiling:
                    pileFlg = afterPiling;
                    downSmashTime = time.TotalSeconds + GetSmashPower(minDownSmashTime, maxDownSmashTime);
                    downSmashA = GetSmashPower(0, maxDownSmashA);

                    Object o = scene.Parameters["AttackArea"];
                    if (o != null)
                    {
                        attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recCollision, downSmasheX, downSmasheY));
                        attackArea.Follow(this);
                        attackArea.NotAttack(player.id);
                        attackArea.Power = GetSmashPower(minDownSmashPower, maxDownSmashPower);
                        attackArea.Damage = GetSmashPower(minDownSmashDamage, maxDownSmashDamage);
                        scene.EntityList.Add(attackArea);
                        scene.AttackerList.Add(attackArea);
                    }
                    Cue c = GLOBAL.soundBank.GetCue("smash");
                    c.Play();
                    break;
                case afterPiling:
                    if (mi.recVisible.Top3D > normalRectangle.Top3D - downSmashA)
                    {
                        mi.recVisible.Top3D -= downSmashExtendSpeed * dt;
                    }
                    if (time.TotalSeconds > downSmashTime)
                    {
                        endAnimation = true;
                    }
                    break;
                default:
                    break;
            }

            downSmashRot += downSmashRotSpeed * dt;

            // 横方向の拡大
            ey = mi.recVisible.Height / normalRectangle.Height;
            ex = 1 / Math.Sqrt(ey);
            double t = mi.recVisible.Top3D;
            RectangleD.ExtendRect(normalRectangle, ex, ey, mi.recVisible);
            mi.depth = mi.defaultDepth * ex;
            mi.recVisible.Offset(0, (mi.recVisible.Height - normalRectangle.Height) * 0.5);
            if (attackArea != null)
            {
                RectangleD.ExtendRect(mi.recVisible, downSmasheX, downSmasheY, attackArea.RecCollision);
            }
            mi.depth = ex;

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                if (pileFlg == piling)
                {
                    AnimStop(gameTime);
                }
            }

            mi.rotation = new Vector3(0, (float)downSmashRot, 0);
            mi.emissiveColor = new Vector3(
                (float)Math.Min(mi.emissiveColor.X + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Y + 1.0 * dt / maxPileTime, 1),
                (float)Math.Min(mi.emissiveColor.Z + 1.0 * dt / maxPileTime, 1));

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.DownSmash,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.A] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.DownA] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                commandFlg[(int)Animation.DownSmash] = false;
                normalizeRectangle = true;
                if (attackArea != null) attackArea.Dispose();
                mi.emissiveColor = tmpEmissiveColor;
                mi.rotation = Vector3.Zero;
                mi.depth = mi.defaultDepth;
            }

            #endregion

            return true;
        }

        #endregion

        #region CubeのB技

        int cubeBCnt = 0;
        CubicBomb[] cubeBBomb = new CubicBomb[] { null, null, null, null, null };

        bool CubeBAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;

                int i = 0;

                for (i = 0; i < cubeBBomb.Length; i++)
                {
                    if (cubeBBomb[i] == null)
                    {
                        break;
                    }
                }

                if (i < cubeBBomb.Length)
                {
                    Object o = scene.Parameters["CubicBomb"];
                    if (o != null)
                    {
                        RectangleD rect = new RectangleD();
                        rect.Copy(normalRectangle);
                        if (direction.X > 0)
                        {
                            rect.X += 1;
                        }
                        else
                        {
                            rect.X -= 1;
                        }
                        cubeBBomb[i] = new CubicBomb(scene, (Hashtable)o, rect, player.id);
                        scene.EntityList.Add(cubeBBomb[i]);
                        scene.SolidList.Add(cubeBBomb[i]);
                        Cue c = GLOBAL.soundBank.GetCue("B");
                        c.Play();
                    }
                }
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic

            endAnimation = true;

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            mi.rotation = Vector3.Zero;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.B,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;
            }

            #endregion

            return true;
        }

        double cubeUpBA = 10;
        double cubeUpBSpeed1 = 10;
        double cubeUpBSpeed2;
        double cubeUpBTime1 = 10;
        double cubeUpBTime2;
        double cubeUpBTime3;
        double cubeUpBBottom3D;
        double cubeUpBBottom3D2;
        bool CubeUpBAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;

                normalizeRectangle = false;

                attackArea = null;
                Object o = scene.Parameters["AttackArea"];
                if (o != null)
                {
                    attackArea = new AttackArea(scene, (Hashtable)o, RectangleD.ExtendRect(mi.recVisible, AeX, AeY));
                    attackArea.NotAttack(player.id);
                    attackArea.Follow(this);
                    attackArea.Power = APower;
                    attackArea.Damage = ADamage;
                    scene.EntityList.Add(attackArea);
                    scene.AttackerList.Add(attackArea);
                    Cue c = GLOBAL.soundBank.GetCue("UpB");
                    c.Play();
                }
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            if (time < TimeSpan.FromSeconds(cubeUpBTime1))
            {
                mi.recVisible.Top3D = normalRectangle.Top3D - cubeUpBA * Math.Sin(MathHelper.Pi * time.TotalSeconds / cubeUpBTime1);
                OffsetWithAdjust(gameTime);
                cubeUpBBottom3D = cubeUpBBottom3D2 = mi.recVisible.Bottom3D;
            }
            else if (time < TimeSpan.FromSeconds(cubeUpBTime1 + cubeUpBTime2))
            {
                mi.recCollision = normalRectangle;
                velocity.X = 0;
                velocity.Y = (float)cubeUpBSpeed1;
                OffsetWithAdjust(gameTime);
                mi.recVisible.Bottom3D = cubeUpBBottom3D;
            }
            else if (time < TimeSpan.FromSeconds(cubeUpBTime1 + cubeUpBTime2 + cubeUpBTime3))
            {
                double A = normalRectangle.Bottom3D - cubeUpBBottom3D2;
                double t = MathHelper.PiOver2 * (time.TotalSeconds - cubeUpBTime1 - cubeUpBTime2) / cubeUpBTime3;
                mi.recVisible.Bottom3D = normalRectangle.Bottom3D - A * Math.Cos(t);
            }
            else
            {
                velocity = new Vector2(0, (float)cubeUpBSpeed2);
                endAnimation = true;
            }

            attackArea.RecVisible = RectangleD.ExtendRect(mi.recVisible, AeX, AeY);

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.UpB,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;

                mi.recCollision = mi.recVisible;
                mi.recVisible.Copy(normalRectangle);
                Depth = 1;
                normalizeRectangle = true;
                upBFlg = false;

                if (attackArea != null) attackArea.Dispose();
            }

            #endregion

            return true;
        }

        double tmp1;
        Vector3 tmpVec;
        double cubeDownBMaxFallSpeed;
        Vector3 cubeDownBEmissiveColor;
        bool attackedAbsorb = false;

        bool CubeDownBAnimation(GameTime gameTime)
        {
            #region 1.初期化

            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;
                attackedAbsorb = true;
                tmpVec = mi.emissiveColor;
                mi.emissiveColor = cubeDownBEmissiveColor;
                tmp1 = maxFallSpeed;
                maxFallSpeed = cubeDownBMaxFallSpeed;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            if (commandState[(int)Animation.DownB] == false)
            {
                endAnimation = true;
            }

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            mi.rotation = Vector3.Zero;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.DownB,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;

                attackedAbsorb = false;
                maxFallSpeed = tmp1;
                mi.emissiveColor = tmpVec;
            }

            #endregion

            return true;
        }

        CubicBomb[] cubeSBBomb = new CubicBomb[10];
        int cubeSBCnt = 0;
        double cubeSBRad = 10;
        double cubeSBAngle = 0;
        double cubeSBDAngle = 0;
        double cubeSBTimeSpan = 0.3;

        bool CubeSpecialBAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (cubeSBCnt <= 0 && time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;
                cubeSBCnt = 0;
                cubeSBDAngle = 360.0 / (double)cubeSBBomb.Length * (direction.X > 0 ? 1 : -1);
                cubeSBAngle = (direction.X > 0 ? 0 : 180);
                time = TimeSpan.FromSeconds(cubeSBTimeSpan);
                gage = 0;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            if (time >= TimeSpan.FromSeconds(cubeSBTimeSpan))
            {
                Object o = scene.Parameters["SpecialCubicBomb"];
                if (o != null)
                {
                    double x = mi.recCollision.Center.X + cubeSBRad * Math.Cos(MathHelper.ToRadians((float)cubeSBAngle));
                    double y = mi.recCollision.Center.Y + cubeSBRad * Math.Sin(MathHelper.ToRadians((float)cubeSBAngle));
                    RectangleD rect = new RectangleD(x - 0.5, y - 0.5, 1, 1);
                    cubeSBBomb[cubeSBCnt] = new CubicBomb(scene, (Hashtable)o, rect, player.id);
                    scene.EntityList.Add(cubeSBBomb[cubeSBCnt]);
                    scene.SolidList.Add(cubeSBBomb[cubeSBCnt]);
                }
                cubeSBAngle += cubeSBDAngle;
                cubeSBCnt++;
                time = TimeSpan.Zero;
                Cue c = GLOBAL.soundBank.GetCue("B");
                c.Play();
            }

            if (cubeSBCnt >= cubeSBBomb.Length)
            {
                endAnimation = true;
            }

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.SpecialB,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;
                cubeSBCnt = 0;
            }

            #endregion

            return true;
        }

        #endregion

        #region SphereのB技

        GuidedBomb[] sphereBBomb = new GuidedBomb[1];
        int sphereBCnt = 0;

        bool SphereBAnimation(GameTime gameTime)
        {
            #region 1.初期化

            if (time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;
                int i = 0;

                for (i = 0; i < sphereBBomb.Length; i++)
                {
                    if (sphereBBomb[i] == null)
                    {
                        break;
                    }
                }

                if (i < sphereBBomb.Length)
                {
                    Object o = scene.Parameters["GuidedBomb"];
                    if (o != null)
                    {
                        RectangleD rect = new RectangleD();
                        rect.Copy(normalRectangle);
                        if (direction.X > 0)
                        {
                            rect.X += 1.5;
                        }
                        else
                        {
                            rect.X -= 1.5;
                        }
                        sphereBBomb[i] = new GuidedBomb(scene, (Hashtable)o, rect, player.id);
                        sphereBBomb[i].Follow(this);
                        scene.EntityList.Add(sphereBBomb[i]);
                        Cue c = GLOBAL.soundBank.GetCue("B");
                        c.Play();
                    }
                }

            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic

            endAnimation = true;

            if (CheckFloat(gameTime))
            {
                AnimFloat(gameTime);
            }
            else
            {
                AnimStop(gameTime);
            }

            mi.rotation = Vector3.Zero;

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.B,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;
            }

            #endregion

            return true;
        }
        
        bool SphereUpBAnimation(GameTime gameTime)
        {
            return CubeUpBAnimation(gameTime);
        }

        bool SphereDownBAnimation(GameTime gameTime)
        {
            return CubeDownBAnimation(gameTime);
        }

        GuidedBomb[] sphereSBBomb = new GuidedBomb[10];
        int sphereSBCnt = 0;
        double sphereSBRad = 10;
        double sphereSBAngle = 0;
        double sphereSBDAngle = 0;
        double sphereSBTimeSpan = 0.3;
        bool SphereSpecialBAnimation(GameTime gameTime)
        {
            #region 1.初期化
            if (sphereSBCnt <= 0 && time <= gameTime.ElapsedGameTime)
            {
                mi.recVisible.Copy(normalRectangle);
                mi.depth = 1;
                sphereSBCnt = 0;
                sphereSBDAngle = 360.0 / (double)sphereSBBomb.Length * (direction.X > 0 ? 1 : -1);
                sphereSBAngle = (direction.X > 0 ? 0 : 180);
                time = TimeSpan.FromSeconds(sphereSBTimeSpan);
                gage = 0;
            }

            #endregion

            #region 2.アニメーション

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Write your logic
            if (time >= TimeSpan.FromSeconds(sphereSBTimeSpan))
            {
                Object o = scene.Parameters["SpecialGuidedBomb"];
                if (o != null)
                {
                    double x = mi.recCollision.Center.X + sphereSBRad * Math.Cos(MathHelper.ToRadians((float)sphereSBAngle));
                    double y = mi.recCollision.Center.Y + sphereSBRad * Math.Sin(MathHelper.ToRadians((float)sphereSBAngle));
                    RectangleD rect = new RectangleD(x - 0.5, y - 0.5, 1, 1);
                    sphereSBBomb[sphereSBCnt] = new GuidedBomb(scene, (Hashtable)o, rect, player.id);
                    scene.EntityList.Add(sphereSBBomb[sphereSBCnt]);
                }
                sphereSBAngle += sphereSBDAngle;
                sphereSBCnt++;
                time = TimeSpan.Zero;
                Cue c = GLOBAL.soundBank.GetCue("B");
                c.Play();
            }

            if (sphereSBCnt >= sphereSBBomb.Length)
            {
                endAnimation = true;
            }

            #endregion

            #region 3、次のアニメーションへの遷移判定
            Animation prevAnim = anim;
            NextAnimation[] nextAnim = new NextAnimation[]{
                new NextAnimation(Animation.Dead,false,CheckDead,true),
                new NextAnimation(Animation.Attacked,false,CheckAttacked,true),
                new NextAnimation(Animation.Ground,false,CheckGround,endAnimation),
                new NextAnimation(Animation.Float,false,CheckFloat,endAnimation),
                new NextAnimation(Animation.Run,true,null,endAnimation),
                new NextAnimation(Animation.Walk,true,null,endAnimation),
                new NextAnimation(Animation.Stop,false,null,endAnimation),
                new NextAnimation(Animation.SpecialB,false,null,true),
            };

            anim = FindNextAnimation(nextAnim, gameTime);
            if (anim == Animation.None)
            {
                anim = prevAnim;
            }

            #endregion

            #region 4、アニメーションが遷移するときの処理
            if (anim != prevAnim)
            {
                commandFlg[(int)Animation.B] = false;
                commandFlg[(int)Animation.UpB] = false;
                commandFlg[(int)Animation.DownB] = false;
                commandFlg[(int)Animation.SpecialB] = false;
                sphereSBCnt = 0;
            }

            #endregion

            return true;
        }

        #endregion
    }
}
