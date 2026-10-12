using System.IO;
using UnityEngine;

namespace Assets.Scripts.Log
{
    internal class ExperimentLogger : MonoBehaviour
    {
        public static ExperimentLogger Instance { get; private set; }

        private StreamWriter _streamWriter;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
