using System;
using System.Collections.Generic;
using UnityEngine;

namespace var_browser
{
    class Messager : MonoBehaviour
    {
        public GameObject target;

        void Invoke(string msg)
        {
            string[] splits = msg.Split(',');
            if (splits.Length == 1)
            {
                target.SendMessage(splits[0]);
            }
            else
            {
                string p = splits[1];
                if (splits.Length > 2)
                {
                    for (int i = 2; i < splits.Length; i++)
                    {
                        p += "," + splits[i];
                    }
                }
                target.SendMessage(splits[0], p);
            }
        }

    }
}
