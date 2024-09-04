using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MakeMeSmall.Demo2_FileConverter
{
    public class FilterToggleHandler : MonoBehaviour
    {

        public bool CheckOnStart = false;

        void Start()
        {
            if (CheckOnStart)
            {
                OnValueChanged(true);
            }
        }

        public void OnValueChanged(bool newValue)
        {
            Demo2_Main.OnFilterToggleValueChanged(gameObject, newValue);
        }
    }
}