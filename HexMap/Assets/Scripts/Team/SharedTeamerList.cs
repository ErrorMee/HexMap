using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedTeamerList : SharedVariable<List<Teamer>>
    {
        public static implicit operator SharedTeamerList(List<Teamer> value) { return new SharedTeamerList { mValue = value }; }
    }
}