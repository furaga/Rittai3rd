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
    public enum Animation
    {
        Stop = 0,   // 静止
        Walk,       // 歩行
        Run,        // 走行
        Jump,       // 跳躍
        Float,      // 浮遊
        SlideS,     // 浮遊中の横移動
        Fall,       // 浮遊中の高速落下（空中で↓ボタン）
        Ground,     // 着地;
        Climb,      // 壁の駆け上がり;
        Sit,        // しゃがみ
        Shield,     // シールド
        AvoidG,     // その場緊急回避
        AvoidR,     // 横緊急回避
        AvoidS,     // 空中緊急回避
        Brown,      // 吹っ飛ばされ
        BreakFall,  // 受け身
        BreakFallR, // 横受け身;
        Down,       // 倒れている;
        Bump,       // 跳ね返り;
        Attacked,   // 攻撃された
        Dead,       // 死亡;
        Reborn,     // 復活;
        A,          // A
        SideA,      // 横A
        UpA,        // 上A
        DownA,      // 下A
        SideSmash,  // 横スマッシュ
        UpSmash,    // 上スマッシュ
        DownSmash,  // 下スマッシュ
        B,          // B
        UpB,        // 上B
        DownB,      // 下B
        SpecialB,   // 特殊B（ゲージが溜まったとき）
        Size,       // 動作の数
        None        // 便宜上用意
    };

    public partial class Character : Entity
    {        
        InputManager input;
        TimeSpan time;
        
        #region 各種パラメータの宣言
        Entity dirIndicator = null;
        double dist = 0;

        /*
         * 残機
         */
        public int life = 3;

        /*
         * ゲージ
         */
        double gage = 0;                // ゲージ
        public int Gage
        {
            get
            {
                return (int)gage;
            }
        }
        double gageV = 1.0;             // ゲージのたまる速さ
        public double MAX_GAGE = 1000;         // ゲージの最大値

        public int Damage
        {
            get
            {
                return (int)damage;
            }
        }
        /*
         * アニメーション
         */
        // 共通して使用する変数
        Animation anim = Animation.Stop;
        Vector2 velocity;               // 速度
        Vector3 direction;              // どの方向を向いているか（ふつうはX軸の正か負の方向）
        RectangleD normalRectangle;     // 回転・拡大・縮小されていないと仮定したときの矩形の位置
        bool endAnimation = false;      // アニメーションが終わったかどうかのフラグ
        int tmpJumpFlg = 0;             // ジャンプフラグの退避用変数
        AttackArea attackArea = null;   // 見えない攻撃範囲（通常攻撃などで使用）
        double ex = 1.0, ey = 1.0;      // 拡大率
        Vector2 tmpVelocity;            // 速度の退避用変数
        double ox, oy;                  // 差分
        int pileFlg = 0;                // スマッシュのアニメーションに使うフラグ
        const int piling = 0;           // タメ中
        const int finishPiling = 1;     // 貯め終わった初めのフレーム
        const int afterPiling = 2;      // 貯め終わった2回目以降のフレーム
        double maxPileTime;             // ため時間の上限
        Vector3 tmpEmissiveColor;       // EmissiveColorの退避用変数
        
        
        // Walk
        double walkAccel;               // 歩きの加速度
        double maxWalkSpeed;            // 歩く速さの上限

        // Run
        double runAccel;                // 走りの加速度
        double maxRunSpeed;             // 走る速さの上限

        // Jump
        double jumpSpeed;               // ジャンプの初速度
        double gravity;                 // 重力加速度
        double jumpTime1;               // ジャンプの溜めの時間
        double jumpA;                   // ジャンプの溜めのときのモデル伸縮幅
        int jumpFlg;                    // 今、何段目のジャンプをしているか
        int MAX_JUMP;                   // 何段ジャンプが出来るか        
        double jumpSpeed1 = 1.8;
        double jumpSpeed2 = 0.8;

        // Stop
        double stopAccel = 5.0;         // 静止しようとする加速度

        // Float
        double slideAccel = 1.0;        // 浮遊時に横に動こうしたときの加速度
        double fallAccel = 1.0;         // 高速落下しているときの落下スピード 
        double maxSlideSpeed = 1.0;     // 浮遊時の横方向の速度の最大値
        double maxFallSpeed = 3.0;      // 落下速度の最大値
        double floatRot = 0;            // 二段目以降のジャンプでの回転角度
        double floatRotSpeed = 360;     // 二段目以降のジャンプでの回転速度

        // Ground
        double groundTime;              // アニメーションの時間
        double groundA;                 // 伸縮幅
        double maxGroundSpeed;          // 横方向の最大速度
        double groundAccel;             // 横方向の加速度

        double climbRot = 0.0;                  // 回転角度。Degree
        double climbRotSpeed = 1000.0;   // 回転速度。Degree
        double sitTime;                         // アニメーションの時間
        double sitA;

        double avoidGTime = 0.5;        // アニメーションの時間
        
        double avoidRSpeed = 1.0;
        double avoidRTime = 0.5;        // アニメーションの時間
        
        double avoidSSpeed = 1.0;
        double avoidSTime = 0.5;        // アニメーションの時間    
        
        double maxBrownSpeed = 5.0;     // 最大の吹っ飛び速度
        double brownFallAccel = 1.0;    // 下ボタンを押しているときの重力加速度
        double brownSlideAccel = 0.5;   // 横ボタンを押しているときの横方向の加速度

        double breakFallSpeed = 1.0;    // 跳ね返りの速さ
        double maxBreakFallSpeed = 3.0; // 衝突面と平行な方向の速度の最大値

        double checkDownSpeedSquared = 25;

        double attackedTime = 0.1;      // アニメーションの時間

        double deadTime = 1.0;          // アニメーションの時間
        RectangleD aliveZone;           // この外に出れば死ぬ

        Vector2 rebornPos;              // 復活するときの位置
                
        double ARot = 0;                // 回転角度
        double ARotSpeed = -1000;       // 回転速度
        double AeX = 1.2, AeY = 1.2;    // 攻撃範囲のRecCollisionに対する大きさ比率
        double ADamage, APower;

        double sideAeY = 0.9, sideAeX = 1 / 0.9 / 0.9;  // 攻撃範囲のRecCollisionに対する大きさ比率
        double sideASpeed;                              // 移動速度
        double sideAExtendSpeed;                        // 伸縮する速度
        double sideATime1;                              // 伸びきるのにかかる時間
        double sideATime2;                              // 伸びきってから縮み始めるまでの時間
        double sideATime3;                              // 縮みきるのにかかる時間
        double sideADamage, sideAPower;
        
        double upAeX = 0.9, upAeY = 1 / 0.9 / 0.9;      // 攻撃範囲のRecCollisionに対する大きさ比率
        double upAExtendSpeed;                          // 伸縮する速度
        double upATime1;                                // 伸びきるのにかかる時間
        double upATime2;                                // 伸びきってから縮み始めるまでの時間
        double upATime3;                                // 縮みきるのにかかる時間
        double upADamage, upAPower;
            
        double downAeX = 0.9, downAeY = 1 / 0.9 / 0.9;  // 攻撃範囲のRecCollisionに対する大きさ比率
        double downAExtendSpeed;                        // 伸縮する速度
        double downATime1;                              // 縮みきるのにかかる時間
        double downATime2;                              // 縮みきってから伸び始めるまでの時間
        double downATime3;                              // 伸びきるのにかかる時間
        double downADamage, downAPower;

        double sideSmashTimeRatio = 1;               // ため終わった後のアニメーションの時間
        double sideSmashRot = 0;                // 回転角度
        double sideSmashRotSpeed = 0;           // 回転速度
        double sideSmashRotAccel = 1.0;         // 回転加速度
        double sideSmashRotSpeedRatio = 3.0;    // タメ時間に対する回転速度の比率
        double sideSmashSpeed = 1.0;            // 移動速度
        double sideSmashSpeedRatio = 3.0;       // タメ時間に対する移動速度の比率
        double minSideSmashSpeed = 1.0;         // 移動速度の最小値
        double maxSideSmashSpeed = 10.0;        // 移動速度の最大値
        double sideSmasheX, sideSmasheY;        // 攻撃範囲のRecCollisionに対する大きさ比率
        double minSideSmashDamage, minSideSmashPower;
        double maxSideSmashDamage, maxSideSmashPower;

        double upSmashTime = 1.0;               // ため終わった後のアニメーションの時間
        double minUpSmashDamage, minUpSmashPower;
        double maxUpSmashDamage, maxUpSmashPower;

        double downSmashTime = 1.0;             // ため終わった後のアニメーションの時間
        double minDownSmashDamage, minDownSmashPower;
        double maxDownSmashDamage, maxDownSmashPower;
        
        #endregion

        #region 各種パラメータの初期化

        void InitializePrameters()
        {
            Object o = null;

            time = TimeSpan.Zero;
            input = GLOBAL.inputManager;

            // モデルの作成とパラメータの設定
            string ch = (player.character == CHARACTER.CUBE ? "ModelCube" : "ModelSphere");
            MakeModel(ch);
            
            string name = (o = parameters["DirectionIndicator_name"]) != null ? (string)o : "";
            Vector3 originalSize = new Vector3(
                float.Parse((o = parameters["DirectionIndicator_originalSizeX"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters["DirectionIndicator_originalSizeY"]) != null ? (string)o : "0.0"),
                float.Parse((o = parameters["DirectionIndicator_originalSizeZ"]) != null ? (string)o : "0.0")
                );
            RectangleD recVisible = new RectangleD(
                double.Parse((o = parameters["DirectionIndicator_recVisibleX"]) != null ? (string)o : "0.0"),
                double.Parse((o = parameters["DirectionIndicator_recVisibleY"]) != null ? (string)o : "0.0"),
                double.Parse((o = parameters["DirectionIndicator_recVisibleW"]) != null ? (string)o : "0.0"),
                double.Parse((o = parameters["DirectionIndicator_recVisibleH"]) != null ? (string)o : "0.0")
                );
            float depth = float.Parse((o = parameters["DirectionIndicator_depth"]) != null ? (string)o : "1.0");
            Vector3 rotation = Vector3.Zero;
            Vector3 emissiveColor;
            
            mi = ((ModelInfo)model["model"]);
            if (player.id == 0)
            {
                mi.emissiveColor = new Vector3(
                    float.Parse((o = parameters["1P_emissiveColorX"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["1P_emissiveColorY"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["1P_emissiveColorZ"]) != null ? (string)o : "1.0")
                    );
                mi.recVisible = new RectangleD(
                    double.Parse((o = parameters["1P_recVisibleX"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["1P_recVisibleY"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["1P_recVisibleW"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["1P_recVisibleH"]) != null ? (string)o : "0.0")
                );
                direction = new Vector3(1, 0, 0);
                rotation = new Vector3(0, 0, 0);
            }
            else
            {
                mi.emissiveColor = new Vector3(
                    float.Parse((o = parameters["2P_emissiveColorX"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["2P_emissiveColorY"]) != null ? (string)o : "1.0"),
                    float.Parse((o = parameters["2P_emissiveColorZ"]) != null ? (string)o : "1.0")
                    );
                mi.recVisible = new RectangleD(
                    double.Parse((o = parameters["2P_recVisibleX"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["2P_recVisibleY"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["2P_recVisibleW"]) != null ? (string)o : "0.0"),
                    double.Parse((o = parameters["2P_recVisibleH"]) != null ? (string)o : "0.0")
                );
                direction = new Vector3(-1, 0, 0);
                rotation = new Vector3(0, 180, 0);
            }

            mi.recCollision = mi.recVisible;
            recVisible.Offset(mi.recVisible.Center.X - recVisible.Center.X, 0);

            emissiveColor = mi.emissiveColor;
            
            model.Add("DirectionIndicator", new ModelInfo(name, originalSize, recVisible, depth, RectangleD.Empty, rotation, emissiveColor));

            dist = ((ModelInfo)model["DirectionIndicator"]).recVisible.Bottom3D - RecVisible.Top3D;

            normalRectangle = new RectangleD(mi.recVisible);

            cnt_ColStateWithAttacker = cnt_ColStateWithSolid = 0;
            MAX_COLSTATEWithAttacker = MAX_COLSTATEWithSolid = 60;
            colStateWithAttacker = new Hashtable[MAX_COLSTATEWithAttacker];
            colStateWithSolid = new Hashtable[MAX_COLSTATEWithSolid];

            MAX_ADJUSTTOHIST = 60;
            adjustToHist = new int[MAX_ADJUSTTOHIST];
            for (int i = 0; i < adjustToHist.Length; i++)
            {
                adjustToHist[i] = -1;
            }

            life = int.Parse((o = parameters["life"]) != null ? (string)o : "3");

            /*
             * ゲージ
             */ 
            gage = 0;
            gageV = double.Parse((o = parameters["gageV"]) != null ? (string)o : "1.0");
            MAX_GAGE = double.Parse((o = parameters["MAX_GAGE"]) != null ? (string)o : "100.0");
        
            /*
             * アニメーション
             */
            // 共通して使用する変数
            anim = Animation.Stop;
            velocity = Vector2.Zero;               // 速度
            endAnimation = false;      // アニメーションが終わったかどうかのフラグ
            tmpJumpFlg = 0;            // ジャンプフラグの退避用変数
            attackArea = null;   // 見えない攻撃範囲（通常攻撃などで使用）
            ex = ey = 1;      // 拡大率
            tmpVelocity = Vector2.Zero;            // 速度の退避用変数
            ox =  oy = 0;                  // 差分
            pileFlg = 0;                // スマッシュのアニメーションに使うフラグ
            maxPileTime = double.Parse((o = parameters["maxPileTime"]) != null ? (string)o : "3.0");             // ため時間の上限
            tmpEmissiveColor = Vector3.Zero;       // EmissiveColorの退避用変数

            // Stop
            stopAccel = double.Parse((o = parameters["stopAccel"]) != null ? (string)o : "1.0");         // 静止しようとする加速度
            
            // Walk
            maxWalkSpeed = double.Parse((o = parameters["maxWalkSpeed"]) != null ? (string)o : "1.0");  // 歩く速さの上限
            double peekTime = double.Parse((o = parameters["walkPeekTime"]) != null ? (string)o : "1.0");
            walkAccel = maxWalkSpeed / peekTime;            // 歩きの加速度

            // Run
            maxRunSpeed = double.Parse((o = parameters["maxRunSpeed"]) != null ? (string)o : "3.0");// 走りの加速度
            peekTime = double.Parse((o = parameters["runPeekTime"]) != null ? (string)o : "1.0");   // 走る速さの上限
            runAccel = maxRunSpeed / peekTime;

            // Jump
            jumpTime1 = double.Parse((o = parameters["jumpTime1"]) != null ? (string)o : "0");               // ジャンプの溜めの時間
            jumpA = double.Parse((o = parameters["jumpA"]) != null ? (string)o : "0");                   // ジャンプの溜めのときのモデル伸縮幅
            jumpFlg = 0;                    // 今、何段目のジャンプをしているか
            MAX_JUMP = int.Parse((o = parameters["MAX_JUMP"]) != null ? (string)o : "2");                   // 何段ジャンプが出来るか    
            peekTime = double.Parse((o = parameters["jumpPeekTime1"]) != null ? (string)o : "1.0");
            double peekHeight = double.Parse((o = parameters["jumpPeekHeight1"]) != null ? (string)o : "1.0");
            jumpSpeed1 = 2 * peekHeight / peekTime;
            jumpSpeed = jumpSpeed1;
            gravity = jumpSpeed1 / peekTime;
            peekTime = double.Parse((o = parameters["jumpPeekTime2"]) != null ? (string)o : "1.0");
            peekHeight = double.Parse((o = parameters["jumpPeekHeight2"]) != null ? (string)o : "1.0");
            jumpSpeed2 = 2 * peekHeight / peekTime;
            maxJumpSpeedX = double.Parse((o = parameters["maxJumpSpeedX"]) != null ? (string)o : "3.0");

             // Float
            peekTime = double.Parse((o = parameters["slidePeekTime"]) != null ? (string)o : "1.0");
            maxSlideSpeed = double.Parse((o = parameters["maxSlideSpeed"]) != null ? (string)o : "1.0");
            slideAccel = maxSlideSpeed / peekTime;        // 浮遊時に横に動こうしたときの加速度
            maxFallSpeed = double.Parse((o = parameters["maxFallSpeed"]) != null ? (string)o : "1.0"); 
            fallAccel = gravity * double.Parse((o = parameters["fallAccel"]) != null ? (string)o : "1.0"); ;     // 高速落下しているときの落下スピード (gravityに対する比率で指定)
            floatRot = 0;            // 二段目以降のジャンプでの回転角度
            floatRotSpeed = double.Parse((o = parameters["floatRotSpeed"]) != null ? (string)o : "360.0"); ;     // 二段目以降のジャンプでの回転速度

            // Ground
            groundTime = double.Parse((o = parameters["groundTime"]) != null ? (string)o : "1");              // アニメーションの時間
            groundA = double.Parse((o = parameters["groundA"]) != null ? (string)o : "0.2");                 // 伸縮幅
            maxGroundSpeed = double.Parse((o = parameters["maxGroundSpeed"]) != null ? (string)o : "3");          // 横方向の最大速度
            groundAccel = double.Parse((o = parameters["groundAccel"]) != null ? (string)o : "1");             // 横方向の加速度

            // Climb
            climbRot = 0.0;                                                                                 // 回転角度。Degree
            climbRotSpeed = double.Parse((o = parameters["climbRotSpeed"]) != null ? (string)o : "1000.0"); // 回転速度。Degree

            // Sit
            sitTime = double.Parse((o = parameters["sitTime"]) != null ? (string)o : "0");                         // アニメーションの時間
            sitA = double.Parse((o = parameters["sitA"]) != null ? (string)o : "0");

            avoidGTime = double.Parse((o = parameters["avoidGTime"]) != null ? (string)o : "0");        // アニメーションの時間
        
            avoidRSpeed = double.Parse((o = parameters["avoidRSpeed"]) != null ? (string)o : "0");
            avoidRTime = double.Parse((o = parameters["avoidRTime"]) != null ? (string)o : "0");        // アニメーションの時間
        
            avoidSSpeed = double.Parse((o = parameters["avoidSSpeed"]) != null ? (string)o : "0");
            avoidSTime = double.Parse((o = parameters["avoidSTime"]) != null ? (string)o : "0");        // アニメーションの時間    

            brownTime = double.Parse((o = parameters["brownTime"]) != null ? (string)o : "0");
            maxBrownTime = double.Parse((o = parameters["maxBrownTime"]) != null ? (string)o : "0");
            minBrownTime = double.Parse((o = parameters["minBrownTime"]) != null ? (string)o : "0");
            maxGetDamage = double.Parse((o = parameters["maxGetDamage"]) != null ? (string)o : "0"); ;
            maxBrownSpeed = double.Parse((o = parameters["maxBrownSpeed"]) != null ? (string)o : "0");     // 最大の吹っ飛び速度
            brownFallAccel = gravity * double.Parse((o = parameters["brownFallAccel"]) != null ? (string)o : "1");    // 下ボタンを押しているときの重力加速度
            brownSlideAccel = slideAccel * double.Parse((o = parameters["brownSlideAccel"]) != null ? (string)o : "1");   // 横ボタンを押しているときの横方向の加速度

            brownRotSpeed = 0;
            _brownRotSpeed = double.Parse((o = parameters["brownRotSpeed"]) != null ? (string)o : "0");
            brownRot = 0;

            breakFallSpeed = double.Parse((o = parameters["breakFallSpeed"]) != null ? (string)o : "0");    // 跳ね返りの速さ
            maxBreakFallSpeed = double.Parse((o = parameters["maxBreakFallSpeed"]) != null ? (string)o : "0"); // 衝突面と平行な方向の速度の最大値

            checkDownSpeedSquared = double.Parse((o = parameters["checkDownSpeedSquared"]) != null ? (string)o : "25");
            
            attackedTime = double.Parse((o = parameters["attackedTime"]) != null ? (string)o : "0.5");      // アニメーションの時間
            brownAngle = double.Parse((o = parameters["brownAngle"]) != null ? (string)o : "15");
            minAttackedRatio = double.Parse((o = parameters["minAttackedRatio"]) != null ? (string)o : "1");
            maxAttackedRatio = double.Parse((o = parameters["maxAttackedRatio"]) != null ? (string)o : "5");
            maxDamage = double.Parse((o = parameters["maxDamage"]) != null ? (string)o : "200");

            deadTime = double.Parse((o = parameters["deadTime"]) != null ? (string)o : "1.0");          // アニメーションの時間

            o = scene.Parameters[scene.Stage];
            if ( o != null )
            {
                Object o1;
                double y = double.Parse((o1 = ((Hashtable)o)["aliveZoneY"]) != null ? (string)o1 : "-100");
                double h = double.Parse((o1 = ((Hashtable)o)["aliveZoneH"]) != null ? (string)o1 : "200");
                double w = h * GLOBAL.WindowWidth / GLOBAL.WindowHeight;
                double x = -0.5 * w;

                aliveZone = new RectangleD(x, y, w, h);// この外に出れば死ぬ
                rebornPos = new Vector2(
                    float.Parse((o1 = ((Hashtable)o)["rebornPos" + player.id + "X"]) != null ? (string)o1 : "0"), 
                    float.Parse((o1 = ((Hashtable)o)["rebornPos" + player.id + "Y"]) != null ? (string)o1 : "10")
                    );
            }
            else
            {
                aliveZone = RectangleD.Empty;
                rebornPos = Vector2.Zero;
            }
                
            ARot = 0;                // 回転角度
            ARotSpeed = double.Parse((o = parameters["ARotSpeed"]) != null ? (string)o : "360");       // 回転速度
            AeX = double.Parse((o = parameters["AeX"]) != null ? (string)o : "1.2");    // 攻撃範囲のRecCollisionに対する大きさ比率
            AeY = double.Parse((o = parameters["AeY"]) != null ? (string)o : "1.2");    // 攻撃範囲のRecCollisionに対する大きさ比率
            ADamage = double.Parse((o = parameters["ADamage"]) != null ? (string)o : "5");
            APower = double.Parse((o = parameters["APower"]) != null ? (string)o : "10");

            sideAeY = double.Parse((o = parameters["sideAeY"]) != null ? (string)o : "0.9");
            sideAeX = 1 / sideAeY / sideAeY;  // 攻撃範囲のRecCollisionに対する大きさ比率
            sideASpeed = double.Parse((o = parameters["sideASpeed"]) != null ? (string)o : "0");                              // 移動速度
            sideATime1 = double.Parse((o = parameters["sideATime1"]) != null ? (string)o : "1");                              // 伸びきるのにかかる時間
            sideAExtendSpeed = (sideAeX - 1) / sideATime1;                                                                          // 伸縮する速度
            sideATime2 = double.Parse((o = parameters["sideATime2"]) != null ? (string)o : "1");                              // 伸びきってから縮み始めるまでの時間
            sideATime3 = double.Parse((o = parameters["sideATime3"]) != null ? (string)o : "1");                              // 縮みきるのにかかる時間
            sideADamage = double.Parse((o = parameters["sideADamage"]) != null ? (string)o : "5");
            sideAPower = double.Parse((o = parameters["sideAPower"]) != null ? (string)o : "10");
        
            upAeX = double.Parse((o = parameters["upAeY"]) != null ? (string)o : "0.9");
            upAeY = 1 / upAeX / upAeX;                                                                              // 攻撃範囲のRecCollisionに対する大きさ比率
            upATime1 = double.Parse((o = parameters["upATime1"]) != null ? (string)o : "1");                                // 伸びきるのにかかる時間
            upAExtendSpeed = (upAeY - 1) / upATime1;                          // 伸縮する速度
            upATime2 = double.Parse((o = parameters["upATime2"]) != null ? (string)o : "1");                                // 伸びきってから縮み始めるまでの時間
            upATime3 = double.Parse((o = parameters["upATime3"]) != null ? (string)o : "1");                                // 縮みきるのにかかる時間
            upADamage = double.Parse((o = parameters["upADamage"]) != null ? (string)o : "5");
            upAPower = double.Parse((o = parameters["upAPower"]) != null ? (string)o : "10");
 
            downAeY = double.Parse((o = parameters["downAeY"]) != null ? (string)o : "0.9");
            downAeX = 1 / downAeY / downAeY;  // 攻撃範囲のRecCollisionに対する大きさ比率
            downATime1 = double.Parse((o = parameters["downATime1"]) != null ? (string)o : "1");                              // 伸びきるのにかかる時間
            downAExtendSpeed = (downAeX - 1) / downATime1;                                                                          // 伸縮する速度
            downATime2 = double.Parse((o = parameters["downATime2"]) != null ? (string)o : "1");                              // 伸びきってから縮み始めるまでの時間
            downATime3 = double.Parse((o = parameters["downATime3"]) != null ? (string)o : "1");                              // 縮みきるのにかかる時間
            downADamage = double.Parse((o = parameters["downADamage"]) != null ? (string)o : "5");
            downAPower = double.Parse((o = parameters["downAPower"]) != null ? (string)o : "10");
  
            sideSmashTimeRatio = double.Parse((o = parameters["sideSmashTimeRatio"]) != null ? (string)o : "1");               // ため終わった後のアニメーションの時間
            sideSmashRot = 0;                // 回転角度
            sideSmashRotSpeed = 0;           // 回転速度
            sideSmashRotAccel = double.Parse((o = parameters["sideSmashRotAccel"]) != null ? (string)o : "1");         // 回転加速度
            sideSmashRotSpeedRatio = double.Parse((o = parameters["sideSmashRotSpeedRatio"]) != null ? (string)o : "3");    // タメ時間に対する回転速度の比率
            sideSmashSpeed = 0;            // 移動速度
            sideSmashSpeedRatio = double.Parse((o = parameters["sideSmashSpeedRatio"]) != null ? (string)o : "3");       // タメ時間に対する移動速度の比率
            minSideSmashSpeed = double.Parse((o = parameters["minSideSmashSpeed"]) != null ? (string)o : "3");         // 移動速度の最小値
            maxSideSmashSpeed = double.Parse((o = parameters["maxSideSmashSpeed"]) != null ? (string)o : "10");        // 移動速度の最大値
            sideSmasheX = double.Parse((o = parameters["sideSmasheX"]) != null ? (string)o : "0"); 
            sideSmasheY = double.Parse((o = parameters["sideSmasheY"]) != null ? (string)o : "0");        // 攻撃範囲のRecCollisionに対する大きさ比率
            minSideSmashDamage = double.Parse((o = parameters["minSideSmashDamage"]) != null ? (string)o : "5");
            minSideSmashPower = double.Parse((o = parameters["minSideSmashPower"]) != null ? (string)o : "10");
            maxSideSmashDamage = double.Parse((o = parameters["maxSideSmashDamage"]) != null ? (string)o : "5");
            maxSideSmashPower = double.Parse((o = parameters["maxSideSmashPower"]) != null ? (string)o : "10");
            maxSideSmashTime = double.Parse((o = parameters["maxSideSmashTime"]) != null ? (string)o : "1");
            minSideSmashTime = double.Parse((o = parameters["minSideSmashTime"]) != null ? (string)o : "1");
            sideSmashTime = 0;
 
            upSmashTime = double.Parse((o = parameters["upSmashTime"]) != null ? (string)o : "1");               // ため終わった後のアニメーションの時間
            minUpSmashDamage = double.Parse((o = parameters["minUpSmashDamage"]) != null ? (string)o : "5");
            minUpSmashPower = double.Parse((o = parameters["minUpSmashPower"]) != null ? (string)o : "10");
            maxUpSmashDamage = double.Parse((o = parameters["maxUpSmashDamage"]) != null ? (string)o : "5");
            maxUpSmashPower = double.Parse((o = parameters["maxUpSmashPower"]) != null ? (string)o : "10");
            upSmashRot = 0;
            upSmashRotSpeed = 0;
            upSmashRotAccel = double.Parse((o = parameters["upSmashRotAccel"]) != null ? (string)o : "360");
            maxUpSmashTime = double.Parse((o = parameters["maxUpSmashTime"]) != null ? (string)o : "10");
            minUpSmashTime = double.Parse((o = parameters["minUpSmashTime"]) != null ? (string)o : "10");
            upSmashA = 0;
            upSmasheX = double.Parse((o = parameters["upSmasheX"]) != null ? (string)o : "1.2");
            upSmasheY = double.Parse((o = parameters["upSmasheY"]) != null ? (string)o : "1.2");
            maxUpSmashA = double.Parse((o = parameters["maxUpSmashA"]) != null ? (string)o : "10");
            upSmashExtendSpeed = double.Parse((o = parameters["upSmashExtendSpeed"]) != null ? (string)o : "10");
            
            downSmashTime = double.Parse((o = parameters["downSmashTime"]) != null ? (string)o : "1");               // ため終わった後のアニメーションの時間
            minDownSmashDamage = double.Parse((o = parameters["minDownSmashDamage"]) != null ? (string)o : "5");
            minDownSmashPower = double.Parse((o = parameters["minDownSmashPower"]) != null ? (string)o : "10");
            maxDownSmashDamage = double.Parse((o = parameters["maxDownSmashDamage"]) != null ? (string)o : "5");
            maxDownSmashPower = double.Parse((o = parameters["maxDownSmashPower"]) != null ? (string)o : "10");
            downSmashRot = 0;
            downSmashRotSpeed = 0;
            downSmashRotAccel = double.Parse((o = parameters["downSmashRotAccel"]) != null ? (string)o : "360");
            maxDownSmashTime = double.Parse((o = parameters["maxDownSmashTime"]) != null ? (string)o : "10");
            minDownSmashTime = double.Parse((o = parameters["minDownSmashTime"]) != null ? (string)o : "10");
            downSmashA = 0;
            downSmasheX = double.Parse((o = parameters["downSmasheX"]) != null ? (string)o : "1.2");
            downSmasheY = double.Parse((o = parameters["downSmasheY"]) != null ? (string)o : "1.2");
            maxDownSmashA = double.Parse((o = parameters["maxDownSmashA"]) != null ? (string)o : "10");
            downSmashExtendSpeed = double.Parse((o = parameters["downSmashExtendSpeed"]) != null ? (string)o : "10");

            cubeBBomb = new CubicBomb[int.Parse((o = parameters["cubeBNum"]) != null ? (string)o : "5")];
            sphereBBomb = new GuidedBomb[int.Parse((o = parameters["sphereBNum"]) != null ? (string)o : "5")];

            cubeUpBA = double.Parse((o = parameters["cubeUpBA"]) != null ? (string)o : "0.3");
            cubeUpBTime1 = double.Parse((o = parameters["cubeUpBTime1"]) != null ? (string)o : "0.2");
            peekHeight = double.Parse((o = parameters["cubeUpBPeekHeight"]) != null ? (string)o : "10");
            cubeUpBTime2 = double.Parse((o = parameters["cubeUpBTime2"]) != null ? (string)o : "10");
            cubeUpBSpeed1 = peekHeight / cubeUpBTime2;
            cubeUpBSpeed2 = jumpSpeed1 * double.Parse((o = parameters["cubeUpBSpeed2"]) != null ? (string)o : "0.7");
            cubeUpBTime3 = double.Parse((o = parameters["cubeUpBTime3"]) != null ? (string)o : "10");
            cubeUpBBottom3D = 0;
            cubeUpBBottom3D2 = 0;

            cubeDownBMaxFallSpeed = double.Parse((o = parameters["cubeDownBMaxFallSpeed"]) != null ? (string)o : "5");
            cubeDownBEmissiveColor =  new Vector3(
                    float.Parse((o = parameters["cubeDownBEmissiveColorX"]) != null ? (string)o : "5"),
                    float.Parse((o = parameters["cubeDownBEmissiveColorY"]) != null ? (string)o : "5"),
                    float.Parse((o = parameters["cubeDownBEmissiveColorZ"]) != null ? (string)o : "5")
                    );
            attackedAbsorb = false;

            int maxBomb = int.Parse((o = parameters["maxCubeSBBomb"]) != null ? (string)o : "10");
            cubeSBBomb = new CubicBomb[maxBomb];
            cubeSBCnt = 0;
            cubeSBRad = double.Parse((o = parameters["cubeSBRad"]) != null ? (string)o : "10");
            cubeSBAngle = 0;
            cubeSBDAngle = 0;
            cubeSBTimeSpan = double.Parse((o = parameters["cubeSBTimeSpan"]) != null ? (string)o : "10");

            maxBomb = int.Parse((o = parameters["maxSphereSBBomb"]) != null ? (string)o : "10");
            sphereSBBomb = new GuidedBomb[maxBomb];
            sphereSBCnt = 0;
            sphereSBRad = double.Parse((o = parameters["sphereSBRad"]) != null ? (string)o : "10");
            sphereSBAngle = 0;
            sphereSBDAngle = 0;
            sphereSBTimeSpan = double.Parse((o = parameters["sphereSBTimeSpan"]) != null ? (string)o : "10");
        }

        #endregion

        #region コマンドの初期化

        string[] animationName;
        BUTTON[][][] command;
        string[] commandName;
        bool[] commandState;
        int[] debugId;

        void InitializeCommand()
        {
            animationName = new string[]
            {
                "Stop",   // 静止
                "Walk",       // 歩行
                "Run",        // 走行
                "Jump",       // 跳躍
                "Float",      // 浮遊
                "SlideS",     // 浮遊中の横移動
                "Fall",       // 浮遊中の高速落下（空中で↓ボタン）
                "Ground",     // 着地;
                "Climb",      // 壁の駆け上がり;
                "Sit",        // しゃがみ
                "Shield",     // シールド
                "AvoidG",     // その場緊急回避
                "AvoidR",     // 横緊急回避
                "AvoidS",     // 空中緊急回避
                "Brown",      // 吹っ飛ばされ
                "BreakFall",  // 受け身
                "BreakFallR", // 横受け身;
                "Down",       // 倒れている;
                "Bump",       // 跳ね返り;
                "Attacked",   // 攻撃された
                "Dead",       // 死亡;
                "Reborn",     // 復活;
                "A",          // A
                "SideA",      // 横A
                "UpA",        // 上A
                "DownA",      // 下A
                "SideSmash",  // 横スマッシュ
                "UpSmash",    // 上スマッシュ
                "DownSmash",  // 下スマッシュ
                "B",          // B
                "UpB",        // 上B
                "DownB",      // 下B
                "SpecialB",   // 特殊B（ゲージが溜まったとき）
                "Size",       // 動作の数
                "None"        // 便宜上用意
            };

            commandName = new string[]
            {
                "ComStop",   // 静止
                "ComWalk",       // 歩き
                "ComRun",        // 走り
                "ComJump",       // ジャンプ
                "ComFloat",      // 空中にいる
                "ComSlideS",     // 空中で横に動く
                "ComFall",       // 高速落下（空中で↓ボタン）
                "ComGround",     // 着地;
                "ComClimb",      // 壁の駆け上がり;
                "ComSit",        // しゃがみ
                "ComShield",     // シールド
                "ComAvoidG",     // その場緊急回避
                "ComAvoidR",     // 転がり緊急回避
                "ComAvoidS",     // 空中緊急回避
                "ComBrown",      // 吹っ飛ばされている
                "ComBreakFall",  // 受け身
                "ComBreakFallR", // 横受け身;
                "ComDown",       // 倒れている;
                "ComBump",       // 跳ね返り;
                "ComAttacked",   // 攻撃された瞬間
                "ComDead",       // 死亡;
                "ComReborn",     // 復活;
                "ComA",          // A
                "ComSideA",      // 横A
                "ComUpA",        // 上A
                "ComDownA",      // 下A
                "ComSideSmash",  // 横スマッシュ
                "ComUpSmash",    // 上スマッシュ
                "ComDownSmash",  // 下スマッシュ
                "ComB",          // B
                "ComUpB",        // 上B
                "ComDownB",      // 下B
                "ComSpecialB",   // 特殊B（ゲージが溜まったときの）
                "ComSize",       // 動作の数
                "ComNone"        // 便宜上用意
            };

            debugId = new int[128];
            for (int x = 0; x < debugId.Length; x++)
            {
                debugId[x] = -1;
            }
            
            commandState = new bool[(int)Animation.Size];
            commandFlg = new bool[(int)Animation.Size];
            command = new BUTTON[(int)Animation.Size][][];
            
            for (int x = 0; x < (int)Animation.Size; x++)
            {
                debugId[x] = -1;
                commandState[x] = false;
                commandFlg[x] = true;
                command[x] = null;
            }
            
            // 静止
            command[(int)Animation.Stop] = new BUTTON[][]{};

            // 歩き
            command[(int)Animation.Walk] = new BUTTON[][]{ 
                new BUTTON[]{BUTTON.LEFT},
                new BUTTON[]{BUTTON.RIGHT}
            };

            // 走り
            command[(int)Animation.Run] = new BUTTON[][]{
                new BUTTON[]{BUTTON.RB, BUTTON.LEFT},
                new BUTTON[]{BUTTON.LB, BUTTON.LEFT},
                new BUTTON[]{BUTTON.RB, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.LB, BUTTON.RIGHT}            
            };
            
            // ジャンプ
            command[(int)Animation.Jump] = new BUTTON[][]{
                new BUTTON[]{BUTTON.RB, BUTTON.UP},
                new BUTTON[]{BUTTON.LB, BUTTON.UP},
                new BUTTON[]{BUTTON.X},
                new BUTTON[]{BUTTON.Y}       
            };

            // 空中で横に動く
            command[(int)Animation.SlideS] = new BUTTON[][]{ 
                new BUTTON[]{BUTTON.LEFT},
                new BUTTON[]{BUTTON.RIGHT}
            };

            // 高速落下
            command[(int)Animation.Fall] = new BUTTON[][]{
                new BUTTON[]{BUTTON.DOWN}
            };
            
            // 着地;
            // command[(int)Animation.Ground] = new BUTTON[][]{};

            // 壁の駆け上がり;（必要ないからこれには遷移しない）
            // command[(int)Animation.Climb] = new BUTTON[][] { };

            // しゃがみ
            command[(int)Animation.Sit] = new BUTTON[][]{
                new BUTTON[]{BUTTON.DOWN}
            };

            // シールド
            command[(int)Animation.Shield] = new BUTTON[][]{
                new BUTTON[]{BUTTON.LTRIGGER},
                new BUTTON[]{BUTTON.RTRIGGER}
            };

            // その場緊急回避
            command[(int)Animation.AvoidG] = new BUTTON[][]{
                /*
                new BUTTON[]{BUTTON.LB, BUTTON.LTRIGGER, BUTTON.DOWN},
                new BUTTON[]{BUTTON.RB, BUTTON.LTRIGGER, BUTTON.DOWN},
                new BUTTON[]{BUTTON.LB, BUTTON.RTRIGGER, BUTTON.DOWN},
                new BUTTON[]{BUTTON.RB, BUTTON.RTRIGGER, BUTTON.DOWN}
                 * */
                new BUTTON[]{ BUTTON.LTRIGGER, BUTTON.DOWN},
                new BUTTON[]{ BUTTON.RTRIGGER, BUTTON.DOWN}
            };

            // 回転緊急回避
            command[(int)Animation.AvoidR] = new BUTTON[][]{
                /*
                new BUTTON[]{BUTTON.LB, BUTTON.LTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.RB, BUTTON.LTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.LB, BUTTON.RTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.RB, BUTTON.RTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.LB, BUTTON.LTRIGGER, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.RB, BUTTON.LTRIGGER, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.LB, BUTTON.RTRIGGER, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.RB, BUTTON.RTRIGGER, BUTTON.RIGHT}
                 * */
                new BUTTON[]{BUTTON.LTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.RTRIGGER, BUTTON.LEFT},
                new BUTTON[]{BUTTON.LTRIGGER, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.RTRIGGER, BUTTON.RIGHT}
            };

            // 空中緊急回避
            command[(int)Animation.AvoidS] = new BUTTON[][]{
                new BUTTON[]{BUTTON.LTRIGGER},
                new BUTTON[]{BUTTON.RTRIGGER}
            };

           
            // 吹っ飛ばされ
            //command[(int)Animation.Brown] = new BUTTON[][]{};

            // 受身
            command[(int)Animation.BreakFall] = new BUTTON[][]{            
                new BUTTON[]{BUTTON.LTRIGGER},
                new BUTTON[]{BUTTON.RTRIGGER}
            };

            // 横受身
            command[(int)Animation.BreakFallR] = new BUTTON[][]{            
                new BUTTON[]{BUTTON.LEFT},
                new BUTTON[]{BUTTON.RIGHT}
            };

            // 倒れている
            // command[(int)Animation.Down] = new BUTTON[][]{};

            // 跳ね返り
            // command[(int)Animation.Bump] = new BUTTON[][]{};

            // 攻撃された
            //command[(int)Animation.Attacked] = new BUTTON[][]{};

            // 死亡
            //command[(int)Animation.Dead] = new BUTTON[][]{};

            // 復活
            //command[(int)Animation.Reborn] = new BUTTON[][]{};

            // A
            command[(int)Animation.A] = new BUTTON[][]{
                new BUTTON[]{BUTTON.A}            
            };

            // 横A
            command[(int)Animation.SideA] = new BUTTON[][]{
                new BUTTON[]{BUTTON.A, BUTTON.LEFT},
                new BUTTON[]{BUTTON.A, BUTTON.RIGHT},
            };

            // 上A
            command[(int)Animation.UpA] = new BUTTON[][]{
                new BUTTON[]{BUTTON.A, BUTTON.UP}
            };

            // 下A
            command[(int)Animation.DownA] = new BUTTON[][]{
                new BUTTON[]{BUTTON.A, BUTTON.DOWN}
            };

            // 横スマッシュ
            command[(int)Animation.SideSmash] = new BUTTON[][]{
                new BUTTON[]{BUTTON.LB, BUTTON.A, BUTTON.LEFT},
                new BUTTON[]{BUTTON.LB, BUTTON.A, BUTTON.RIGHT},
                new BUTTON[]{BUTTON.RB, BUTTON.A, BUTTON.LEFT},
                new BUTTON[]{BUTTON.RB, BUTTON.A, BUTTON.RIGHT}
            };

            // 上スマッシュ
            command[(int)Animation.UpSmash] = new BUTTON[][]{
                new BUTTON[]{BUTTON.LB, BUTTON.A, BUTTON.UP},
                new BUTTON[]{BUTTON.RB, BUTTON.A, BUTTON.UP}
            };

            // 下スマッシュ
            command[(int)Animation.DownSmash] = new BUTTON[][]{
                new BUTTON[]{BUTTON.LB, BUTTON.A, BUTTON.DOWN},
                new BUTTON[]{BUTTON.RB, BUTTON.A, BUTTON.DOWN}
            };

            // B
            command[(int)Animation.B] = new BUTTON[][]{
                new BUTTON[]{BUTTON.B}            
            };

            // 上B
            command[(int)Animation.UpB] = new BUTTON[][]{
                new BUTTON[]{BUTTON.B, BUTTON.UP}
            }; ;

            // 下B
            command[(int)Animation.DownB] = new BUTTON[][]{
                new BUTTON[]{BUTTON.B, BUTTON.DOWN}
            };

            // 特殊B
            command[(int)Animation.SpecialB] = new BUTTON[][]{
                new BUTTON[]{BUTTON.B}            
            };
        }

        #endregion

        #region アニメーション関数の設定
        void InitializeUpdateFuns()
        {
            funcs = new UpdateFuncs[(int)Animation.Size];
            funcs[(int)Animation.Stop] = StopAnimation;   // 静止
            funcs[(int)Animation.Walk] = WalkAnimation;       // 歩き
            funcs[(int)Animation.Run] = RunAnimation;        // 走り
            funcs[(int)Animation.Jump] = JumpAnimation;       // ジャンプ
            funcs[(int)Animation.Float] = FloatAnimation;     // 空中
            // funcs[(int)Animation.SlideS] = SlideSAnimation;     // 空中で横に動く
            // funcs[(int)Animation.Fall] = FallAnimation;       // 高速落下（空中で↓ボタン）
            funcs[(int)Animation.Ground] = GroundAnimation;        // 着地
            funcs[(int)Animation.Climb] = ClimbAnimation;        // 駆け上がり
            funcs[(int)Animation.Sit] = SitAnimation;        // しゃがみ
            funcs[(int)Animation.Shield] = ShieldAnimation;     // シールド
            funcs[(int)Animation.AvoidG] = AvoidGAnimation;     // その場緊急回避
            funcs[(int)Animation.AvoidR] = AvoidRAnimation;     // 転がり緊急回避
            funcs[(int)Animation.AvoidS] = AvoidSAnimation;     // 空中緊急回避
            funcs[(int)Animation.Brown] = BrownAnimation;      // 吹っ飛ばされている
            funcs[(int)Animation.BreakFall] = BreakFallAnimation;  // 受け身
            funcs[(int)Animation.BreakFallR] = BreakFallRAnimation;  // 横受け身
            funcs[(int)Animation.Down] = DownAnimation;  // 横受け身
            funcs[(int)Animation.Bump] = BumpAnimation;  // 横受け身
            funcs[(int)Animation.Attacked] = AttackedAnimation;   // 攻撃された瞬間
            funcs[(int)Animation.Dead] = DeadAnimation;          // A
            funcs[(int)Animation.Reborn] = RebornAnimation;          // A
            funcs[(int)Animation.A] = AAnimation;          // A
            funcs[(int)Animation.SideA] = SideAAnimation;      // 横A
            funcs[(int)Animation.UpA] = UpAAnimation;        // 上A
            funcs[(int)Animation.DownA] = DownAAnimation;      // 下A
            funcs[(int)Animation.SideSmash] = SideSmashAnimation;      // 横スマッシュ
            funcs[(int)Animation.UpSmash] = UpSmashAnimation;        // 上スマッシュ
            funcs[(int)Animation.DownSmash] = DownSmashAnimation;      // 下スマッシュ
            if (player.character == CHARACTER.CUBE)
            {
                funcs[(int)Animation.B] = CubeBAnimation;          // B
                funcs[(int)Animation.UpB] = CubeUpBAnimation;        // 上B
                funcs[(int)Animation.DownB] = CubeDownBAnimation;      // 下B
                funcs[(int)Animation.SpecialB] = CubeSpecialBAnimation;   // 特殊B（ゲージが溜まったときの
            }
            else
            {
                funcs[(int)Animation.B] = SphereBAnimation;          // B
                funcs[(int)Animation.UpB] = SphereUpBAnimation;        // 上B
                funcs[(int)Animation.DownB] = SphereDownBAnimation;      // 下B
                funcs[(int)Animation.SpecialB] = SphereSpecialBAnimation;   // 特殊B（ゲージが溜まったときの
            }
        }
        #endregion

        #region コンストラクタ・Updateなどの基本関数
        PLAYER_INFO player;
        UpdateFuncs[] funcs;

        public Character(Fight scene, Hashtable parameters, PLAYER_INFO p)
            : base( scene, parameters )
        {
            player = p;
            Initialize();
        }

        public void Initialize()
        {
            InitializePrameters();
            InitializeCommand();
            InitializeUpdateFuns();
            InitializeCPU();
        }

        public ModelInfo mi;

        public override bool Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime;

            mi = ((ModelInfo)model["model"]);

            cpuInput(gameTime);

            // 各コマンドが入力されたかを判定
            for (int com = 0; com < (int)Animation.Size; com++)
            {
                commandState[com] = false;
                if (command[com] == null) continue;
                for (int val = 0; val < command[com].Length; val++)
                {
                    if (command[com][val] == null)
                    {
                        continue;
                    }
                    bool flg = true;
                    for (int i = 0; i < command[com][val].Length; i++)
                    {
                        if (input.isOn(command[com][val][i], player.dev) == false)
                        {
                            flg = false;
                            continue;
                        }
                    }
                    // コマンドが入力された
                    if (flg)
                    {
                        commandState[com] = true;
                        break;
                    }
                }
            }
            for (int com = 0; com < (int)Animation.Size; com++)
            {
                if (commandFlg[com] == false)
                {
                    if (commandState[com] == false)
                    {
                        commandFlg[com] = true;
                    }
                }
            }

#if _DEBUG
            int debugi = 0;
            for (debugi = 0; debugi < (int)Animation.Size; debugi++)
            {
                debugId[debugi] = GLOBAL.debug.AddMessage(player.id + "P : " + commandName[debugi], "" + commandState[debugi], debugId[debugi]);
            }
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : Gage", "" + gage, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : VelocityX", "" + velocity.X, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : VelocityY", "" + velocity.Y, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : DirectionX", "" + direction.X, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : DirectionY", "" + direction.Y, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : DirectionZ", "" + direction.Z, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : Animation", "" + animationName[(int)anim], debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : Damage", "" + damage, debugId[debugi++]);
            debugId[debugi] = GLOBAL.debug.AddMessage((player.id + 1) + "P : Life", "" + life, debugId[debugi++]);
            GLOBAL.debug.NewLine();
#endif

            // ゲージを貯める
            if (gage < MAX_GAGE)
            {
                gage += gageV * gameTime.ElapsedGameTime.TotalSeconds;
            }

            checkedColState = false;

            adjustTo = -1;

            // 矩形の正規化
            if (normalizeRectangle)
            {
                NormalizeRectangle(gameTime);
            }

            // 各状態での処理
            Animation prevAnim = anim;

            funcs[(int)anim](gameTime);

            Object o = model["DirectionIndicator"];
            if (o != null)
            {
                ModelInfo md = (ModelInfo)o;
                md.recVisible.Locate(RecVisible.Center.X - md.recVisible.Width / 2, RecVisible.Top3D + dist);
                if (direction.X > 0)
                {
                    md.rotation = new Vector3(0, 0, 0);
                }
                else
                {
                    md.rotation = new Vector3(0, 180, 0);
                }
            }

            if (CheckFloat(gameTime) == false)
            {
                jumpFlg = 0;
                avoidSFlg = true;
                upBFlg = true;
            }

            if (anim != prevAnim)
            {
                time = TimeSpan.Zero;
                endAnimation = false;
            }

            for (int i = 0; i < cubeBBomb.Length; i++)
            {
                if (cubeBBomb[i] != null && cubeBBomb[i].IsAlive == false)
                {
                    cubeBBomb[i] = null;
                }
            }
            for (int i = 0; i < sphereBBomb.Length; i++)
            {
                if (sphereBBomb[i] != null && sphereBBomb[i].IsAlive == false)
                {
                    sphereBBomb[i] = null;
                }
            }

#if _DEBUG
            if (double.IsNaN(RecVisible.X) || double.IsNaN(RecVisible.Y))
            {
                while (true)
                {

                }
            }
#endif

            return true;
        }

        bool normalizeRectangle = true;
        double normalizeSpeed = 1.0;

        void NormalizeRectangle(GameTime gameTime)
        {
            double dx = normalizeSpeed * gameTime.ElapsedGameTime.TotalSeconds;

            RectangleD r = mi.recVisible, c = normalRectangle;
            if (r.Top < c.Top) r.Top = Math.Min(r.Top + dx, c.Top);
            else if (r.Top > c.Top) r.Top = Math.Max(r.Top - dx, c.Top);

            if (r.Bottom < c.Bottom) r.Bottom = Math.Min(r.Bottom + dx, c.Bottom);
            else if (r.Bottom > c.Bottom) r.Bottom = Math.Max(r.Bottom - dx, c.Bottom);

            if (r.Left < c.Left) r.Left = Math.Min(r.Left + dx, c.Left);
            else if (r.Left > c.Left) r.Left = Math.Max(r.Left - dx, c.Left);

            if (r.Right < c.Right) r.Right = Math.Min(r.Right + dx, c.Right);
            else if (r.Right > c.Right) r.Right = Math.Max(r.Right - dx, c.Right);
        }

        public override void Draw()
        {
            DrawModel("DirectionIndicator");
            base.Draw();
        }


        #endregion
        
    }
}