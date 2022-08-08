using FlatRedBall.Screens;
using GlueControl.Models;
using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.GlueHelpers;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueDynamicManager.Converters;
using FlatRedBall.Graphics;
using FlatRedBall.Content.Instructions;

namespace GlueDynamicManager.Processors
{
    internal class ScreenOperationProcessor
    {
        private static Regex PathMatchRegEx_NamedObject = new Regex("^/NamedObjects/\\d+$");
        private static Regex PathMatchRegEx_NamedObjectInstructionSave = new Regex("^/NamedObjects/(\\d+)/InstructionSaves/(\\d+)/Value$");

        internal static void ApplyOperations(HybridScreen screen, ScreenSave oldScreenSave, ScreenSave newScreenSave, JToken glueDifferences, IList<Operation> operations)
        {
            foreach(var operation in operations)
            {
                if(operation.Op == "add")
                {
                    if(PathMatchRegEx_NamedObject.IsMatch(operation.Path))
                    {
                        var nos = JsonConvert.DeserializeObject<NamedObjectSave>(operation.Value.ToString());

                        var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, newScreenSave);
                        NamedObjectSaveHelper.InitializeNamedObject(nos, itemContainer, newScreenSave, screen.PropertyFinder, out var positionedObjectLists, out var instancedObjects, out var instancedEntities);

                        DoInitialize(screen, newScreenSave, positionedObjectLists, instancedObjects, instancedEntities);
                        AddToManagers(screen, newScreenSave, positionedObjectLists, instancedObjects, instancedEntities);

                        screen.PositionedObjectLists.AddRange(positionedObjectLists);
                        screen.InstancedObjects.AddRange(instancedObjects);
                        screen.InstancedEntities.AddRange(instancedEntities);
                    }
                }else if(operation.Op == "replace")
                {
                    if(PathMatchRegEx_NamedObjectInstructionSave.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var oldInstruction = oldScreenSave.NamedObjects[noIndex].InstructionSaves[isIndex];
                        var newInstruction = newScreenSave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = screen.PropertyFinder(newScreenSave.NamedObjects[noIndex].InstanceName);

                        CleanupOldInstruction(oldInstruction);
                        ApplyInstruction(newInstruction, newScreenSave, screen, obj.GetType().Name, obj);
                    }
                }
            }
        }

        private static void ApplyInstruction(InstructionSave instruction, ScreenSave screenSave, HybridScreen screen, string objectType, object value)
        {
            var convertedValue = ValueConverter.ConvertValue(instruction, screenSave);
            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, objectType);
            screen.Screen.ApplyVariable(instruction.Member, convertedValue, value);
        }

        private static void CleanupOldInstruction(InstructionSave oldInstruction)
        {
            //Here, we can clean up any registration or things that a new instruction won't override
        }

        private static void DoInitialize(HybridScreen screen, ScreenSave screenSave, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
        {
            bool oldShapeManagerSuppressAdd = FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue;
            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = true;

            for (int i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, screenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);
                        screen.Screen.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                    }
            }

            for (int i = 0; i < instancedEntities.Count; i++)
            {
                var instance = instancedEntities[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, screenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                        instance.Value.SetVariable(instruction.Member, convertedValue);
                    }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private static void AddToManagers(HybridScreen screen, ScreenSave screenSave, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
        {
            for (int i = 0; i < instancedEntities.Count; i++)
            {
                var mLayerProp = screen.Screen.GetType().BaseType.GetProperty("mLayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                instancedEntities[i].Value.AddToManagers(mLayerProp == null ? null : (Layer)mLayerProp.GetValue(screen));
            }

            for (var i = 0; i < instancedObjects.Count; i++)
            {
                if (!GlueDynamicManager.Self.IsAttachedEntity(instancedObjects[i].Value))
                {
                    // todo: need to support layers
                    FlatRedBall.Graphics.Layer layer = null;
                    InstanceAddToManager.AddToManager(instancedObjects[i].Value, layer);
                }
            }
        }
    }
}
