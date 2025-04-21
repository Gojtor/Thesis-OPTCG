using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TCGSim
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> executionQueue = new Queue<Action>();
        public static bool isProcessing = false;

        public static UnityMainThreadDispatcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        public static void Enqueue(Action action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            isProcessing = true;
            while (executionQueue.Count > 0)
            {
                Action action;
                lock (executionQueue)
                {
                    action = executionQueue.Dequeue();
                }
                action?.Invoke();
            }
            isProcessing = false;
        }
        public static Task RunOnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                action();
                tcs.SetResult(true);
            });
            return tcs.Task;
        }
    }
}
