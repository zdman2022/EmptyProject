using FlatRedBall;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.IO;
using FlatRedBall.TileGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager.Converters
{
    internal class FileLoader
    {
        public static object LoadFile(FilePath filePath, string contentManagerName, string typeName = null)
        {
            var extension = filePath.Extension;
            if (typeName == typeof(FlatRedBall.Graphics.Animation.AnimationChainList).FullName || extension == "achx")
            {
                return FlatRedBallServices.Load<AnimationChainList>(filePath.FullPath);
            }
            else if (typeName == typeof(Microsoft.Xna.Framework.Graphics.Texture2D).FullName || extension == "png")
            {
                return FlatRedBallServices.Load<Microsoft.Xna.Framework.Graphics.Texture2D>(filePath.FullPath);
            }
            else if(typeName == typeof(MapDrawableBatch).FullName || extension == "tmx")
            {
                return FlatRedBall.TileGraphics.LayeredTileMap.FromTiledMapSave(filePath.FullPath, contentManagerName);
            }

            return null;
        }
    }
}
