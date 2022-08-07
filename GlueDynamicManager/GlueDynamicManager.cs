using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.States;
using GlueControl.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager
{
    internal class GlueDynamicManager
    {
        private GlueJsonContainer _initialState;
        private GlueJsonContainer _curState;

        public static GlueDynamicManager Self { get; private set; } = new GlueDynamicManager();

        internal void SetInitialState(GlueJsonContainer glueJsonContainer)
        {
            _initialState = glueJsonContainer;
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

                returnValue.ScreenSave = JsonConvert.DeserializeObject<ScreenSave>(_curState.Screens[correctedScreenName].ToString());

                if (!string.IsNullOrEmpty(returnValue.ScreenSave.BaseScreen))
                    returnValue.BaseScreenSave = JsonConvert.DeserializeObject<ScreenSave>(_curState.Screens[CorrectScreenName(returnValue.ScreenSave.BaseScreen)].ToString());

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

                returnValue.EntitySave = JsonConvert.DeserializeObject<EntitySave>(_curState.Entities[correctedEntityName].ToString());
                returnValue.CustomVariablesSave = JsonConvert.DeserializeObject<List<CustomVariable>>(_curState.Entities[correctedEntityName]["CustomVariables"].ToString());
                returnValue.StateCategoryList = JsonConvert.DeserializeObject<List<StateSaveCategory>>(_curState.Entities[correctedEntityName]["StateCategoryList"].ToString());

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
    }
}
