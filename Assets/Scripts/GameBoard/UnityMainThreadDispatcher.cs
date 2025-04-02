using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCGSim
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> executionQueue = new Queue<Action>();

        public static void Enqueue(Action action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            while (executionQueue.Count > 0)
            {
                Action action;
                lock (executionQueue)
                {
                    action = executionQueue.Dequeue();
                }
                action?.Invoke();
            }
        }
    }
}
