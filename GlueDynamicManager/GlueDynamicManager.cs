using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.States;
using GlueControl.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Screens;
using System.Linq.Expressions;
using System.Reflection;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using GlueDynamicManager.Processors;
using Newtonsoft.Json.Linq;
using GlueCommunication.Json;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueControl.Managers;
using GlueControl;
using GlueCommunication;

namespace GlueDynamicManager
{
    internal class GlueDynamicManager
    {
        private readonly JsonDiffPatch _jdp = new JsonDiffPatch();
        private readonly JsonDeltaFormatter _jdf = new JsonDeltaFormatter();
        private GlueJsonContainer _initialState;
        private GlueJsonContainer _curState;
        private readonly List<IDynamic> _dynamicScreens = new List<IDynamic>();
        private readonly List<IDynamic> _dynamicEntities = new List<IDynamic>();

        public static GlueDynamicManager Self { get; private set; } = new GlueDynamicManager();
        public static bool ScreenIsLoading { get; private set; }

        internal void SetInitialState(GlueJsonContainer glueJsonContainer)
        {
            _initialState = glueJsonContainer;
            _curState = glueJsonContainer;
            ScreenManager.ScreenLoaded += ScreenLoadedHandler;
        }

        internal void UpdateState(GlueJsonContainer glueJsonContainer)
        {
            //Update ObjectFinder
            ObjectFinder.Self.GlueProject = glueJsonContainer.GetFullClone();
            foreach (var screen in ObjectFinder.Self.GlueProject.Screens)
            {
                screen.FixAllTypes();
            }
            foreach (var entity in ObjectFinder.Self.GlueProject.Entities)
            {
                entity.FixAllTypes();
            }

            //Do Updates
            foreach (var dynamicEntity in _dynamicEntities)
            {
                EntityDoChanges(dynamicEntity, true, _curState.Entities.ContainsKey(dynamicEntity.TypeName) ? _curState.Entities[dynamicEntity.TypeName] : null, glueJsonContainer.Entities.ContainsKey(dynamicEntity.TypeName) ? glueJsonContainer.Entities[dynamicEntity.TypeName] : null);
            }

            foreach (var dynamicScreen in _dynamicScreens)
            {
                ScreenDoChanges(dynamicScreen, true, _curState.Screens.ContainsKey(dynamicScreen.TypeName) ? _curState.Screens[dynamicScreen.TypeName] : null, glueJsonContainer.Screens.ContainsKey(dynamicScreen.TypeName) ? glueJsonContainer.Screens[dynamicScreen.TypeName] : null);
            }

            _curState = glueJsonContainer;
        }

        internal DynamicScreenState GetDynamicScreenState(string screenNameGlue)
        {
            var screenNameGame = CommandReceiver.GlueToGameElementName(screenNameGlue);
            if (ElementIsDynamic(screenNameGlue))
            {
                var returnValue = new DynamicScreenState
                {
                    ScreenSave = _curState.Screens[screenNameGame].Value
                };

                if (!string.IsNullOrEmpty(returnValue.ScreenSave.BaseScreen))
                    returnValue.BaseScreenSave = _curState.Screens[CommandReceiver.GlueToGameElementName(returnValue.ScreenSave.BaseScreen)].Value;

                return returnValue;
            }

            return null;
        }

        internal EntityState GetEntityState(string entityNameGlue)
        {
            var entityNameGame = CommandReceiver.GlueToGameElementName(entityNameGlue);
            var returnValue = new EntityState
            {
                EntitySave = JsonConvert.DeserializeObject<EntitySave>(_curState.Entities[entityNameGame].Json.ToString()),
                CustomVariablesSave = JsonConvert.DeserializeObject<List<CustomVariable>>(_curState.Entities[entityNameGame].Json["CustomVariables"]?.ToString() ?? "[]"),
                StateCategoryList = JsonConvert.DeserializeObject<List<StateSaveCategory>>(_curState.Entities[entityNameGame].Json["StateCategoryList"]?.ToString() ?? "[]")
            };

            return returnValue;
        }

