using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Forms.ugh;
using ff14bot.Helpers;
using ff14bot.Managers;

namespace LlamaLibrary
{
    public class LisbethVentures : BotPlugin
    {
        private static readonly string name = "Lisbeth Ventures";
        public override string Author { get; } = "Kayla";

        public override Version Version => new Version(2, 6);

        public override string Name { get; } = name;

        private static Func<Task> VentureTask;
        private static Action<string, Func<Task>> _addHook;
        private static Action<string> _removeHook;
        private static Func<List<string>> _getHookList;
        private static bool FoundLisbeth = false;
        private static bool FoundLL = false;
        private static string HookName = "Retainers";
        
		public override bool WantButton
        {
            get { return false; }
        }

        public override string ButtonText
        {
            get { return "Toggle"; }
        }

        public override void OnButtonPress()
        {
            FindLL();
        }

        public override void OnInitialize()
        {
            
            FindLL();
        }

        public override void OnEnabled()
        {
			TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
			Log($"{name} Enabled");
        }
		
		public override void OnDisabled()
        {
			TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
			Log($"{name} Disabled");
        }
        
        private void OnBotStop(BotBase bot)
        {
            if (bot.Name == "Lisbeth")
            {
                if (!FoundLisbeth) FindLisbeth();
                if (FoundLisbeth && FoundLL)
                    RemoveHooks();
            }
        }

        private void OnBotStart(BotBase bot)
        {
            if (bot.Name == "Lisbeth")
            {
                if (!FoundLisbeth) FindLisbeth();
                if (FoundLisbeth && FoundLL)
                    AddHooks();
            }
        }
        
        private void AddHooks()
        {
            var hooks = _getHookList.Invoke();
            Log($"Adding {HookName} Hook");
            if (!hooks.Contains(HookName))
            {
                _addHook.Invoke(HookName, VentureTask);
            }
        }

        private void RemoveHooks()
        {
            var hooks = _getHookList.Invoke();
            Log($"Removing {HookName} Hook");
            if (hooks.Contains(HookName))
            {
                _removeHook.Invoke(HookName);
            }
        }

        private void FindLL()
        {
            var loader = BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Retainers");

            if (loader == null) return;
            
            var CheckVentureTask = loader?.GetType().GetMethod("CheckVentureTask");
            VentureTask = (Func<Task>) CheckVentureTask?.CreateDelegate(typeof(Func<Task>));

            //Log(VentureTask.Method.Name);
            FoundLL = true;
        }
        
        internal static void FindLisbeth()
        {
            var loader = BotManager.Bots
                .FirstOrDefault(c => c.Name == "Lisbeth");

            if (loader == null) return;

            var lisbethObjectProperty = loader.GetType().GetProperty("Lisbeth");
            var lisbeth = lisbethObjectProperty?.GetValue(loader);
            if (lisbeth == null) return;
            var apiObject = lisbeth.GetType().GetProperty("Api")?.GetValue(lisbeth);
            if (apiObject != null)
            {
                var m = apiObject.GetType().GetMethod("GetCurrentAreaName");
                if (m != null)
                {
                    _addHook = (Action<string, Func<Task>>) Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddHook");
                    _removeHook = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveHook");
                    _getHookList = (Func<List<string>>) Delegate.CreateDelegate(typeof(Func<List<string>>), apiObject, "GetHookList");
                    FoundLisbeth = true;
                }
            }
            Logging.Write("Lisbeth found.");
        }

        private static void Log(string text)
        {
            var msg = string.Format($"[{name}] " + text);
            Logging.Write(Colors.Aquamarine, msg);
        }

        /// <summary>
        ///     Blocks while condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
                throw new TimeoutException();
        }

        /// <summary>
        ///     Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask,
                                               Task.Delay(timeout)))
                throw new TimeoutException();
        }
    }
}