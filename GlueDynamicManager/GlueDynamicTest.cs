using GlueDynamicManager.Operations;
using FlatRedBall.IO;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlueControl.Models;
using GlueControl;

namespace GlueDynamicManager
{

    internal class GlueDynamicTest
    {
        internal static GlueJsonContainer GetTest(string testName, string instance)
        {

            var prefixPath = "..\\..\\..\\..\\";

            FilePath glujFilePath = $"{prefixPath}GlueDynamicManager\\Test\\{testName}\\{instance}\\Glue.gluj";

            return GetTest(glujFilePath);
        }

        internal static GlueJsonContainer GetTest(FilePath glujFilePath, bool includeExcludedFromGeneration = false)
        {
            var returnValue = new GlueJsonContainer();
            returnValue.Glue = new GlueJsonContainer.JsonContainer<GlueControl.Models.GlueProjectSave>(File.ReadAllText(glujFilePath.FullPath));

            var glujDirectory = glujFilePath.GetDirectoryContainingThis();
            var entityDirectory = glujDirectory + "\\Entities";

            if (Directory.Exists(entityDirectory))
            {
                foreach (FilePath file in Directory.GetFiles(entityDirectory).Where(item => item.EndsWith(".glej")))
                {
                    var relative = file.RemoveExtension().RelativeTo(glujDirectory);
                    //var entityName = Path.GetFileNameWithoutExtension(file);
                    var gameName = CommandReceiver.GlueToGameElementName(relative);

                    var entityJsonContainer = new GlueJsonContainer.JsonContainer<EntitySave>(File.ReadAllText(file.FullPath));

                    if(entityJsonContainer.Value.ExcludeFromGeneration == false || includeExcludedFromGeneration)
                    {
                        returnValue.Entities.Add(gameName, entityJsonContainer);
                    }
                }
            }

            var screenDirectory = glujFilePath.GetDirectoryContainingThis() + "\\Screens";
            if (Directory.Exists(screenDirectory))
            {
                foreach (FilePath file in Directory.GetFiles(screenDirectory).Where(item => item.EndsWith(".glsj")))
                {
                    var relative = file.RemoveExtension().RelativeTo(glujDirectory);

                    var gameName = CommandReceiver.GlueToGameElementName(relative);

                    var fileContents = File.ReadAllText(file.FullPath);

                    var screenJsonContainer = new GlueJsonContainer.JsonContainer<ScreenSave>(fileContents);

                    if(screenJsonContainer.Value.ExcludeFromGeneration == false || includeExcludedFromGeneration)
                    {
                        returnValue.Screens.Add(gameName, screenJsonContainer);
                    }

                }
            }

            return returnValue;
        }
    }

}