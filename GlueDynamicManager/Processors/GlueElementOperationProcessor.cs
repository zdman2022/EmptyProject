using FlatRedBall;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Instructions;
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
using System.Threading.Tasks;

namespace GlueDynamicManager.Processors
{
    internal class GlueElementOperationProcessor
    {
        private static Regex PathMatchRegEx_NamedObject = new Regex("^/NamedObjects$");
        private static Regex PathMatchRegEx_NamedObject_Item = new Regex("^/NamedObjects/(\\d+)?$");
        private static Regex PathMatchRegEx_NamedObjectInstructionSave_Item = new Regex("^/NamedObjects/(\\d+)/InstructionSaves/(\\d+)(/Value)?$");
        private static Regex PathMatchRegEx_NamedObjectContainedObject = new Regex("^/NamedObjects/(\\d+)/ContainedObjects$");
        private static Regex PathMatchRegEx_NamedObjectContainedObject_Item = new Regex("^/NamedObjects/(\\d+)/ContainedObjects/(\\d+)$");
        private static Regex PathMatchRegEx_CustomVariables_Field = new Regex("^/CustomVariables/(\\d+)/([^/]*)$");

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
                    else if (PathMatchRegEx_NamedObjectInstructionSave_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var newInstruction = newSave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = element.PropertyFinder(newSave.NamedObjects[noIndex].InstanceName);

                        ApplyInstruction(newInstruction, newSave, element, obj.GetType().Name, obj);
                    }
                    else if(PathMatchRegEx_NamedObjectContainedObject_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectContainedObject_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var coIndex = int.Parse(match.Groups[2].Value);

                        var nos = newSave.NamedObjects[noIndex];
                        var co = nos.ContainedObjects[coIndex];

                        AddNamedObject(element, newSave, co, addToManagers);
                    }
                    else if(PathMatchRegEx_NamedObjectContainedObject.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectContainedObject.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);

                        foreach(var co in newSave.NamedObjects[noIndex].ContainedObjects)
                        {
                            AddNamedObject(element, newSave, co, addToManagers);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (operation.Op == "replace")
                {
                    if (PathMatchRegEx_NamedObjectInstructionSave_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectInstructionSave_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var isIndex = int.Parse(match.Groups[2].Value);

                        var oldInstruction = oldSave.NamedObjects[noIndex].InstructionSaves[isIndex];
                        var newInstruction = newSave.NamedObjects[noIndex].InstructionSaves[isIndex];

                        var obj = element.PropertyFinder(newSave.NamedObjects[noIndex].InstanceName);

                        CleanupOldInstruction(oldInstruction);
                        ApplyInstruction(newInstruction, newSave, element, obj.GetType().Name, obj);
                    }
                    else if(PathMatchRegEx_CustomVariables_Field.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_CustomVariables_Field.Match(operation.Path);

                        var cvIndex = int.Parse(match.Groups[1].Value);

                        ApplyCustomVariable(element, newSave.CustomVariables[cvIndex], newSave);
                    }
                    else if (PathMatchRegEx_NamedObjectContainedObject_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectContainedObject_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var coIndex = int.Parse(match.Groups[2].Value);

                        var nos = newSave.NamedObjects[noIndex];
                        var co = nos.ContainedObjects[coIndex];

                        element.RemoveNamedObject(oldSave.NamedObjects[noIndex].ContainedObjects[coIndex]);
                        AddNamedObject(element, newSave, nos, addToManagers);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }else if(operation.Op == "remove")
                {
                    if (PathMatchRegEx_NamedObject_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObject_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);

                        var removeNO = oldSave.NamedObjects[noIndex];

                        element.RemoveNamedObject(removeNO);
                    }
                    else if (PathMatchRegEx_NamedObjectContainedObject_Item.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectContainedObject_Item.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);
                        var coIndex = int.Parse(match.Groups[2].Value);

                        element.RemoveNamedObject(oldSave.NamedObjects[noIndex].ContainedObjects[coIndex]);
                    }
                    else if (PathMatchRegEx_NamedObjectContainedObject.IsMatch(operation.Path))
                    {
                        var match = PathMatchRegEx_NamedObjectContainedObject.Match(operation.Path);

                        var noIndex = int.Parse(match.Groups[1].Value);

                        foreach (var co in oldSave.NamedObjects[noIndex].ContainedObjects)
                        {
                            element.RemoveNamedObject(co);
                        }
                    }
                    else
                    {
                        
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static void ApplyCustomVariable(HybridGlueElement element, CustomVariable customVariable, GlueElement save)
        {
            Action body = () =>
            {
                var convertedValue = ValueConverter.ConvertValue(customVariable, save);
                var convertedPropertyName = ValueConverter.ConvertForPropertyName(customVariable.Name, element.GlueElement);
                ScreenManager.CurrentScreen.ApplyVariable(convertedPropertyName, convertedValue, element.GlueElement);
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }

        private static void AddNamedObject(HybridGlueElement element, GlueElement newSave, NamedObjectSave nos, bool addToManagers)
        {
            Action body = () =>
            {
                var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, newSave);
                NamedObjectSaveHelper.InitializeNamedObject(element.GlueElement, nos, itemContainer, newSave, element.PropertyFinder, out var instancedObjects);

                DoInitialize(element, newSave, instancedObjects);
                if (addToManagers)
                    AddToManagers(element, newSave, instancedObjects);

                element.InstancedObjects.AddRange(instancedObjects);
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }

        private static void DoInitialize(HybridGlueElement element, GlueElement save, List<ObjectContainer> instancedObjects)
        {
            bool oldShapeManagerSuppressAdd = FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue;
            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = true;

            for (int i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if (instance.Value is DynamicEntity dynamicEntity)
                {
                    if (instance.CombinedInstructionSaves != null)
                        foreach (var instruction in instance.CombinedInstructionSaves)
                        {
                            var convertedValue = ValueConverter.ConvertValue(instruction, save);
                            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                            dynamicEntity.SetVariable(instruction.Member, convertedValue);
                        }
                }
                else
                {
                    if (instance.CombinedInstructionSaves != null)
                        foreach (var instruction in instance.CombinedInstructionSaves)
                        {
                            var convertedValue = ValueConverter.ConvertValue(instruction, save);
                            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);
                            ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                        }
                }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private static void AddToManagers(HybridGlueElement element, GlueElement save, List<ObjectContainer> instancedObjects)
        {
            for (var i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if(instance.Value is DynamicEntity dynamicEntity)
                {
                    dynamicEntity.AddToManagers(null);
                }
                else
                {
                    // todo: need to support layers
                    FlatRedBall.Graphics.Layer layer = null;
                    InstanceAddToManager.AddToManager(instancedObjects[i], instancedObjects, layer);
                }
            }
        }

        private static void CleanupOldInstruction(InstructionSave oldInstruction)
        {
            //Here, we can clean up any registration or things that a new instruction won't override
        }

        private static void ApplyInstruction(InstructionSave instruction, GlueElement save, HybridGlueElement element, string objectType, object value)
        {
            Action body = () =>
            {
                var convertedValue = ValueConverter.ConvertValue(instruction, save);
                convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, objectType);
                var convertedPropertyName = ValueConverter.ConvertForPropertyName(instruction.Member, value);
                ScreenManager.CurrentScreen.ApplyVariable(convertedPropertyName, convertedValue, value);
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }
    }
}
