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
    /// パーティクルを生成するクラス（の継承元）
    /// ParticleSystemsにある各クラスを使って適宜エフェクトをつくっていく。
    /// </summary>
    public class ParticleManager : Entity
    {
        protected Random random = new Random();

        TimeSpan time;
        double aliveTime = 1.0;     // パーティクルを出し切るまでの時間
        int cnt = 30;               // 出すべきパーティクルの数
        double dt = 0.1, nextTime = 0;
        ParticleSystem particles;

        public ParticleManager(Fight scene, string type, Vector2 pos)
            : base(scene, null)
        {
            RectangleD position = new RectangleD();
            position.Locate(pos);
            model.Add("model", new ModelInfo("", Vector3.Zero, RectangleD.Empty, 0, position, Vector3.Zero, Vector3.Zero));
            time = TimeSpan.Zero;
            Object o;

            switch (type)
            {
                case "ExplosionParticles":
                    particles = GLOBAL.explosionParticles;
                    break;
                case "DeadParticles":
                    particles = GLOBAL.deadParticles;
                    break;
                case "AttackedParticles":
                case "AbsorbParticles":
                    particles = GLOBAL.attackedParticles;
                    break;
                case "SmokeParticles":
                    particles = GLOBAL.smokeParticles;
                    break;
                default:
                    particles = GLOBAL.explosionParticles;
                    break;
            }

            o = scene.Parameters[type];
            if (o != null)
            {
                parameters = (Hashtable)o;
                cnt = int.Parse((o = parameters["cnt"]) != null ? (string)o : "30");
                aliveTime = double.Parse((o = parameters["aliveTime"]) != null ? (string)o : "1");
                z = double.Parse((o = parameters["z"]) != null ? (string)o : "2");
                dt = aliveTime / cnt;
                nextTime = 0;
            }
        }

        double z;

        public override bool Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime;

            Chase();

            while (time.TotalSeconds >= nextTime)
            {
                Vector2 position = RecCollision.Position();
                particles.AddParticle(new Vector3(position, (float)z), Vector3.Zero);
                nextTime += dt;
            }

            return time < TimeSpan.FromSeconds(aliveTime);
        }
        public override void Draw()
        {
            Matrix view = scene.Camera.View;
            Matrix projection = scene.Camera.Projection;
            particles.SetCamera(view, projection);
        }
    }
}
