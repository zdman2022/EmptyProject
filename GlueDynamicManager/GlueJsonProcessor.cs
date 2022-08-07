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

namespace GlueDynamicManager
{
    internal class GlueJsonProcessor
    {
        private static readonly Regex EntityArrayMatch = new Regex("^/EntityReferences/\\d+$");
        private static readonly Regex ScreenArrayMatch = new Regex("^/ScreenReferences/\\d+$");

        public static List<IOperation> GetOperations(GlueJsonContainer before, GlueJsonContainer after)
        {
            var jdp = new JsonDiffPatch();
            var jdf = new JsonDeltaFormatter();

            var glueDifferences = jdp.Diff(before.Glue, after.Glue);

            var operations = jdf.Format(glueDifferences);

            var returnValue = new List<IOperation>();
            foreach (var operation in operations)
            {
                if (EntityArrayMatch.IsMatch(operation.Path) && operation.Op == "add")
                {
                    returnValue.Add(new CreateNewEntityOperation((JToken)operation.Value));
                }else if(ScreenArrayMatch.IsMatch(operation.Path) && operation.Op == "add")
                {
                    returnValue.Add(new CreateNewScreenOperation((JToken)operation.Value));
                }
            }

            ReorderOperations(returnValue);

            return returnValue;
        }

        private static void ReorderOperations(List<IOperation> returnValue)
        {
            //ToDo
        }

        internal static GlueJsonContainer GetTest(string testName, string instance)
        {

            var prefixPath = "..\\..\\..\\..\\";

            FilePath glujFilePath = $"{prefixPath}GlueDynamicManager\\Test\\{testName}\\{instance}\\Glue.gluj";

            return GetTest(glujFilePath);
        }

        internal static GlueJsonContainer GetTest(FilePath glujFilePath)
        {
            var returnValue = new GlueJsonContainer();
            returnValue.Glue = JToken.Parse(File.ReadAllText(glujFilePath.FullPath));

            var entityDirectory = glujFilePath.GetDirectoryContainingThis() + "\\Entities";
            if (Directory.Exists(entityDirectory))
            {
                foreach (var file in Directory.GetFiles(entityDirectory).Where(item => item.EndsWith(".glej")))
                {
                    var entityName = Path.GetFileNameWithoutExtension(file);

                    returnValue.Entities.Add(entityName, JToken.Parse(File.ReadAllText(file)));
                }
            }

            var screenDirectory = glujFilePath.GetDirectoryContainingThis() + "\\Screens";
            if (Directory.Exists(screenDirectory))
            {
                foreach (var file in Directory.GetFiles(screenDirectory).Where(item => item.EndsWith(".glsj")))
                {
                    var screenName = Path.GetFileNameWithoutExtension(file);

                    returnValue.Screens.Add(screenName, JToken.Parse(File.ReadAllText(file)));
                }
            }

            return returnValue;
        }
    }
}
