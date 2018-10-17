using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace MULTIFORM_PCS.ControlModules.SchedulingModule
{
    public class TAOptModule : SchedulingModule
    {
        private Thread taoptThread;
        private bool running = false;
        public bool getRunning()
        {
            return running;
        }

        #region singletonPattern;
        private static TAOptModule taoptCaller;
        public static TAOptModule getInstance()
        {
            if (taoptCaller == null)
            {
                taoptCaller = new TAOptModule();
            }
            return taoptCaller;
        }
        private TAOptModule()
        {
        }
        #endregion;

        public override void scheduleRecipes()
        {
            if (!running)
            {
                running = true;
                taoptThread = new Thread(new ThreadStart(runInThread));
                taoptThread.Start();
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("!!! WARNING: Starting of TAOpt 4.3 not possible. TAOpt 4.3 is already running. !!!");
            }

        }

        private void runInThread()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (FileInfo f in dirInfo.EnumerateFiles())
            {
                if (f.Extension == ".log")
                {
                    if (f.Name != "PCS.log")
                    {
                        f.Delete();
                    }
                }
            }

            ControlModules.SchedulingModule.TAOptModel.TAOptModelBuilder.getInstance().buildModel(Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData, System.IO.Directory.GetCurrentDirectory());

            string unsafeSettings = "-n 500000 -w Mdmc -s AF0L -g 1 -p 2 -b 4 -B 3 -O 0 -c 0 -t 0 -L 2";
            string commandline_arguments_TAOpt4 = unsafeSettings + " plant.trtn recipe_1.trtn recipe_2.trtn recipe_3.trtn recipe_4.trtn recipe_5.trtn recipe_6.trtn orders.trtn";

            string path_TAOpt4_executeable = AppDomain.CurrentDomain.BaseDirectory + "TAOptBin\\TAOpt4.3.exe";

            System.Diagnostics.ProcessStartInfo taoptStartInfo = new System.Diagnostics.ProcessStartInfo(@path_TAOpt4_executeable);
            taoptStartInfo.RedirectStandardOutput = true;
            taoptStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            taoptStartInfo.UseShellExecute = false;
            taoptStartInfo.Arguments = commandline_arguments_TAOpt4;
            taoptStartInfo.CreateNoWindow = true;

            System.Diagnostics.Process taoptRun;
            taoptRun = System.Diagnostics.Process.Start(taoptStartInfo);


            while (!taoptRun.HasExited)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage(taoptRun.StandardOutput.ReadLine());
              //NOOP
            }

            dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (FileInfo f in dirInfo.EnumerateFiles())
            {
                if (f.Extension == ".log")
                {
                    if (f.Name != "PCS.log" && f.Name != "schedules.log" && f.Name != "statistics.log" && f.Name != "maps.log" && f.Name != "runs.log" && f.Name != "sequence_mapping.log")
                    {
                         f.Delete();
                    }
                }
            }

            Gateway.CTRLModule.getInstance().CurrentSchedule = new Datastructure.Schedule.Schedule(Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData, AppDomain.CurrentDomain.BaseDirectory + "schedules.log", AppDomain.CurrentDomain.BaseDirectory + "maps.log", AppDomain.CurrentDomain.BaseDirectory + "runs.log", AppDomain.CurrentDomain.BaseDirectory + "sequence_mapping.log");
            Gateway.CTRLModule.getInstance().DetailedPlan = new Datastructure.Schedule.DetailedProductionPlan(Gateway.CTRLModule.getInstance().CurrentSchedule);
            //GUI.PCSMainWindow.getInstance().drawGanttChart(Gateway.CTRLModule.getInstance().CurrentSchedule);
            GUI.PCSMainWindow.getInstance().drawGanttChart(Gateway.CTRLModule.getInstance().DetailedPlan, Gateway.CTRLModule.getInstance().CurrentSchedule);

            running = false;

            GUI.PCSMainWindow.getInstance().unmarkRunningTAOpt();
            GUI.PCSMainWindow.getInstance().postStatusMessage("Scheduling using TAOpt 4.3 done!");
        }
    }
}
