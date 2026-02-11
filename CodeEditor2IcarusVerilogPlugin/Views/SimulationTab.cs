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
        public static SimulationTab? Create(CodeEditor2.Tests.ITest simulation)
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
            tab.Simulation = simulation;

            return tab;
        }

        private CodeEditor2.Tests.ITest Simulation;
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
            Simulation.LogReceived += LogReceived;
            await Simulation.RunSimulationAsync(token);
        }
        private void LogReceived(string lineString,Avalonia.Media.Color? color)
        {
            SimPanel.LineReceived(lineString, color);
        }
    }
}
