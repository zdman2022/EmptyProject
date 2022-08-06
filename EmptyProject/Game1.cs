using System;
using System.Collections.Generic;
using System.Reflection;

using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Screens;
using Microsoft.Xna.Framework;

using System.Linq;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FlatRedBall.IO;
using GlueControl.Managers;

namespace EmptyProject
{
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        partial void GeneratedInitialize();
        partial void GeneratedUpdate(Microsoft.Xna.Framework.GameTime gameTime);
        partial void GeneratedDraw(Microsoft.Xna.Framework.GameTime gameTime);

        public Game1() : base()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE || ANDROID || IOS

            // Frame rate is 30 fps by default for Windows Phone,
            // so let's keep that for other phones too
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#elif WINDOWS || DESKTOP_GL
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
#endif


#if WINDOWS_8
            FlatRedBall.Instructions.Reflection.PropertyValuePair.TopLevelAssembly = 
                this.GetType().GetTypeInfo().Assembly;
#endif

        }

        protected override void Initialize()
        {
            #if IOS
            var bounds = UIKit.UIScreen.MainScreen.Bounds;
            var nativeScale = UIKit.UIScreen.MainScreen.Scale;
            var screenWidth = (int)(bounds.Width * nativeScale);
            var screenHeight = (int)(bounds.Height * nativeScale);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            #endif
        
            FlatRedBallServices.InitializeFlatRedBall(this, graphics);

            GlobalContent.Initialize();
            GeneratedInitialize();

            base.Initialize();


            //Test Code
            GlueDynamicManager.GlueDynamicManager.Self.SetInitialState(GlueDynamicManager.GlueJsonProcessor.GetTest("Test1", "Base"));
            FilePath gluj = FileManager.RelativeDirectory + "../../../../../ProjectWithCodegen/ProjectWithCodegen/ProjectWithCodegen.gluj";

            GlueDynamicManager.GlueDynamicManager.Self.UpdateState(GlueDynamicManager.GlueJsonProcessor.GetTest(gluj));
            // temporary hack:
            // GlueCommands (and
            // ObjectFinder) need
            // to have access to the
            // entire Glue project. However
            // calling LoadProject re-creates
            // the entire project from the json 
            // files, so the references stored by
            // ObjectFinder do not match the references
            // stored by the dynamic objects. We need to
            // make ObjectFinder the authority.
            GlueCommands.Self.LoadProject(gluj.FullPath);
            GlueDynamicManager.DynamicInstances.DynamicScreen.CurrentScreen = "ScreenWithShapes";
            ScreenManager.Start(typeof(GlueDynamicManager.DynamicInstances.DynamicScreen));
        }

        protected override void Update(GameTime gameTime)
        {
            FlatRedBallServices.Update(gameTime);

            FlatRedBall.Screens.ScreenManager.Activity();

            GeneratedUpdate(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FlatRedBallServices.Draw();

            GeneratedDraw(gameTime);

            base.Draw(gameTime);
        }
    }
}
