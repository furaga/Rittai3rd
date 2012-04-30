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
    public class ModelInfo
    {
        public string name;             // モデルの名前
        public Model model;             // モデル
        public Vector3 originalSize;    // モデルの元々の大きさ
        public RectangleD recVisible, defaultRecVisible;    // モデルはこの四角形(*depth)にすっぽり入るように描画する。ただし、重心のZ座標は常に０
        public double depth, defaultDepth;            // モデルの奥行き（Z軸の幅）
        public RectangleD recCollision, defaultRecCollision;  // 衝突判定用の四角形
        public Vector3 rotation, defaultRotation;        // モデルの回転角度
        public Vector3 emissiveColor, defaultEmissiveColor;    // モデルの発する光

        public int addAlpha;    // 加算ブレンドのときのブレンド値（1 ~ 255のとき加算ブレンドにする）

        public ModelInfo( string name, Vector3 originalSize, RectangleD recVisible, double depth, RectangleD recCollision, Vector3 rotation, Vector3 emissiveColor )
        {
            this.name = name;
            this.model = null;
            this.originalSize = originalSize;
            this.recVisible = recVisible;
            this.defaultRecVisible = new RectangleD(recVisible);
            this.depth = depth;
            this.defaultDepth = depth;
            this.recCollision = recCollision;
            this.defaultRecCollision = new RectangleD(recCollision);
            this.rotation = rotation;
            this.defaultRotation = new Vector3(rotation.X, rotation.Y, rotation.Z);
            this.emissiveColor = emissiveColor;
            this.defaultEmissiveColor = new Vector3(emissiveColor.X, emissiveColor.Y, emissiveColor.Z);
            this.addAlpha = -1;
            Load();
        }

        public void Load()
        {
            if (name != null && name.Length >= 1)
            {
                model = GLOBAL.game1.Content.Load<Model>(name);
            }
        }
    }

    /// <summary>
    /// 注意！！
    /// 初期化後にmodel連想配列の要素を変更したい場合
    /// 
    ///     ModelInfo mi = model[ "model" ];
    ///     /* miの編集 */
    ///     model["model"] = mi;
    /// 
    /// と、一旦別の変数に退避させて、最後に再び代入しなくてはならない。
    /// </summary>

    public class Entity
    {
        protected Fight scene;
        protected Hashtable parameters;
        protected Hashtable model;
        public RectangleD RecVisible
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).recVisible;
                }
                return RectangleD.Empty;
            }
            set
            {
                Object o = model["model"];
                if (o != null)
                {
                    ModelInfo md = (ModelInfo)o;
                    md.recVisible = value;
                }
            }
        }
        public RectangleD RecCollision
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).recCollision;
                }
                return RectangleD.Empty;
            }
            set
            {
                Object o = model["model"];
                if (o != null)
                {
                    ModelInfo md = (ModelInfo)o;
                    md.recCollision = value;
                }
            }
        }
        public Vector3 Rotation
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).rotation;
                }
                return Vector3.Zero;
            }
        }
        public double Depth
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).depth;
                }
                return 1;
            }
            set
            {
                Object o = model["model"];
                if (o != null)
                {
                    ModelInfo md = (ModelInfo)o;
                    md.depth = value;
                }
            }
        }
        public Vector3 EmissiveColor
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).emissiveColor;
                }
                return Vector3.Zero;
            }
            set
            {
                Object o = model["model"];
                if (o != null)
                {
                    ModelInfo md = (ModelInfo)o;
                    md.emissiveColor = value;
                }
            }
        }

        protected bool isAlive = true;
        public bool IsAlive
        {
            get
            {
                return isAlive;
            }
        }
        
        public string Name
        {
            get
            {
                Object o = model["model"];
                if (o != null)
                {
                    return ((ModelInfo)o).name;
                }
                return "";
            }
        }
        public Entity()
        {

        }

        /// <summary>
        /// scene, parameterのコピーとデバッグ用のモデルのロード
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="parameters"></param>
        public Entity(Fight scene, Hashtable parameters)
        {
            this.scene = scene;
            this.parameters = parameters;
            Initialize();
        }

        public void Initialize()
        {
            model = new Hashtable();
#if _DEBUG
            model.Add("recVisible", new ModelInfo("Board", new Vector3(20.0f, 20.0f, 0.0f), RectangleD.Empty, 0.0, RectangleD.Empty, GLOBAL.vec3Zero, new Vector3(0.0f,1.0f,0.0f)));
            model.Add("recCollision", new ModelInfo("Board", new Vector3(20.0f, 20.0f, 0.0f), RectangleD.Empty, 0.0, RectangleD.Empty, GLOBAL.vec3Zero, new Vector3(1.0f,0.5f,0.0f)));
#endif
        }

        public void MakeModel(string paramName)
        {
            object o = null;
            if (paramName != "") paramName += "_";

            string name = (o = parameters[paramName + "name"]) != null ? (string)o :
                          (o = parameters["ModelDeadBlock"]) != null ? (string)o : "DeadBlock";
            Vector3 originalSize = new Vector3(
                float.Parse((o = parameters[paramName + "originalSizeX"]) != null ? (string)o : "200.0"),
                float.Parse((o = parameters[paramName + "originalSizeY"]) != null ? (string)o : "200.0"),
                float.Parse((o = parameters[paramName + "originalSizeZ"]) != null ? (string)o : "200.0")
                );
            RectangleD recVisible = new RectangleD(
                float.Parse((o = parameters[paramName + "recVisibleX"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recVisibleY"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recVisibleW"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recVisibleH"]) != null ? (string)o : "0.0")
                );
            float depth = float.Parse((o = parameters[paramName + "depth"]) != null ? (string)o : "1.0");
            RectangleD recCollision = new RectangleD(
                float.Parse((o = parameters[paramName + "recCollisionX"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recCollisionY"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recCollisionW"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "recCollisionH"]) != null ? (string)o : "0.0")
                );
            Vector3 rotation = new Vector3(
                float.Parse((o = parameters[paramName + "rotationX"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "rotationY"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters[paramName + "rotationZ"]) != null ? (string)o : "0.0")
                );
            Vector3 emissiveColor = new Vector3(
                float.Parse((o = parameters[paramName + "emissiveColorX"]) != null ? (string)o : "1.0"),
                float.Parse((o = parameters[paramName + "emissiveColorY"]) != null ? (string)o : "1.0"),
                float.Parse((o = parameters[paramName + "emissiveColorZ"]) != null ? (string)o : "1.0")
                );
            model.Add("model", new ModelInfo(name, originalSize, recVisible, depth, recCollision, rotation, emissiveColor));
        }


        /// <summary>
        /// false を返したらそのフレームでオブジェクトは死ぬ
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public virtual bool Update(GameTime gameTime)
        {
#if _DEBUG
            // デバッグ処理
            Object o = model["model"];
            if (o != null)
            {
                ModelInfo m = (ModelInfo)o;
                string[] names = { "recVisible", "recCollision" };
                foreach (string name in names)
                {
                    o = model[name];
                    if (o == null)
                    {
                        continue; ;
                    }
                    ModelInfo mi = (ModelInfo)o;
                    if ( name == "recVisible" ) mi.recVisible = m.recVisible;
                    else mi.recVisible = m.recCollision;
                }
            }

#endif
            return isAlive;
        }

        public void Dispose()
        {
            isAlive = false;
        }

        public virtual void Draw()
        {
            DrawModel("model");
#if _DEBUG
            // デバッグ処理
            DrawModel("recVisible");
            DrawModel("recCollision");
#endif
        }

        public void DrawModel(string name)
        {
            Object o = model[name];
            if (o == null)
            {
                return;
            }
            ModelInfo mi = (ModelInfo)o;
            if (mi.model == null) return;
            
            GraphicsDevice graphic = GLOBAL.game1.GraphicsDevice;

            RectangleD recV;
            Vector3 oSize;
            ModelInfo mi2;
            int add = -1;
            switch (name)
            {
                case "recVisible":
                    o = model["model"];
                    if (o == null)
                    {
                        return;
                    }
                    mi2 = (ModelInfo)o;
                    recV = mi2.recVisible;
                    oSize = mi.originalSize;
                    break;
                case "recCollision":
                    o = model["model"];
                    if (o == null)
                    {
                        return;
                    }
                    mi2 = (ModelInfo)o;
                    recV = mi2.recCollision;
                    oSize = mi.originalSize;
                    break;
                default:
                    recV = mi.recVisible;
                    oSize = mi.originalSize;
                    add = mi.addAlpha;
                    break;
            }

            Vector2 v = recV.Center;
            Vector3 position = new Vector3(v.X, v.Y, 0.0f);

            Vector3 scale = new Vector3((float)recV.Width / oSize.X, (float)recV.Height / oSize.Y, (oSize.Z != 0.0f ? (float)mi.depth / oSize.Z : (float)recV.Width / oSize.X));
  
            Matrix world = Matrix.CreateScale(scale)
                *Matrix.CreateRotationZ(MathHelper.ToRadians(mi.rotation.Z))
                * Matrix.CreateRotationY(MathHelper.ToRadians(mi.rotation.Y))
                * Matrix.CreateRotationX(MathHelper.ToRadians(mi.rotation.X))
                * Matrix.CreateTranslation(position);

            Matrix view = scene.Camera.View;

            Matrix projection = scene.Camera.Projection;

            /*
            if (0 <= add && add <= 255)
            {
                graphic.BlendFactor = Blend.SourceAlpha
                graphic.BlendState = BlendState.Additive;
            }
            */
            foreach (ModelMesh mesh in mi.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EmissiveColor = mi.emissiveColor;
                    
                    if (0 <= add && add <= 255)
                    {
                        effect.GraphicsDevice.BlendState = BlendState.Additive;
                        effect.Alpha = (float)mi.addAlpha / 255.0f;
                    }
                    else
                    {
                        effect.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        effect.Alpha = 1;
                    }
                     
                }
                mesh.Draw();
            }
        }

        public Entity target = null;
        public Vector2 vec;
        public ModelInfo mi;

        public void Follow(Entity he)
        {
            Object o = model["model"];
            if (o == null)
            {
                return;
            }

            mi = (ModelInfo)o;

            target = he;

            vec = mi.recCollision.Center - target.RecCollision.Center;
        }

        public void UnFollow()
        {
            target = null;
        }

        public void Chase()
        {
            Object o = model["model"];
            if (o == null)
            {
                return;
            }

            ModelInfo mi = (ModelInfo)o;

            if (target != null)
            {
                Vector2 v = target.RecCollision.Center + vec;
                mi.recCollision.Locate(v.X - mi.recCollision.Width * 0.5f, v.Y - mi.recCollision.Height * 0.5f);
            }
        }
    }
}
