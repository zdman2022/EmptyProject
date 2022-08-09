using FlatRedBall.Content.Instructions;
using FlatRedBall.Graphics;
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
using System.Text.RegularExpressions;

namespace GlueDynamicManager.Processors
{
    internal class EntityOperationProcessor
    {
        private static Regex PathMatchRegEx_NamedObject = new Regex("^/NamedObjects/\\d+$");
        private static Regex PathMatchRegEx_NamedObjectInstructionSave = new Regex("^/NamedObjects/(\\d+)/InstructionSaves/(\\d+)(/Value)?$");

        internal static void ApplyOperations(HybridEntity entity, EntitySave oldEntitySave, EntitySave newEntitySave, JToken glueDifferences, IList<Operation> operations)
        {
            foreach (var operation in operations)
            {
                if (operation.Op == "add")
                {
                    if (PathMatchRegEx_NamedObject.IsMatch(operation.Path))
                    {
                        var nos = JsonConvert.DeserializeObject<NamedObjectSave>(operation.Value.ToString());

                        var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, newEntitySave);
                        NamedObjectSaveHelper.InitializeNamedObject(nos, itemContainer, newEntitySave, entity.PropertyFinder, out var positionedObjectLists, out var instancedObjects, out var instancedEntities);

                        DoInitialize(entity, newEntitySave, positionedObjectLists, instancedObjects, instancedEntities);
                        AddToManagers(entity, newEntitySave, positionedObjectLists, instancedObjects, instancedEntities);

                        entity.PositionedObjectLists.AddRange(positionedObjectLists);
                        entity.InstancedObjects.AddRange(instancedObjects);
                        entity.InstancedEntities.AddRange(instancedEntities);
                    }else if (PathMatchRegEx_NamedObjectInstructionSave.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var newInstruction = newEntitySave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = entity.PropertyFinder(newEntitySave.NamedObjects[noIndex].InstanceName);

                        ApplyInstruction(newInstruction, newEntitySave, entity, obj.GetType().Name, obj);
                    }
                }
                else if (operation.Op == "replace")
                {
                    if (PathMatchRegEx_NamedObjectInstructionSave.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var oldInstruction = oldEntitySave.NamedObjects[noIndex].InstructionSaves[isIndex];
                        var newInstruction = newEntitySave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = entity.PropertyFinder(newEntitySave.NamedObjects[noIndex].InstanceName);

                        CleanupOldInstruction(oldInstruction);
                        ApplyInstruction(newInstruction, newEntitySave, entity, obj.GetType().Name, obj);
                    }
                }
            }
        }

        private static void DoInitialize(HybridEntity entity, EntitySave entitySave, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
        {
            bool oldShapeManagerSuppressAdd = FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue;
            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = true;

            for (int i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, entitySave);
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
                        var convertedValue = ValueConverter.ConvertValue(instruction, entitySave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                        instance.Value.SetVariable(instruction.Member, convertedValue);
                    }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private static void AddToManagers(HybridEntity entity, EntitySave entitySave, List<PositionedListContainer> positionedObjectLists, List<ObjectContainer> instancedObjects, List<DynamicEntityContainer> instancedEntities)
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

        private static void ApplyInstruction(InstructionSave instruction, EntitySave entitySave, HybridEntity entity, string objectType, object value)
        {
            var convertedValue = ValueConverter.ConvertValue(instruction, entitySave);
            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, objectType);
            ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, value);
        }
    }
}
