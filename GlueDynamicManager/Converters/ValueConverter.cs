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
            return ConvertValue(instruction.Type, instruction.Value, instanceContainer);
        }

        internal static object ConvertValue(CustomVariable customVariable, GlueElement instanceContainer)
        {
            return ConvertValue(customVariable.Type, customVariable.DefaultValue, instanceContainer);
        }

        private static object ConvertValue(string type, object variableValue, GlueElement instanceContainer)
        {
            if (type == "int")
            {
                if (variableValue is long asLong)
                {
                    variableValue = (int)asLong;
                }
            }
            else if (type == "int?")
            {
                if (variableValue is long asLong)
                {
                    variableValue = (int?)asLong;
                }
            }
            else if (type == "float" || type == "Single")
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
            else if (type == "float?")
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
            else if (type == typeof(FlatRedBall.Graphics.Animation.AnimationChainList).FullName ||
                type == typeof(Microsoft.Xna.Framework.Graphics.Texture2D).FullName)
            {
                if (variableValue is string asString && !string.IsNullOrWhiteSpace(asString))
                {
                    var rfs = instanceContainer.GetReferencedFileSaveRecursively(asString);
                    var absoluteRfs = GlueCommands.Self.GetAbsoluteFilePath(rfs);


                    // todo - need to support global content loading
                    variableValue = FileLoader.LoadFile(absoluteRfs, FlatRedBallServices.GlobalContentManager, type);
                }
            }
            //else if (type == typeof(Microsoft.Xna.Framework.Color).FullName)
            //{
            //    if (variableValue is string asString && !string.IsNullOrWhiteSpace(asString))
            //    {
            //        variableValue = Editing.VariableAssignmentLogic.ConvertStringToType(type, asString, false, out conversionReport);
            //    }
            //}
            else if (type == typeof(Microsoft.Xna.Framework.Graphics.TextureAddressMode).Name)
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
                type == typeof(FlatRedBall.Graphics.ColorOperation).Name ||
                type == typeof(FlatRedBall.Graphics.ColorOperation).FullName
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
            }
            else if (
                type == typeof(Microsoft.Xna.Framework.Color).Name ||
                type == typeof(Microsoft.Xna.Framework.Color).FullName
            )
            {
                variableValue = typeof(Microsoft.Xna.Framework.Color).GetProperty(variableValue.ToString()).GetValue(null);
            }
            else if (RegExList.IsMatch(type))
            {
                var match = RegExList.Match(type);
                var subType = match.Groups[1].Value;

                if (subType == "Vector2")
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

        internal static string ConvertForPropertyName(string name, object glueElement)
        {
            if(name == "X")
            {
                if(TypeHandler.GetPropValueIfExists(glueElement, "Parent", out var value) && value != null)
                    return "RelativeX";
            }

            if (name == "Y")
            {
                if (TypeHandler.GetPropValueIfExists(glueElement, "Parent", out var value) && value != null)
                    return "RelativeY";
            }

            return name;
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
