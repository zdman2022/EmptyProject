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
        private JsonDiffPatch _jdp = new JsonDiffPatch();
        private JsonDeltaFormatter _jdf = new JsonDeltaFormatter();
        private GlueJsonContainer _initialState;
        private GlueJsonContainer _curState;
        private List<HybridScreen> _hybridScreens = new List<HybridScreen>();

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
                var returnValue = new DynamicScreenState();

                returnValue.ScreenSave = _curState.Screens[correctedScreenName].Value;

                if (!string.IsNullOrEmpty(returnValue.ScreenSave.BaseScreen))
                    returnValue.BaseScreenSave = _curState.Screens[CorrectScreenName(returnValue.ScreenSave.BaseScreen)].Value;

                return returnValue;
            }

            return null;
        }

        internal DynamicEntityState GetDynamicEntityState(string entityName)
        {
            var correctedEntityName = CorrectEntityName(entityName);
            if (EntityIsDynamic(correctedEntityName))
            {
                var returnValue = new DynamicEntityState();

                returnValue.EntitySave = JsonConvert.DeserializeObject<EntitySave>(_curState.Entities[correctedEntityName].Json.ToString());
                returnValue.CustomVariablesSave = JsonConvert.DeserializeObject<List<CustomVariable>>(_curState.Entities[correctedEntityName].Json["CustomVariables"].ToString());
                returnValue.StateCategoryList = JsonConvert.DeserializeObject<List<StateSaveCategory>>(_curState.Entities[correctedEntityName].Json["StateCategoryList"].ToString());

                return returnValue;
            }

            return null;
        }

        internal bool ScreenIsDynamic(string screenName)
        {
            if(_curState == null)
                return false;

            if (_curState.Screens.ContainsKey(screenName) && !_initialState.Screens.ContainsKey(screenName))
                return true;

            return false;
        }

        internal bool ContainsEntity(string entityName)
        {
            return _curState.Entities.ContainsKey(CorrectEntityName(entityName));
        }

        private string CorrectEntityName(string entityName)
        {
            return entityName.Replace("Entities\\", "");
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

            ScreenDoChanges(screen, true);
        }

        private void ScreenDoChanges(Screen screen, bool beforeInitialization)
        {
            if (screen.GetType() != typeof(DynamicScreen))
            {
                var oldScreenJson = _initialState.Screens[screen.GetType().Name];
                var newScreenJson = _curState.Screens[screen.GetType().Name];

                var glueDifferences = _jdp.Diff(oldScreenJson.Json, newScreenJson.Json);
                var operations = _jdf.Format(glueDifferences);

                ScreenOperationProcessor.ApplyOperations(_hybridScreens.First(item => item.Screen == screen), oldScreenJson.Value, newScreenJson.Value, glueDifferences, operations);
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

        private void ScreenDestroyHandler(object caller)
        {
            RemoveEventHandler(caller, "ActivityEvent", "ScreenActivityHandler");
            RemoveEventHandler(caller, "ActivityEditModeEvent", "ScreenActivityEditModeHandler");
            RemoveEventHandler(caller, "DestroyEvent", "ScreenDestroyHandler");

            _hybridScreens.Remove(_hybridScreens.First(item => item.Screen == caller));
        }
    }
}
