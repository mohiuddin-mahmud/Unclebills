﻿using System;
using Autofac;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Infrastructure;
using Nop.Services.Logging;

namespace Nop.Services.Tasks
{
    /// <summary>
    /// Task
    /// </summary>
    public partial class Task
    {
        #region Ctor

        /// <summary>
        /// Ctor for Task
        /// </summary>
        private Task()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Ctor for Task
        /// </summary>
        /// <param name="task">Task </param>
        public Task(ScheduleTask task)
        {
            this.Type = task.Type;
            this.Enabled = task.Enabled;
            this.StopOnError = task.StopOnError;
            this.Name = task.Name;
            this.LastSuccessUtc = task.LastSuccessUtc;
            this.DefaultStartTimeUtc = task.DefaultStartTimeUtc;
        }

        #endregion

        #region Utilities

        private ITask CreateTask(ILifetimeScope scope)
        {
            ITask task = null;
            if (this.Enabled)
            {
                var type2 = System.Type.GetType(Type);
                if (type2 != null)
                {
                    object instance;
                    if (!EngineContext.Current.ContainerManager.TryResolve(type2, scope, out instance))
                    {
                        //not resolved
                        instance = EngineContext.Current.ContainerManager.ResolveUnregistered(type2, scope);
                    }
                    task = instance as ITask;
                }
            }
            return task;
        }

