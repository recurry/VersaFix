//************************************************************************
// The VersaFix FIX Engine. 
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VfxEngine.Sequencer
{
    /// <summary>
    /// The VfxSequencer class provides an implementation of
    /// an ordered task queue for asynchronously processing
    /// tasks using the system thread pool.
    /// </summary>
    public sealed class VfxSequencer
    {
        /// <summary>
        /// The SequencedTask class is an internal class that
        /// encapsulates the details of an enqueued task.
        /// </summary>
        private class SequencedTask
        {
            /// <summary>
            /// The state information that was provided when
            /// the task was added to the sequencer's queue.
            /// </summary>
            private object _state;

            /// <summary>
            /// The callback method that was provided when the
            /// task was added to the sequencer's queue.
            /// </summary>
            private ContextCallback _callback;

            /// <summary>
            /// The execution context of the thread that the
            /// task was added from.
            /// </summary>
            private ExecutionContext _context;

            public SequencedTask(ContextCallback callback, object state, ExecutionContext context)
            {
                _state = state;
                _callback = callback;
                _context = context;

            }

            public void Execute()
            {
                ExecutionContext.Run(_context, _callback, _state);
            }
        }

        /// <summary>
        /// The task queue maintains a FIFO queue of the tasks
        /// which are to be processed by the thread pool.
        /// </summary>
        private Queue<SequencedTask> _taskQueue = new Queue<SequencedTask>();

        /// <summary>
        /// The pending flag indicates whether or not a system
        /// thread pool thread has been queue to process tasks
        /// that are currently in the queue.
        /// </summary>
        private bool _pending = false;

        /// <summary>
        /// The Enqueue method is invoked to add a callback action
        /// to the sequencer's queue of operations.
        /// </summary>
        /// <param name="callback">
        /// The callback that is to be invoked.
        /// </param>
        /// <param name="state">
        /// The state information to be passed to the callback
        /// method when it is executed.
        /// </param>
        public void Enqueue(ContextCallback callback, object state)
        {
            SequencedTask task = new SequencedTask(callback, state, ExecutionContext.Capture());
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(task);
                if (_pending == false)
                {
                    _pending = true;
                    ThreadPool.UnsafeQueueUserWorkItem(Execute, null);
                }
            }
        }

        /// <summary>
        /// The Execute method is the entrypoint for the system
        /// thread pool to execute tasks on the task queue.
        /// </summary>
        /// <param name="ignored">
        /// The ignored parameter is provided in order for the
        /// method to match the WaitCallback signature which is
        /// required by the .NET QueueUserWorkItem method.
        /// </param>
        private void Execute(object ignored)
        {
            while (true)
            {
                SequencedTask task = null;
                lock (_taskQueue)
                {
                    if (_taskQueue.Count == 0)
                    {
                        _pending = false;
                        break;
                    }
                    task = _taskQueue.Dequeue();
                }

                try
                {
                    task.Execute();
                }
                catch(System.Exception)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(Execute, null);
                    throw;
                }
            }
        }
    }
}
