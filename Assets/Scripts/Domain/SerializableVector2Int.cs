using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [Serializable]
    public struct SerializableVector2Int
    {
        public int x;
        public int y;

        public SerializableVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public SerializableVector2Int(Vector2Int value)
        {
            x = value.x;
            y = value.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }
}

