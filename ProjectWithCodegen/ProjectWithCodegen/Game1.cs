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
using GlueControl;
using ProjectWithCodegen.Screens;
using FlatRedBall.Input;

namespace ProjectWithCodegen
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
        string startingDirectory;

        protected override void Initialize()
        {
            if (false)
            {
                //Test Code
                GlueDynamicManager.GlueDynamicManager.Self.SetInitialState(GlueDynamicManager.GlueDynamicTest.GetTest(FileManager.RelativeDirectory + "../../../../ProjectWithCodegen.gluj"));
                FilePath gluj = FileManager.RelativeDirectory + "../../../../../../../ProjectWithCodegen/ProjectWithCodegen/ProjectWithCodegen.gluj";

                //GlueDynamicManager.GlueDynamicManager.Self.UpdateStateAsync(GlueDynamicManager.GlueDynamicTest.GetTest(gluj)).Wait();

                //GlueDynamicManager.DynamicInstances.DynamicScreen.CurrentScreen = "SpriteScreen";
                //ScreenManager.MoveToScreen(typeof(GlueDynamicManager.DynamicInstances.DynamicScreen));
            }

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

            //if (true)
            //{
            startingDirectory = FileManager.RelativeDirectory;

            while (System.IO.Directory.Exists(startingDirectory) && !System.IO.File.Exists(startingDirectory + "ProjectWithCodegen.gluj"))
            {
                startingDirectory += "../";
            }

            GlueCommands.Self.LoadProject(startingDirectory + "ProjectWithCodegen.gluj");

            var initialState = GlueDynamicManager.GlueDynamicTest.GetTest(startingDirectory + "ProjectWithCodegen.gluj");


            GlueDynamicManager.GlueDynamicManager.Self.SetInitialState(initialState);

            gameConnectionManager.OnPacketReceived += async (packet) =>
            {

                if (packet.Packet.PacketType == "JsonUpdate")
                {
                    var jPacket = Newtonsoft.Json.Linq.JToken.Parse(packet.Packet.Payload);

                    var entities = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Newtonsoft.Json.Linq.JToken>>(jPacket["Entities"].ToString());
                    var screens = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Newtonsoft.Json.Linq.JToken>>(jPacket["Screens"].ToString());

                    var state = new GlueDynamicManager.GlueJsonContainer()
                    {
                        Glue = new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.GlueProjectSave>(jPacket["Glue"].ToString()),
                        Entities = entities.ToDictionary(item => CommandReceiver.GlueToGameElementName(item.Key), item => new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.EntitySave>(item.Value.ToString())),
                        Screens = screens.ToDictionary(item => CommandReceiver.GlueToGameElementName(item.Key), item => new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.ScreenSave>(item.Value.ToString()))
                    };

                    GlueDynamicManager.GlueDynamicManager.Self.UpdateState(state);
                }
            };
            //}

            //GlueDynamicManager.DynamicInstances.DynamicScreen.CurrentScreenGlue = "Screens\\Level2";

            //var entities = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Newtonsoft.Json.Linq.JToken>> (jPacket["Entities"].ToString());
            //var screens = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Newtonsoft.Json.Linq.JToken>>(jPacket["Screens"].ToString());

            //var state = new GlueDynamicManager.GlueJsonContainer()
            //{
            //    Glue = new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.GlueProjectSave>(jPacket["Glue"].ToString()),
            //    Entities = entities.ToDictionary(item => CommandReceiver.GlueToGameElementName(item.Key), item => new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.EntitySave>(item.Value.ToString())),
            //    Screens = screens.ToDictionary(item => CommandReceiver.GlueToGameElementName(item.Key), item => new GlueDynamicManager.GlueJsonContainer.JsonContainer<GlueControl.Models.ScreenSave>(item.Value.ToString()))
            //};

            //ScreenManager.Start(typeof(GlueDynamicManager.DynamicInstances.DynamicScreen));





            base.Initialize();
        }

        private static void GoToLevel2Dynamic(string startingDirectory)
        {
            GlueDynamicManager.DynamicInstances.HybridScreen.CurrentScreenGlue = "Screens\\Level2Derived";


            var newState = GlueDynamicManager.GlueDynamicTest.GetTest(startingDirectory + "ProjectWithCodegen.gluj", includeExcludedFromGeneration: true);
            GlueDynamicManager.GlueDynamicManager.Self.UpdateState(newState);


            ScreenManager.CurrentScreen.MoveToScreen(typeof(Level2));
        }

        protected override void Update(GameTime gameTime)
        {
            FlatRedBallServices.Update(gameTime);

            FlatRedBall.Screens.ScreenManager.Activity();
            if(InputManager.Keyboard.KeyPushed(Keys.Space))
            {
                GoToLevel2Dynamic(startingDirectory);
            }

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
