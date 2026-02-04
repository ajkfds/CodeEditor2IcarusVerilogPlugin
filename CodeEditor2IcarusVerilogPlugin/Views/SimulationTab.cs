using CodeEditor2.Data;
using CodeEditor2.Shells;
using pluginVerilog.Verilog.BuildingBlocks;
using pluginVerilog.Verilog.DataObjects;
using pluginVerilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Media;
using Avalonia.Controls;
using CodeEditor2.Views;
using System.Threading;
using SkiaSharp;

namespace pluginIcarusVerilog.Views
{
    internal class SimulationTab : CodeEditor2.Views.CodeTabItem
    {
        protected SimulationTab(string title, string ? iconName, Avalonia.Media.Color ? iconColor, bool closeButtonEnable) :base(title,iconName,iconColor,closeButtonEnable)
        {
            SimPanel = new SimPanel();
            Content = SimPanel;
        }

        private const string prompt = "icarusVerilogShell";
        public static SimulationTab? Create()
        {
            CodeEditor2.Data.File? file;
            file = CodeEditor2.Controller.NavigatePanel.GetSelectedFile();

            pluginVerilog.Data.VerilogFile? vFile = file as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return null;

            pluginVerilog.Data.SimulationSetup? simulationSetup = pluginVerilog.Data.SimulationSetup.Create(vFile);
            if (simulationSetup == null) return null;

            SimulationTab tab = new SimulationTab(simulationSetup.TopName,"play",Plugin.ThemeColor,true);
            tab.SimulationSetup = simulationSetup;

            tab.CloseButton_Clicked += new Action(() => { tab.Close(); });

            return tab;
        }

        public SimPanel SimPanel;
        protected pluginVerilog.Data.SimulationSetup? SimulationSetup;
        protected CodeEditor2.Shells.WinCmdShell shell;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private void Close()
        {
            tokenSource.Cancel();
            CodeEditor2.Controller.Tabs.RemoveItem(this);
            tokenSource.Dispose();
        }

        public void Run()
        {
            var _ = work(tokenSource.Token);
        }

        private async Task work(CancellationToken token)
        {
            if (SimulationSetup == null) throw new Exception();

            string simName = SimulationSetup.TopName;


            string simulationPath = Setup.SimulationPath + "\\" + simName;
            simulationPath = @"c:\temp\" + simName;

            if (!System.IO.Directory.Exists(simulationPath))
            {
                System.IO.Directory.CreateDirectory(simulationPath);
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(simulationPath + "\\command"))
            {
                foreach (string includePath in SimulationSetup.IncludePaths)
                {
                    sw.WriteLine("+incdir+" + includePath); // path with space is not accepted
                }
                foreach (CodeEditor2.Data.File file in SimulationSetup.Files)
                {
                    sw.WriteLine( SimulationSetup.Project.GetAbsolutePath(file.RelativePath));
                }
            }

            shell = new CodeEditor2.Shells.WinCmdShell(new List<string> {
                "prompt "+prompt+"$G$_",
                simulationPath.Substring(0,2),
                "cd "+simulationPath
            });

            shell.LineReceived += Shell_LineReceived;
            shell.Start();

            await Task.Delay(1, token);
            while (shell.GetLastLine() != prompt+">")
            {
                await Task.Delay(10, token);
                if (token.IsCancellationRequested) return;
            }
            shell.ClearLogs();
            shell.StartLogging();
            await Task.Delay(1, token);
            shell.Execute("dir");
            await Task.Delay(1, token);
            shell.Execute("type command");
            await Task.Delay(1, token);
            shell.Execute(Setup.BinPath + "iverilog -g2012 -v -Wall -s "+simName+" -f command -o "+simName+".o");
            await Task.Delay(1, token);
            while (shell.GetLastLine() != prompt+">")
            {
                await Task.Delay(10, token);
                if (token.IsCancellationRequested) return;
            }
            //List<string> logs = shell.GetLogs();
            //if(logs.Count != 3 || logs[1] !="")
            //{
            //    return;
            //}
            //shell.EndLogging();
            shell.Execute(Setup.BinPath + "vvp " + simName + ".o");
        }
        private void Shell_LineReceived(string lineString)
        {
            if(lineString == prompt + ">")
            {
                SimPanel.LineReceived(lineString, Colors.Green);
            }
            else
            {
                SimPanel.LineReceived(lineString,null);
            }
        }
    }
}
