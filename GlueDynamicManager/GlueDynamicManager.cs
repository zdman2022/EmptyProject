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

namespace GlueDynamicManager
{
    internal class GlueDynamicManager
    {
        private readonly JsonDiffPatch _jdp = new JsonDiffPatch();
        private readonly JsonDeltaFormatter _jdf = new JsonDeltaFormatter();
        private GlueJsonContainer _initialState;
        private GlueJsonContainer _curState;
        private readonly List<HybridScreen> _hybridScreens = new List<HybridScreen>();
        private readonly List<HybridEntity> _hybridEntities = new List<HybridEntity>();

        public static GlueDynamicManager Self { get; private set; } = new GlueDynamicManager();

        internal void SetInitialState(GlueJsonContainer glueJsonContainer)
        {
            _initialState = glueJsonContainer;
            ScreenManager.ScreenLoaded += ScreenLoadedHandler;
        }

        internal void UpdateState(GlueJsonContainer glueJsonContainer)
        {
            _curState = glueJsonContainer;
        }

        internal DynamicScreenState GetDynamicScreenState(string screenName)
        {
            var correctedScreenName = CorrectScreenName(screenName);
            if (ScreenIsDynamic(correctedScreenName))
            {
                var returnValue = new DynamicScreenState
                {
                    ScreenSave = _curState.Screens[correctedScreenName].Value
                };

                if (!string.IsNullOrEmpty(returnValue.ScreenSave.BaseScreen))
                    returnValue.BaseScreenSave = _curState.Screens[CorrectScreenName(returnValue.ScreenSave.BaseScreen)].Value;

                return returnValue;
            }

            return null;
        }

        internal EntityState GetEntityState(string entityName)
        {
            var correctedEntityName = CorrectEntityName(entityName);
            var returnValue = new EntityState
            {
                EntitySave = JsonConvert.DeserializeObject<EntitySave>(_curState.Entities[correctedEntityName].Json.ToString()),
                CustomVariablesSave = JsonConvert.DeserializeObject<List<CustomVariable>>(_curState.Entities[correctedEntityName].Json["CustomVariables"]?.ToString() ?? "[]"),
                StateCategoryList = JsonConvert.DeserializeObject<List<StateSaveCategory>>(_curState.Entities[correctedEntityName].Json["StateCategoryList"]?.ToString() ?? "[]")
            };

            return returnValue;
        }

        internal bool ScreenIsDynamic(string screenName)
        {
            if (_curState == null)
                return false;

            if (_curState.Screens.ContainsKey(screenName) && !_initialState.Screens.ContainsKey(screenName))
                return true;

            return false;
        }

        internal object GetProperty(object container, string name)
        {
            if (container is DynamicScreen dynamicScreen)
            {
                return dynamicScreen.PropertyFinder(name);
            }
            else if (container is DynamicEntity dynamicEntity)
            {
                return dynamicEntity.PropertyFinder(name);
            }
            else if (container is Screen screen)
            {
                var hScreen = _hybridScreens.Where(item => item.Screen == screen).FirstOrDefault();

                if (hScreen == null)
                    throw new Exception("Hybrid Screen not found");

                return hScreen.PropertyFinder(name);
            }
            //Entity
            else
            {
                var hEntity = _hybridEntities.Where(item => item.Entity == container).FirstOrDefault();

                if (hEntity == null)
                    throw new Exception("Hybrid Entity not found");

                return hEntity.PropertyFinder(name);
            }
        }

        internal bool ContainsEntity(string entityName)
        {
            return _curState.Entities.ContainsKey(CorrectEntityName(entityName));
        }

        private string CorrectEntityName(string entityName)
        {
            return entityName.Replace("Entities\\", "").Replace("Entities.", "");
        }

        private string CorrectScreenName(string entityName)
        {
            return entityName.Replace("Screens\\", "");
        }

