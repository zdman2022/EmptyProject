using FlatRedBall;
using FlatRedBall.Content.AnimationChain;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Math.Geometry;
using GlueControl.Managers;
using GlueControl.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlueDynamicManager.Converters
{
    internal class ValueConverter
    {
        private static readonly Regex RegExList = new Regex("^List<(.*)>$");

        public static object ConvertValue(InstructionSave instruction, GlueControl.Models.GlueElement instanceContainer)
        {
            var variableValue = instruction.Value;

            if (instruction.Type == "int")
            {
                if (variableValue is long asLong)
                {
                    variableValue = (int)asLong;
                }
            }
            else if (instruction.Type == "int?")
            {
                if (variableValue is long asLong)
                {
                    variableValue = (int?)asLong;
                }
            }
            else if (instruction.Type == "float" || instruction.Type == "Single")
            {
                if (variableValue is int asInt)
                {
                    variableValue = (float)asInt;
                }
                else if (variableValue is double asDouble)
                {
                    variableValue = (float)asDouble;
                }
            }
            else if (instruction.Type == "float?")
            {
                if (variableValue is int asInt)
                {
                    variableValue = (float?)asInt;
                }
                else if (variableValue is double asDouble)
                {
                    variableValue = (float?)asDouble;
                }
            }
            else if (instruction.Type == typeof(FlatRedBall.Graphics.Animation.AnimationChainList).FullName ||
                instruction.Type == typeof(Microsoft.Xna.Framework.Graphics.Texture2D).FullName)
            {
                if (variableValue is string asString && !string.IsNullOrWhiteSpace(asString))
                {
                    var rfs = instanceContainer.GetReferencedFileSaveRecursively(asString);
                    var absoluteRfs = GlueCommands.Self.GetAbsoluteFilePath(rfs);


                    // todo - need to support global content loading
                    variableValue = FileLoader.LoadFile(absoluteRfs, FlatRedBallServices.GlobalContentManager, instruction.Type);
                }
            }
            //else if (instruction.Type == typeof(Microsoft.Xna.Framework.Color).FullName)
            //{
            //    if (variableValue is string asString && !string.IsNullOrWhiteSpace(asString))
            //    {
            //        variableValue = Editing.VariableAssignmentLogic.ConvertStringToType(instruction.Type, asString, false, out conversionReport);
            //    }
            //}
            else if (instruction.Type == typeof(Microsoft.Xna.Framework.Graphics.TextureAddressMode).Name)
            {
                if (variableValue is int asInt)
                {
                    variableValue = (Microsoft.Xna.Framework.Graphics.TextureAddressMode)asInt;
                }
                if (variableValue is long asLong)
                {
                    variableValue = (Microsoft.Xna.Framework.Graphics.TextureAddressMode)asLong;
                }
            }
            else if (
                instruction.Type == typeof(FlatRedBall.Graphics.ColorOperation).Name ||
                instruction.Type == typeof(FlatRedBall.Graphics.ColorOperation).FullName
                )
            {
                if (variableValue is int asInt)
                {
                    variableValue = (FlatRedBall.Graphics.ColorOperation)asInt;
                }
                if (variableValue is long asLong)
                {
                    variableValue = (FlatRedBall.Graphics.ColorOperation)asLong;
                }
            }else if (
                instruction.Type == typeof(Microsoft.Xna.Framework.Color).Name ||
                instruction.Type == typeof(Microsoft.Xna.Framework.Color).FullName
            )
            {
                variableValue = typeof(Microsoft.Xna.Framework.Color).GetProperty(variableValue.ToString()).GetValue(null);
            }
            else if(RegExList.IsMatch(instruction.Type))
            {
                var match = RegExList.Match(instruction.Type);
                var subType = match.Groups[1].Value;

                if(subType == "Vector2")
                {
                    variableValue = JsonConvert.DeserializeObject<List<Vector2>>(variableValue.ToString());
                }
                else
                {
                    throw new NotImplementedException();
                }
                
            }

            return variableValue;
        }

        internal static object ConvertForProperty(object value, string type, string objectType)
        {
            if(type == "List<Vector2>" && objectType == "FlatRedBall.Math.Geometry.Polygon")
            {
                var points = new List<FlatRedBall.Math.Geometry.Point>();

                foreach(var vector in (List<Vector2>)value)
                {
                    points.Add(new FlatRedBall.Math.Geometry.Point(vector.X, vector.Y));
                }

                return points;
            }

            return value;
        }
    }
}