        private bool ValidStartTime(ScheduleTask scheduleTask)
        {
            // Default nopCommerce functionality doesn't allow for exact run time on jobs that need to run every night
            // This method ensures custom nightly tasks do not run outside of their scheduled times
            if (scheduleTask.DefaultStartTimeUtc != null)
            {
                /**** CHANGE - Now using DefaultStartTimeUtc field as the NEXT scheduled run date/time ****/
                // if CURRENT time is past the NEXT RUN time AND the LAST_START was before the NEXT scheduled time
                if(scheduleTask.DefaultStartTimeUtc < DateTime.UtcNow &&
                    scheduleTask.DefaultStartTimeUtc > scheduleTask.LastStartUtc)
                {
                    return true;
                }
                else { return false; }
            }
            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="throwException">A value indicating whether exception should be thrown if some error happens</param>
        /// <param name="dispose">A value indicating whether all instances should be disposed after task run</param>
        /// <param name="ensureRunOnOneWebFarmInstance">A value indicating whether we should ensure this task is run on one farm node at a time</param>
        public void Execute(bool throwException = false, bool dispose = true, bool ensureRunOnOneWebFarmInstance = true)
        {
            //background tasks has an issue with Autofac
            //because scope is generated each time it's requested
            //that's why we get one single scope here
            //this way we can also dispose resources once a task is completed
            var scope = EngineContext.Current.ContainerManager.Scope();
            var scheduleTaskService = EngineContext.Current.ContainerManager.Resolve<IScheduleTaskService>("", scope);
            var scheduleTask = scheduleTaskService.GetTaskByType(this.Type);

            if (ValidStartTime(scheduleTask))
            {
                try
                {
                    //flag that task is already executed
                    var taskExecuted = false;

                    //task is run on one farm node at a time?
                    if (ensureRunOnOneWebFarmInstance)
                    {
                        //is web farm enabled (multiple instances)?
                        var nopConfig = EngineContext.Current.ContainerManager.Resolve<NopConfig>("", scope);
                        if (nopConfig.MultipleInstancesEnabled)
                        {
                            var machineNameProvider = EngineContext.Current.ContainerManager.Resolve<IMachineNameProvider>("", scope);
                            var machineName = machineNameProvider.GetMachineName();
                            if (String.IsNullOrEmpty(machineName))
                            {
                                throw new Exception("Machine name cannot be detected. You cannot run in web farm.");
                                //actually in this case we can generate some unique string (e.g. Guid) and store it in some "static" (!!!) variable
                                //then it can be used as a machine name
                            }

                            if (scheduleTask != null)
                            {
                                if (nopConfig.RedisCachingEnabled)
                                {
                                    //get expiration time
                                    var expirationInSeconds = scheduleTask.Seconds <= 300 ? scheduleTask.Seconds - 1 : 300;

                                    var executeTaskAction = new Action(() =>
                                    {
                                        //execute task
                                        taskExecuted = true;
                                        var task = this.CreateTask(scope);
                                        if (task != null)
                                        {
                                            //update appropriate datetime properties
                                            scheduleTask.LastStartUtc = DateTime.UtcNow;
                                            scheduleTaskService.UpdateTask(scheduleTask);
                                            task.Execute();
                                            this.LastEndUtc = this.LastSuccessUtc = DateTime.UtcNow;
                                        }
                                    });

                                    //execute task with lock
                                    var redisWrapper = EngineContext.Current.ContainerManager.Resolve<IRedisConnectionWrapper>(scope: scope);
                                    if (!redisWrapper.PerformActionWithLock(scheduleTask.Type, TimeSpan.FromSeconds(expirationInSeconds), executeTaskAction))
                                        return;
                                }
                                else
                                {
                                    //lease can't be acquired only if for a different machine and it has not expired
                                    if (scheduleTask.LeasedUntilUtc.HasValue &&
                                        scheduleTask.LeasedUntilUtc.Value >= DateTime.UtcNow &&
                                        scheduleTask.LeasedByMachineName != machineName)
                                        return;

                                    //lease the task. so it's run on one farm node at a time
                                    scheduleTask.LeasedByMachineName = machineName;
                                    scheduleTask.LeasedUntilUtc = DateTime.UtcNow.AddMinutes(30);
                                    scheduleTaskService.UpdateTask(scheduleTask);
                                }
                            }
                        }
                    }

                    //execute task in case if is not executed yet
                    if (!taskExecuted)
                    {
                        //initialize and execute
                        var task = this.CreateTask(scope);
                        if (task != null)
                        {
                            this.LastStartUtc = DateTime.UtcNow;
                            if (scheduleTask != null)
                            {
                                //update appropriate datetime properties
                                scheduleTask.LastStartUtc = LastStartUtc;
                                scheduleTaskService.UpdateTask(scheduleTask);
                            }
                            task.Execute();
                            this.LastEndUtc = this.LastSuccessUtc = DateTime.UtcNow;
                        }
                    }
                }
                catch (Exception exc)
                {
                    this.Enabled = !this.StopOnError;
                    this.LastEndUtc = DateTime.UtcNow;

                    //log error
                    var logger = EngineContext.Current.ContainerManager.Resolve<ILogger>("", scope);
                    logger.Error(string.Format("Error while running the '{0}' schedule task. {1}", this.Name, exc.Message), exc);
                    if (throwException)
                        throw;
                }

                if (scheduleTask != null)
                {
                    //update appropriate datetime properties
                    scheduleTask.LastEndUtc = this.LastEndUtc;
                    scheduleTask.LastSuccessUtc = this.LastSuccessUtc;
					//if (scheduleTask.DefaultStartTimeUtc.HasValue) scheduleTask.DefaultStartTimeUtc = scheduleTask.DefaultStartTimeUtc.Value.AddHours(24);
                    if (scheduleTask.DefaultStartTimeUtc.HasValue)
                    {
                        if (scheduleTask.Name == "Rewards Certificate Email Task")
                        {
                            //rewards certificate email task needs to run 14 days out
                            scheduleTask.DefaultStartTimeUtc = scheduleTask.DefaultStartTimeUtc.Value.AddDays(14);
                        }
                        else
                        {
                            scheduleTask.DefaultStartTimeUtc = scheduleTask.DefaultStartTimeUtc.Value.AddHours(24);
                        }
                    }
                    scheduleTaskService.UpdateTask(scheduleTask);
                }

                //dispose all resources
                if (dispose)
                {
                    scope.Dispose();
                }
            }
            
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Datetime of the last start
        /// </summary>
        public DateTime? LastStartUtc { get; private set; }

        /// <summary>
        /// Datetime of the last end
        /// </summary>
        public DateTime? LastEndUtc { get; private set; }

        /// <summary>
        /// Datetime of the last success
        /// </summary>
        public DateTime? LastSuccessUtc { get; private set; }

        /// <summary>
        /// A value indicating type of the task
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// A value indicating whether to stop task on error
        /// </summary>
        public bool StopOnError { get; private set; }

        /// <summary>
        /// Get the task name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A value indicating whether the task is enabled
        /// </summary>
        public bool Enabled { get; set; }

        public DateTime? DefaultStartTimeUtc { get; set;  }

        #endregion
    }
}
