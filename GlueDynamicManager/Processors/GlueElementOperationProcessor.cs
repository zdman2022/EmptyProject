using FlatRedBall.Content.Instructions;
using FlatRedBall.Screens;
using GlueControl.Models;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueDynamicManager.GlueHelpers;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GlueDynamicManager.Processors
{
    internal class GlueElementOperationProcessor
    {
        private static Regex PathMatchRegEx_NamedObject = new Regex("^/NamedObjects$");
        private static Regex PathMatchRegEx_NamedObject_Item = new Regex("^/NamedObjects/\\d+?$");
        private static Regex PathMatchRegEx_NamedObjectInstructionSave = new Regex("^/NamedObjects/(\\d+)/InstructionSaves/(\\d+)(/Value)?$");

        internal static void ApplyOperations(HybridGlueElement element, GlueElement oldSave, GlueElement newSave, JToken glueDifferences, IList<Operation> operations, bool addToManagers)
        {
            foreach (var operation in operations)
            {
                if (operation.Op == "add")
                {
                    if (PathMatchRegEx_NamedObject.IsMatch(operation.Path))
                    {
                        var nosList = JsonConvert.DeserializeObject<List<NamedObjectSave>>(operation.Value.ToString());

                        foreach(var nos in nosList)
                        {
                            AddNamedObject(element, newSave, nos, addToManagers);
                        }
                    }
                    else if (PathMatchRegEx_NamedObject_Item.IsMatch(operation.Path))
                    {
                        var nos = JsonConvert.DeserializeObject<NamedObjectSave>(operation.Value.ToString());
                        AddNamedObject(element, newSave, nos, addToManagers);
                    }
                    else if (PathMatchRegEx_NamedObjectInstructionSave.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var newInstruction = newSave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = element.PropertyFinder(newSave.NamedObjects[noIndex].InstanceName);

                        ApplyInstruction(newInstruction, newSave, element, obj.GetType().Name, obj);
                    }
                }
                else if (operation.Op == "replace")
                {
                    if (PathMatchRegEx_NamedObjectInstructionSave.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var oldInstruction = oldSave.NamedObjects[noIndex].InstructionSaves[isIndex];
                        var newInstruction = newSave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = element.PropertyFinder(newSave.NamedObjects[noIndex].InstanceName);

                        CleanupOldInstruction(oldInstruction);
                        ApplyInstruction(newInstruction, newSave, element, obj.GetType().Name, obj);
                    }
                }
            }
        }

        private static void AddNamedObject(HybridGlueElement element, GlueElement newSave, NamedObjectSave nos, bool addToManagers)
        {
            var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, newSave);
            NamedObjectSaveHelper.InitializeNamedObject(nos, itemContainer, newSave, element.PropertyFinder, out var positionedObjectLists, out var instancedObjects, out var instancedEntities);

            DoInitialize(element, newSave, positionedObjectLists, instancedObjects, instancedEntities);
            if(addToManagers)
                AddToManagers(element, newSave, positionedObjectLists, instancedObjects, instancedEntities);

            element.PositionedObjectLists.AddRange(positionedObjectLists);
            element.InstancedObjects.AddRange(instancedObjects);
            element.InstancedEntities.AddRange(instancedEntities);
        }

        private static void DoInitialize(HybridGlueElement element, GlueElement save, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
        {
            bool oldShapeManagerSuppressAdd = FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue;
            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = true;

            for (int i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, save);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);
                        ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                    }
            }

            for (int i = 0; i < instancedEntities.Count; i++)
            {
                var instance = instancedEntities[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, save);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                        instance.Value.SetVariable(instruction.Member, convertedValue);
                    }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private static void AddToManagers(HybridGlueElement element, GlueElement save, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
        {
            for (int i = 0; i < instancedEntities.Count; i++)
            {
                instancedEntities[i].Value.AddToManagers(null);
            }

            for (var i = 0; i < instancedObjects.Count; i++)
            {
                // todo: need to support layers
                FlatRedBall.Graphics.Layer layer = null;
                InstanceAddToManager.AddToManager(instancedObjects[i], instancedObjects, layer);
            }
        }

        private static void CleanupOldInstruction(InstructionSave oldInstruction)
        {
            //Here, we can clean up any registration or things that a new instruction won't override
        }

        private static void ApplyInstruction(InstructionSave instruction, GlueElement save, HybridGlueElement element, string objectType, object value)
        {
            var convertedValue = ValueConverter.ConvertValue(instruction, save);
            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, objectType);
            ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, value);
        }
    }
}
