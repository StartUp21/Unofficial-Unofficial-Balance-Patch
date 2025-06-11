using System;
using System.Collections.Generic;
using System.Text;

namespace Unofficial_Unofficial_Balance_Patch.RoomStateModifiers
{
    internal class RoomStateCapacityModifier : RoomStateModifierBase, IRoomStateCapacityModifier
    {
        private int capacity;
        public override void Initialize(RoomModifierData roomModifierData, ICoreGameManagers coreGameManagers)
        {
            base.Initialize(roomModifierData, coreGameManagers);
            capacity = roomModifierData.GetParamInt();
        }
        public int GetModifiedCapacity()
        {
            return this.capacity;
        }
    }
}
