using FlatRedBall;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Instructions;
using FlatRedBall.Screens;
using GlueCommunication;
using GlueControl;
using GlueControl.Models;
using GlueControl.Screens;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueDynamicManager.GlueHelpers;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlueDynamicManager.Processors
{
    internal class GlueElementOperationProcessor
    {
        internal static void ApplyOperations(IDynamic element, GlueElement oldSave, GlueElement newSave, JToken glueDifferences, IList<Operation> operations, bool addToManagers)
        {
            foreach (var operation in operations)
            {
                if (!GlueDynamicManager.ScreenIsLoading)
                {
                    RestartRequired(true);
                    return;
                }

                if (!operation.Path.StartsWith("/"))
                    throw new NotImplementedException();

                var items = operation.Path.Split('/');

                var workingValue = GetWorkingValue(operation.Op, items.Skip(1).ToArray(), oldSave, newSave, "");

                if (operation.Op == "replace" || operation.Op == "remove")
                {
                    var lst = workingValue.OldValue as IList;

                    if (lst != null)
                    {
                        foreach (var item in lst)
                        {
                            RemoveItem(item, workingValue.OldParents, workingValue.Path, element, addToManagers);
                        }
                    }
                    else
                    {
                        RemoveItem(workingValue.OldValue, workingValue.OldParents, workingValue.Path, element, addToManagers);
                    }
                }

                if (operation.Op == "replace" || operation.Op == "add")
                {
                    var lst = workingValue.NewValue as IList;

                    if (lst != null)
                    {
                        //Order the list so lists are created first
                        if(lst is List<NamedObjectSave> nosList)
                        {
                            lst = nosList.OrderBy(item => item.InstanceType == "FlatRedBall.Math.PositionedObjectList<T>" ? 1 : 2).ToList();
                        }

                        foreach (var item in lst)
                        {
                            AddItem(item, workingValue.NewParents, workingValue.Path, element, addToManagers, newSave);
                        }
                    }
                    else
                    {
                        AddItem(workingValue.NewValue, workingValue.NewParents, workingValue.Path, element, addToManagers, newSave);
                    }
                }
            }
        }

        private static void RestartRequired(bool mustRestartGame)
        {
            if (mustRestartGame || GlueDynamicManager.ScreenIsLoading)
            {
                GameConnectionManager.Self.SendItem(new GameConnectionManager.Packet
                {
                    PacketType = "Command",
                    Payload = JObject.Parse(@"{Command: ""Restart Game""}").ToString()
                });
            }
            else
            {
                CommandReceiver.RestartScreenRerunCommands(true, ScreenManager.IsInEditMode);
            }
        }

        private static void RemoveItem(object item, List<object> parents, string path, IDynamic element, bool addToManagers)
        {
            if (path == "/NamedObjects" || path == "/NamedObjects/ContainedObjects")
            {
                var propName = ((NamedObjectSave)item).InstanceName;
                var obj = element.GetPropertyValue(propName);
                if(obj != null)
                    InstanceDestroy.Destroy(obj);
                element.SetPropertyValue(propName, null, null, null);
            }
            else if (path.StartsWith("/NamedObjects/InstructionSaves"))
            {
                var instructionSave = item as InstructionSave;

                if (instructionSave == null)
                    instructionSave = (InstructionSave)parents[4];

                CleanupOldInstruction(instructionSave);
            }
            else if (path == "/CustomVariables")
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddItem(object item, List<object> parents, string path, IDynamic element, bool addToManagers, GlueElement newSave)
        {
            if (path == "/NamedObjects" || path == "/NamedObjects/ContainedObjects")
            {
                AddNamedObject(element, newSave, (NamedObjectSave)item, addToManagers);
            }
            else if (path.StartsWith("/NamedObjects/InstructionSaves") || path.StartsWith("/NamedObjects/ContainedObjects/InstructionSaves"))
            {
                var obj = path.StartsWith("/NamedObjects/InstructionSaves")
                    ? element.GetPropertyValue(((NamedObjectSave)parents[2]).InstanceName)
                    : element.GetPropertyValue(((NamedObjectSave)parents[4]).InstanceName);

                var instructionSave = item as InstructionSave;

                if (instructionSave == null)
                    instructionSave = (InstructionSave)parents[4];

                ApplyInstruction(instructionSave, newSave, element, obj.GetType().Name, obj);
            }
            else if (path.StartsWith("/CustomVariables"))
            {
                ApplyCustomVariable(element, (CustomVariable)item, newSave);
            }
            else if (path == ("/NamedObjects/InstanceName")) //Note: NamedObject fields also need to be handled for ContainedObjects fields
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path.StartsWith("/NamedObjects/SourceClassType"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path.StartsWith("/NamedObjects/Properties")) //Can be starts with, like instruction saves
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/SourceFile"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/SourceName"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/AttachToContainer"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/GenerateTimedEmit"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/DefinedByBase"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/SourceType"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/SourceGenericType"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/ExposedInDerived"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/NamedObjects/InstantiatedByBase"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path.StartsWith("/Tags"))
            {
                //Ignore for now
            }
            else if (path == ("/Source"))
            {
                //Ignore for now
                //throw new NotImplementedException();
            }
            else if (path == ("/CreatedByOtherEntities"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/Is2D"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path.StartsWith("/Properties"))
            {
                //Todo
                throw new NotImplementedException();
            }
            else if (path == ("/Name"))
            {
                //Ignore for now
            }
            else if (path.StartsWith("/CustomClassesForExport"))
            {
                //Ignore
            }
            else if (path == ("/BaseScreen") || path == ("/BaseElement"))
            {
                var glueName = item as string;
                var gameTypeBase = CommandReceiver.GlueToGameElementName(glueName);

                if (element is HybridEntity)
                {
                    throw new NotImplementedException();
                }

                var currentScreenType = ScreenManager.CurrentScreen.GetType().FullName;
                var matches = currentScreenType == gameTypeBase;

                if (!matches)
                {
                    // Restart the screen with the new base type as specified by item
                    throw new NotImplementedException("");
                }
            }
            else if (path.StartsWith("/ReferencedFiles"))
            {
                //Ignore for now
                //throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static WorkingValue GetWorkingValue(string operation, string[] items, object oldValue, object newValue, string currentPath, WorkingValue workingValue = null)
        {
            if (workingValue == null)
                workingValue = new WorkingValue();

            if (items.Length == 0)
            {
                workingValue.Path = currentPath;
                workingValue.OldValue = operation == "replace" || operation == "remove" ? oldValue : null;
                workingValue.NewValue = operation == "replace" || operation == "add" ? newValue : null;

                return workingValue;
            }

            workingValue.OldParents.Add(oldValue);
            workingValue.NewParents.Add(newValue);

            var currentItem = items[0];

            if (int.TryParse(currentItem, out var i))
            {
                var oldList = oldValue as IList;
                var newList = newValue as IList;

                return GetWorkingValue(operation, items.Skip(1).ToArray(), oldList.Count > i ? oldList[i] : null, newList.Count > i ? newList[i] : null, currentPath, workingValue);
            }
            else
            {
                return GetWorkingValue(operation, items.Skip(1).ToArray(), GetValueForObject(oldValue, currentItem), GetValueForObject(newValue, currentItem), currentPath + "/" + currentItem, workingValue);
            }
        }

        private static object GetValueForObject(object value, string name)
        {
            var prop = value.GetType().GetProperty(name);

            if (prop != null)
                return prop.GetValue(value);

            var field = value.GetType().GetField(name);

            if (field != null)
                return field.GetValue(value);

            return null;
        }

        private static void ApplyCustomVariable(IDynamic element, CustomVariable customVariable, GlueElement save)
        {
            Action body = () =>
            {
                var convertedValue = ValueConverter.ConvertValue(customVariable, save);
                var convertedPropertyName = ValueConverter.ConvertForPropertyName(customVariable.Name, element);
                element.SetPropertyValue(convertedPropertyName, convertedValue, null, null);
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }

        private static void AddNamedObject(IDynamic element, GlueElement newSave, NamedObjectSave nos, bool addToManagers)
        {
            Action body = () =>
            {
                var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, newSave);
                NamedObjectSaveHelper.InitializeNamedObject(element, nos, itemContainer, newSave, element.GetPropertyValue, out var instancedObjects);

                DoInitialize(element, newSave, instancedObjects);
                if (addToManagers)
                    AddToManagers(element, newSave, instancedObjects);

                foreach(var instance in instancedObjects)
                {
                    element.SetPropertyValue(instance.Name, instance, instance.NamedObjectSave, instance.CombinedInstructionSaves);
                }

                foreach(var containedObject in nos.ContainedObjects)
                {
                    AddNamedObject(element, newSave, containedObject, addToManagers);
                }
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }

        private static void DoInitialize(IDynamic element, GlueElement save, List<ObjectContainer> instancedObjects)
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
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.NamedObjectSave.SourceClassType);
                        var convertedPropertyName = ValueConverter.ConvertForPropertyName(instruction.Member, instance.Value);
                        ScreenManager.CurrentScreen.ApplyVariable(convertedPropertyName, convertedValue, instance.Value);
                    }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private static void AddToManagers(IDynamic element, GlueElement save, List<ObjectContainer> instancedObjects)
        {
            for (var i = 0; i < instancedObjects.Count; i++)
            {
                var instance = instancedObjects[i];

                if (instance.Value is DynamicEntity dynamicEntity)
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

        private static void ApplyInstruction(InstructionSave instruction, GlueElement save, IDynamic element, string objectType, object value)
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

        private static WorkingValue GetWorkingValue(string[] items, GlueElement oldSave, GlueElement newSave)
        {
            throw new NotImplementedException();
        }

        private static bool IsPath(string path, Regex fullPath, Regex itemPath, out Match fullMatch, out Match match)
        {
            fullMatch = fullPath.Match(path);
            match = fullMatch != null ? null : itemPath.Match(path);

            return fullMatch != null || match != null;
        }

        private class WorkingValue
        {
            public List<object> OldParents { get; } = new List<object>();
            public List<object> NewParents { get; } = new List<object>();
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public string Path { get; internal set; }
        }
    }
}