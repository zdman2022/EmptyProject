using FlatRedBall;
using FlatRedBall.Content.AnimationChain;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Graphics.Animation;
using GlueControl.Managers;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.Converters
{
    internal class ValueConverter
    {
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

                    if(instruction.Type == typeof(FlatRedBall.Graphics.Animation.AnimationChainList).FullName)
                    {
                        variableValue = FlatRedBallServices.Load<AnimationChainList>(absoluteRfs.FullPath);
                    }
                    else if(instruction.Type == typeof(Microsoft.Xna.Framework.Graphics.Texture2D).FullName)
                    {
                        variableValue = FlatRedBallServices.Load<Microsoft.Xna.Framework.Graphics.Texture2D>(absoluteRfs.FullPath);
                    }
                    //var achx = FlatRedBallServices.Load<AnimationChainListSave>(rfs.Name);


                    // continue here
                    //variableValue = Editing.VariableAssignmentLogic.ConvertStringToType(instruction.Type, asString, false, out conversionReport);
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
            }

            return variableValue;
        }
    }
}
