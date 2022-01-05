﻿namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// this class is used to await async events
    /// </summary>
    public static class EventsAwaiter
    {
        /// <summary>
        /// Invoice the EventHandler Asynchronously
        /// </summary>
        /// <typeparam name="TEventArgs">the Type of the event arguments</typeparam>
        /// <param name="handler">the event handler</param>
        /// <param name="sender">the sender</param>
        /// <param name="args">the event argument</param>
        /// <returns>await-bale task</returns>
        public static async Task InvokeAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler, object sender, TEventArgs args)
            where TEventArgs : EventArgs
        {
            if (handler is null)
                return;

            var invocationList = handler.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
                handlerTasks[i] = ((AsyncEventHandler<TEventArgs>)invocationList[i])(sender, args);

            await Task.WhenAll(handlerTasks);
        }
    }

    /// <summary>
    /// Represents an Async method that will handle an event when the event provides data.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An object that contains the event data.</param>
    /// <returns>the await-able task</returns>
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e)
        where TEventArgs : EventArgs;
}
