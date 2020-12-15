using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [System.Serializable]
    public class Attribute
    {
        [System.NonSerialized]
        public PlayerManager parent;
        public Attributes type;
        public ModifiableInt value;
        public void SetParent(PlayerManager _player)
        {
            parent = _player;
            value = new ModifiableInt(AttributeModified);
        }

        public void AttributeModified()
        {
            parent.AttribureModified(this);
        }
    }

}