        public bool ElementIsDynamic(string elementNameGlue)
        {
            if (_curState == null)
                return false;

            bool IsGlueScreenDynamic(string screenNameGlueInner)
            {
                var screenNameGame = CommandReceiver.GlueToGameElementName(screenNameGlueInner);
                return _curState.Screens.ContainsKey(screenNameGame) && !_initialState.Screens.ContainsKey(screenNameGame);
            }
            bool IsGlueEntityDynamic(string entityNameGlueInner)
            {
                var entityNameGame = CommandReceiver.GlueToGameElementName(entityNameGlueInner);
                return _curState.Entities.ContainsKey(entityNameGame) && !_initialState.Entities.ContainsKey(entityNameGame);
            }

            List<string> glueElementNames = new List<string>();
            glueElementNames.Add(elementNameGlue);
            var glueElement = ObjectFinder.Self.GetElement(elementNameGlue);

            if (glueElement == null)
                throw new Exception($"Element {elementNameGlue} was not found");

            var baseNames = ObjectFinder.Self.GetAllBaseElementsRecursively(glueElement).Select(item => item.Name);
            glueElementNames.AddRange(baseNames);

            var areAllDynamic =
                elementNameGlue.StartsWith("Entities\\")
                ? glueElementNames.All(item => IsGlueEntityDynamic(item))
                : glueElementNames.All(item => IsGlueScreenDynamic(item));

            return areAllDynamic;
        }

        //internal object GetProperty(object container, string name)
        //{
        //    if (container is DynamicScreen dynamicScreen)
        //    {
        //        return dynamicScreen.PropertyFinder(name);
        //    }
        //    else if (container is DynamicEntity dynamicEntity)
        //    {
        //        return dynamicEntity.GetPropertyValue(name);
        //    }
        //    else if (container is Screen screen)
        //    {
        //        var hScreen = _hybridScreens.Where(item => item.Screen == screen).FirstOrDefault();

        //        if (hScreen == null)
        //            throw new Exception("Hybrid Screen not found");

        //        return hScreen.GetPropertyValue(name);
        //    }
        //    //Entity
        //    else
        //    {
        //        var hEntity = _hybridEntities.Where(item => item.Entity == container).FirstOrDefault();

        //        if (hEntity == null)
        //            throw new Exception("Hybrid Entity not found");

        //        return hEntity.GetPropertyValue(name);
        //    }
        //}

        internal bool ContainsEntity(string entityNameGlue)
        {
            return _curState.Entities.ContainsKey(CommandReceiver.GlueToGameElementName(entityNameGlue));
        }

        private void ScreenLoadedHandler(Screen screen)
        {
            ScreenIsLoading = true;

            try
            {
                IDynamic dScreen;

                if (screen is DynamicScreen dynamicScreen)
                {
                    dScreen = dynamicScreen;
                    _dynamicScreens.Add(dynamicScreen);
                }
                else
                {
                    dScreen = new HybridScreen(screen);
                    _dynamicScreens.Add(dScreen);
                }

                AddEventHandler(screen, "ActivityEvent", "ScreenActivityHandler");
                AddEventHandler(screen, "ActivityEditModeEvent", "ScreenActivityEditModeHandler");
                AddEventHandler(screen, "DestroyEvent", "ScreenDestroyHandler");


                ScreenDoChanges(dScreen, screen is DynamicScreen ? true : false,
                    _initialState.Screens.ContainsKey(dScreen.TypeName) ? _initialState.Screens[dScreen.TypeName] : null,
                    _curState.Screens.ContainsKey(dScreen.TypeName) ? _curState.Screens[dScreen.TypeName] : null);

            }
            finally
            {
                ScreenIsLoading = false;
            }
        }

        private void ScreenDoChanges(IDynamic screen, bool addToManagers, GlueJsonContainer.JsonContainer<ScreenSave> oldScreenJson, GlueJsonContainer.JsonContainer<ScreenSave> newScreenJson)
        {
            if (_curState == null)
                return;

            if (screen.GetType() == typeof(DynamicScreen))
            {
                oldScreenJson = new GlueJsonContainer.JsonContainer<ScreenSave>("{}");
            }

            if (oldScreenJson != null && newScreenJson != null)
            {
                var glueDifferences = _jdp.Diff(oldScreenJson.Json, newScreenJson.Json);
                var operations = _jdf.Format(glueDifferences);

                GlueElementOperationProcessor.ApplyOperations(screen, oldScreenJson.Value, newScreenJson.Value, glueDifferences, operations, addToManagers);
            }
        }

