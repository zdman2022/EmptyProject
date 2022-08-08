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

        internal static GlueJsonContainer GetTest(FilePath glujFilePath)
        {
            var returnValue = new GlueJsonContainer();
            returnValue.Glue = new GlueJsonContainer.JsonContainer<GlueControl.Models.GlueProjectSave>(File.ReadAllText(glujFilePath.FullPath));

            var entityDirectory = glujFilePath.GetDirectoryContainingThis() + "\\Entities";
            if (Directory.Exists(entityDirectory))
            {
                foreach (var file in Directory.GetFiles(entityDirectory).Where(item => item.EndsWith(".glej")))
                {
                    var entityName = Path.GetFileNameWithoutExtension(file);

                    returnValue.Entities.Add(entityName, new GlueJsonContainer.JsonContainer<EntitySave>(File.ReadAllText(file)));
                }
            }

            var screenDirectory = glujFilePath.GetDirectoryContainingThis() + "\\Screens";
            if (Directory.Exists(screenDirectory))
            {
                foreach (var file in Directory.GetFiles(screenDirectory).Where(item => item.EndsWith(".glsj")))
                {
                    var screenName = Path.GetFileNameWithoutExtension(file);

                    returnValue.Screens.Add(screenName, new GlueJsonContainer.JsonContainer<ScreenSave>(File.ReadAllText(file)));
                }
            }

            return returnValue;
        }
    }

}