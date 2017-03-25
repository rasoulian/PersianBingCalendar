﻿using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;
using PersianBingCalendar.Utils;

namespace PersianBingCalendar.Core
{
    public static class TaskRunner
    {
        public static void CreateOrUpdateTask()
        {
            const string description = "PersianBingCalendar Runner";
            var execAction = new ExecAction(Application.ExecutablePath, null, Application.StartupPath);
            using (var task = TaskService.Instance.NewTask())
            {
                task.RegistrationInfo.Description = description;
                task.RegistrationInfo.Author = "DNT";
                task.Settings.Hidden = true; // Project -> Properties -> Application tab -> Output type -> `Windows Application`
                task.Settings.Enabled = true;
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.StopIfGoingOnBatteries = false;
                task.Settings.StartWhenAvailable = true;
                task.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

                task.Triggers.Add(createHourlyTrigger());
                task.Triggers.Add(createDailyStartTrigger());

                task.Actions.Add(execAction);
                TaskService.Instance.RootFolder.RegisterTaskDefinition(
                    description,
                    task,
                    TaskCreation.CreateOrUpdate,
                    null,
                    null,
                    TaskLogonType.InteractiveToken,
                    null);
            }
            Console.WriteLine($"`{description}` task has been added.");
        }

        public static void RunTask()
        {
            File.WriteAllText(Path.Combine(DirUtils.GetAppPath(), "last-run.log"), string.Format("{0}{1}{1}", DateTime.Now, Environment.NewLine));
            BingImagesDownloader.DownloadTodayBingImages();
            BingWallpaper.SetTodayWallpapaer();
        }

        private static TimeTrigger createHourlyTrigger()
        {
            return new TimeTrigger
            {
                StartBoundary = new DateTime(2017, 1, 1),
                Repetition =
                {
                    Interval = TimeSpan.FromHours(1),
                    StopAtDurationEnd = false
                },
                Enabled = true
            };
        }

        private static TimeTrigger createDailyStartTrigger()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            var startOfADay = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 1);
            return new TimeTrigger(startOfADay);
        }
    }
}