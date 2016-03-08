using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Xperitos.Common.Utils
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Creates a cold observable that runs the given command and returns its stdout
        /// </summary>
        /// <param name="fileName">command to run</param>
        /// <param name="args">arguments for the command</param>
        /// <returns>Connectable observable - runs the command upon connection and returns each output line from the command</returns>
        public static IConnectableObservable<string> ObserveCommand(string fileName, string args = null)
        {
            return Observable.Create<string>(
                (observer, ct) =>
                {
                    Log.Logger.Debug("Running command {exeFile} with {args}", fileName, args);

                    var process = new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = false,
                            FileName = fileName,
                            Arguments = args,
                            WindowStyle = ProcessWindowStyle.Hidden,

                            // Redirect the output stream of the child process.
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };

                    var registration = ct.Register(() =>
                    {
                        if (process != null && !process.HasExited)
                            process.Kill();
                    });

                    return Task.Factory.StartNew(() =>
                    {
                        process.Start();
                        bool hadError = false;

                        // Do not wait for the child process to exit before
                        // reading to the end of its redirected stream.
                        // p.WaitForExit();
                        // Read the output stream first and then wait.
                        try
                        {
                            while (!process.StandardOutput.EndOfStream)
                                observer.OnNext(process.StandardOutput.ReadLine());
                            process.WaitForExit();
                        }
                        catch (Exception e)
                        {
                            observer.OnError(e);
                            hadError = true;
                        }

                        Log.Logger.Debug("Command {exeFile} with {args} terminated", fileName, args);

                        registration.Dispose();
                        process.Dispose();
                        process = null;

                        if (!hadError)
                            observer.OnCompleted();

                    }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }).Publish();
        }

        /// <summary>
        /// Run process and return the exit code (or null if cancelled before started running).
        /// </summary>
        /// <param name="fileName">Process to run</param>
        /// <param name="args">Arguments for the process</param>
        /// <param name="suppressConsoleOutput">When set to true, standard error and output are suppressed</param>
        /// <param name="ct">Cancellation token. When cancelled, process will be killed!</param>
        /// <returns>Task with process exit code</returns>
        public static Task<int?> RunProcessAsync(string fileName, string args = null, bool suppressConsoleOutput = false, CancellationToken ct = default(CancellationToken))
        {
            return RunProcessInternalAsync(fileName, args, suppressConsoleOutput, ct);
        }

        /// <summary>
        /// Run process and return the exit code (or null if cancelled before started running).
        /// </summary>
        /// <param name="fileName">Process to run</param>
        /// <param name="args">Arguments for the process</param>
        /// <param name="ct">Cancellation token. When cancelled, process will be killed!</param>
        /// <returns>Task with process exit code</returns>
        public static Task<int?> RunProcessAsync(string fileName, string args = null, CancellationToken ct = default(CancellationToken))
        {
            return RunProcessInternalAsync(fileName, args, false, ct);
        }

        /// <summary>
        /// Run process and return the exit code (or null if cancelled before started running).
        /// </summary>
        /// <param name="fileName">Process to run</param>
        /// <param name="args">Arguments for the process</param>
        /// <param name="suppressConsoleOutput">When set to true, standard error and output are suppressed</param>
        /// <param name="ct">Cancellation token. When cancelled, process will be killed!</param>
        /// <returns>Task with process exit code</returns>
        private static Task<int?> RunProcessInternalAsync(string fileName, string args, bool suppressConsoleOutput, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromResult<int?>(null);

            var tcs = new TaskCompletionSource<int?>();

            Log.Logger.Debug("Running command {exeFile} with {args}", fileName, args);

            var psi = new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = fileName,
                Arguments = args ?? "",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            if (suppressConsoleOutput)
            {
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
            }

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            var registration = ct.Register(process.Kill);

            process.Exited += (sender, e) =>
            {
                Log.Logger.Debug("Command {exeFile} with {args} terminated", fileName, args);
                tcs.SetResult(process.ExitCode);
                registration.Dispose();
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
