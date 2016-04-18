using UnityEngine;
using System.Collections;

namespace GardenSystem {
    
    public partial class Plant : MonoBehaviour {
        public int typeId;

        void Awake() {
            Init ();
        }

        partial void Init();
    }
}
