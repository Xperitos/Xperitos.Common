using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ReactiveUI;

namespace Xperitos.Common.Utils
{
    public static class ReactiveCommandEx
    {
        /// <summary>
        /// Create a "synchronous command"
        /// </summary>
        /// <returns>The new command</returns>
        public static ReactiveCommand<Unit> CreateCommandSync(IObservable<bool> canExecute, Action action, IScheduler scheduler = null)
        {
            return CreateCommandSync(action, canExecute, scheduler);
        }

        /// <summary>
        /// Create a "synchronous command"
        /// </summary>
        /// <returns>The new command</returns>
        public static ReactiveCommand<Unit> CreateCommandSync(Action action, IObservable<bool> canExecute = null, IScheduler scheduler = null)
        {
            canExecute = canExecute ?? Observable.Return(true);

            var cmd = new ReactiveCommand<Unit>(canExecute, x => Observable.Return(Unit.Default), scheduler);
            cmd.Subscribe(x => action());

            return cmd;
        }

        /// <summary>
        /// Create a "synchronous command"
        /// </summary>
        /// <returns>The new command</returns>
        public static ReactiveCommand<TArg> CreateCommandSync<TArg>(IObservable<bool> canExecute, Action<TArg> action, IScheduler scheduler = null)
        {
            return CreateCommandSync(action, canExecute, scheduler);
        }

        /// <summary>
        /// Create a "synchronous command"
        /// </summary>
        /// <returns>The new command</returns>
        public static ReactiveCommand<TArg> CreateCommandSync<TArg>(Action<TArg> action, IObservable<bool> canExecute = null, IScheduler scheduler = null) 
        {
            canExecute = canExecute ?? Observable.Return(true);

            var cmd = new ReactiveCommand<TArg>(canExecute, x => Observable.Return((TArg)x), scheduler);
            cmd.Subscribe(action);

            return cmd;
        }

        public static ReactiveCommand<Unit> CreateCommandAsync(Func<Task> actionAsync, IScheduler scheduler)
        {
            return CreateCommandAsync(actionAsync, null, scheduler);
        }

        public static ReactiveCommand<Unit> CreateCommandAsync(Func<Task> actionAsync, IObservable<bool> canExecute = null, IScheduler scheduler = null)
        {
            return new ReactiveCommand<Unit>(canExecute ?? Observable.Return(true), x => actionAsync().ToObservable(), scheduler);
        }

        public static ReactiveCommand<Unit> CreateCommandAsync<TArg>(Func<TArg, Task> actionAsync, IScheduler scheduler)
        {
            return CreateCommandAsync(actionAsync, null, scheduler);
        }

        public static ReactiveCommand<Unit> CreateCommandAsync<TArg>(Func<TArg, Task> actionAsync, IObservable<bool> canExecute = null, IScheduler scheduler = null)
        {
            return new ReactiveCommand<Unit>(canExecute ?? Observable.Return(true), x => actionAsync((TArg)x).ToObservable(), scheduler);
        }
    }
}