        private void AddEventHandler(object obj, string eventName, string handlerName)
        {
            var ei = obj.GetType().GetEvent(eventName);
            var tDelegate = ei.EventHandlerType;
            var miHandler = typeof(GlueDynamicManager).GetMethod(handlerName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var d = Delegate.CreateDelegate(tDelegate, this, miHandler);
            var addHandler = ei.GetAddMethod();
            object[] addHandlerArgs = { d };
            addHandler.Invoke(obj, addHandlerArgs);
        }

        private void RemoveEventHandler(object obj, string eventName, string handlerName)
        {
            var ei = obj.GetType().GetEvent(eventName);
            var tDelegate = ei.EventHandlerType;
            var miHandler = typeof(GlueDynamicManager).GetMethod(handlerName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var d = Delegate.CreateDelegate(tDelegate, this, miHandler);
            ei.RemoveEventHandler(obj, d);
        }

        private void ScreenActivityHandler(object caller)
        {

        }

        private void ScreenActivityEditModeHandler(object caller)
        {

        }

        internal bool IsEntity(object value)
        {
            var name = value.GetType().FullName;

            return _curState.Entities.ContainsKey(name);
        }

        internal bool IsAttachedEntity(object value)
        {
            return _dynamicEntities.Any(item => item.Equals(value));
        }

        private void ScreenDestroyHandler(object caller)
        {
            RemoveEventHandler(caller, "ActivityEvent", "ScreenActivityHandler");
            RemoveEventHandler(caller, "ActivityEditModeEvent", "ScreenActivityEditModeHandler");
            RemoveEventHandler(caller, "DestroyEvent", "ScreenDestroyHandler");

            var screen = _dynamicScreens.First(item => item == caller);
            if (screen is HybridGlueElement)
                screen.Destroy();
            _dynamicScreens.Remove(screen);
        }

        private void EntityInitializeHandler(object caller, bool addToManagers)
        {
            var entity = _dynamicEntities.First(item => item.Equals(caller));
            if (_initialState != null)
                EntityDoChanges(entity, addToManagers, _initialState.Entities.ContainsKey(entity.TypeName) ? _initialState.Entities[entity.TypeName] : null, _curState.Entities.ContainsKey(entity.TypeName) ? _curState.Entities[entity.TypeName] : null);

        }

        private void EntityActivityHandler(object caller)
        {

        }

        private void EntityActivityEditModeHandler(object caller)
        {

        }

        private void EntityDestroyHandler(object caller)
        {


            if (caller is DynamicEntity dynamicEntity)
            {
                dynamicEntity.InitializeEvent -= EntityInitializeHandler;
                dynamicEntity.ActivityEvent -= EntityActivityHandler;
                dynamicEntity.ActivityEditModeEvent -= EntityActivityEditModeHandler;
                dynamicEntity.DestroyEvent -= EntityDestroyHandler;

                _dynamicEntities.Remove(dynamicEntity);
            }
            else
            {
                RemoveEventHandler(caller, "InitializeEvent", "EntityInitializeHandler");
                RemoveEventHandler(caller, "ActivityEvent", "EntityActivityHandler");
                RemoveEventHandler(caller, "ActivityEditModeEvent", "EntityActivityEditModeHandler");
                RemoveEventHandler(caller, "DestroyEvent", "EntityDestroyHandler");

                var entity = _dynamicEntities.First(item => item.Equals(caller));

                if (entity is HybridGlueElement)
                    entity.Destroy();

                _dynamicEntities.Remove(entity);
            }

        }

        public object AttachEntity(object instance, bool addToManagers)
        {
            if (instance is Screen)
                return null;

            IDynamic dEntity;

            if (instance is DynamicEntity dynamicEntity)
            {
                dEntity = dynamicEntity;
            }
            else
            {
                dEntity = new HybridEntity(instance);
            }
            _dynamicEntities.Add(dEntity);

            AddEventHandler(instance, "InitializeEvent", "EntityInitializeHandler");
            AddEventHandler(instance, "ActivityEvent", "EntityActivityHandler");
            AddEventHandler(instance, "ActivityEditModeEvent", "EntityActivityEditModeHandler");
            AddEventHandler(instance, "DestroyEvent", "EntityDestroyHandler");

            if (_initialState != null)
                EntityDoChanges(dEntity, addToManagers, _initialState.Entities.ContainsKey(dEntity.TypeName) ? _initialState.Entities[dEntity.TypeName] : null, _curState.Entities.ContainsKey(dEntity.TypeName) ? _curState.Entities[dEntity.TypeName] : null);

            return instance;
        }

        private void EntityDoChanges(IDynamic entity, bool addToManagers, GlueJsonContainer.JsonContainer<EntitySave> oldEntityJson, GlueJsonContainer.JsonContainer<EntitySave> newEntityJson)
        {
            if (_curState == null)
                return;

            if (entity.GetType() == typeof(DynamicEntity))
            {
                oldEntityJson = new GlueJsonContainer.JsonContainer<EntitySave>("{}");
            }

            if (oldEntityJson != null && newEntityJson != null)
            {
                var glueDifferences = _jdp.Diff(oldEntityJson.Json, newEntityJson.Json);
                var operations = _jdf.Format(glueDifferences);

                GlueElementOperationProcessor.ApplyOperations(entity, oldEntityJson.Value, newEntityJson.Value, glueDifferences, operations, addToManagers);
            }
        }
    }
}
