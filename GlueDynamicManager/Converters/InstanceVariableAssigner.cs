using FlatRedBall.TileGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager
{
    internal class InstanceVariableAssigner
    {
        internal static bool TryAssignVariable(string member, object value, object runtimeInstance)
        {
            // Convert the value, in case we have json modifying float->double etc
            if(runtimeInstance is LayeredTileMap layeredTileMap)
            {
                return TryAssignLayeredTileMapVariable(member, value, layeredTileMap);
            }

            return false;
        }

        private static bool TryAssignLayeredTileMapVariable(string member, object value, LayeredTileMap layeredTileMap)
        {
            switch(member)
            {
                case "CreateEntitiesFromTiles":
                case "ShiftMapToMoveGameplayLayerToZ0":
                    return true;
            }

            return false;
        }
    }
}