        internal bool EntityIsDynamic(string entityName)
        {
            if (_curState == null)
                return false;

            var correctedEntityName = CorrectEntityName(entityName);

            if (_curState.Entities.ContainsKey(correctedEntityName) && !_initialState.Entities.ContainsKey(correctedEntityName))
                return true;

            return false;
        }

        private void ScreenLoadedHandler(Screen screen)
        {
            _hybridScreens.Add(new HybridScreen(screen));

            AddEventHandler(screen, "ActivityEvent", "ScreenActivityHandler");
            AddEventHandler(screen, "ActivityEditModeEvent", "ScreenActivityEditModeHandler");
            AddEventHandler(screen, "DestroyEvent", "ScreenDestroyHandler");

            ScreenDoChanges(screen, false);
        }

        private void ScreenDoChanges(Screen screen, bool addToManagers)
        {
            if (screen.GetType() != typeof(DynamicScreen))
            {
                var oldScreenJson = _initialState.Screens[screen.GetType().Name];
                var newScreenJson = _curState.Screens[screen.GetType().Name];

                var glueDifferences = _jdp.Diff(oldScreenJson.Json, newScreenJson.Json);
                var operations = _jdf.Format(glueDifferences);

                GlueElementOperationProcessor.ApplyOperations(_hybridScreens.First(item => item.Screen == screen), oldScreenJson.Value, newScreenJson.Value, glueDifferences, operations, addToManagers);
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
            var name = value.GetType().Name;

            return _curState.Entities.ContainsKey(name);
        }

        internal bool IsAttachedEntity(object value)
        {
            return _hybridEntities.Any(item => item.Entity == value);
        }

        private void ScreenDestroyHandler(object caller)
        {
            RemoveEventHandler(caller, "ActivityEvent", "ScreenActivityHandler");
            RemoveEventHandler(caller, "ActivityEditModeEvent", "ScreenActivityEditModeHandler");
            RemoveEventHandler(caller, "DestroyEvent", "ScreenDestroyHandler");

            _hybridScreens.Remove(_hybridScreens.First(item => item.Screen == caller));
        }

        private void EntityActivityHandler(object caller)
        {

        }

        private void EntityActivityEditModeHandler(object caller)
        {

        }

        private void EntityDestroyHandler(object caller)
        {
            RemoveEventHandler(caller, "ActivityEvent", "EntityActivityHandler");
            RemoveEventHandler(caller, "ActivityEditModeEvent", "EntityActivityEditModeHandler");
            RemoveEventHandler(caller, "DestroyEvent", "EntityDestroyHandler");

            _hybridEntities.Remove(_hybridEntities.First(item => item.Entity == caller));
        }

        public object AttachEntity(object instance, bool addToManagers)
        {
            if (instance is Screen)
                return null;

            _hybridEntities.Add(new HybridEntity(instance));

            AddEventHandler(instance, "ActivityEvent", "EntityActivityHandler");
            AddEventHandler(instance, "ActivityEditModeEvent", "EntityActivityEditModeHandler");
            AddEventHandler(instance, "DestroyEvent", "EntityDestroyHandler");

            EntityDoChanges(instance, addToManagers);

            return instance;
        }

        private void EntityDoChanges(object entity, bool addToManagers)
        {
            if (entity.GetType() != typeof(DynamicEntity))
            {
                var entityName = entity.GetType().Name;

                if (_initialState != null)
                {
                    var oldEntityJson = _initialState.Entities[entityName];
                    var newEntityJson = _curState.Entities[entityName];

                    var glueDifferences = _jdp.Diff(oldEntityJson.Json, newEntityJson.Json);
                    var operations = _jdf.Format(glueDifferences);

                    GlueElementOperationProcessor.ApplyOperations(_hybridEntities.First(item => item.Entity == entity), oldEntityJson.Value, newEntityJson.Value, glueDifferences, operations, addToManagers);
                }
            }
        }
    }
}
