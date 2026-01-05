using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Utils
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> queue = new();

        private void Update()
        {
            while (queue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        public static void Enqueue(Action action)
        {
            if (action == null)
            {
                return;
            }

            queue.Enqueue(action);
        }
    }
}