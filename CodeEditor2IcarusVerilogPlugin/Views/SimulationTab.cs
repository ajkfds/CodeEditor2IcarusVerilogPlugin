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
        protected SimulationTab(string title, string ? iconName, Avalonia.Media.Color ? iconColor, bool closeButtonEnable, CodeEditor2.Tests.ITest simulation) :base(title,iconName,iconColor,closeButtonEnable)
        {
            SimPanel = new SimPanel();
            Content = SimPanel;
            Simulation = simulation;
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
            return createTab(simulation, simulationSetup);
        }

        // Async creation: the SimulationSetup creation (hierarchy walk /
        // class dependency resolution) is heavy and must not run on the UI
        // thread. Running it on the UI thread freezes the whole application
        // (and blocks background threads that synchronously Invoke to the UI
        // thread, resulting in a deadlock-like freeze).
        public static async Task<SimulationTab?> CreateAsync(CodeEditor2.Tests.ITest simulation)
        {
            CodeEditor2.Data.File? file;
            file = CodeEditor2.Controller.NavigatePanel.GetSelectedFile();

            pluginVerilog.Data.VerilogFile? vFile = file as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return null;

            pluginVerilog.Data.SimulationSetup? simulationSetup =
                await System.Threading.Tasks.Task.Run(
                    async () => await pluginVerilog.Data.SimulationSetup.CreateAsync(vFile)
                    );
            if (simulationSetup == null) return null;

            return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => createTab(simulation, simulationSetup));
        }

        private static SimulationTab? createTab(CodeEditor2.Tests.ITest simulation, pluginVerilog.Data.SimulationSetup simulationSetup)
        {

            SimulationTab tab = new SimulationTab(simulationSetup.TopName,"play",Plugin.ThemeColor,true,simulation);
            tab.SimulationSetup = simulationSetup;

            tab.CloseButton_Clicked += new Action(() => { tab.Close(); });

            return tab;
        }

        private CodeEditor2.Tests.ITest Simulation;
        public SimPanel SimPanel;
        protected pluginVerilog.Data.SimulationSetup? SimulationSetup;
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
            // run the simulation work off the UI thread (it re-creates a
            // SimulationSetup and waits for shell prompts synchronously)
            await System.Threading.Tasks.Task.Run(
                async () => await Simulation.RunSimulationAsync(token)
                );
        }
        private void LogReceived(string lineString,Avalonia.Media.Color? color)
        {
            SimPanel.LineReceived(lineString, color);
        }
    }
}
