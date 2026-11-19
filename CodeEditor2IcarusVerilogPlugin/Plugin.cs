using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace pluginIcarusVerilog
{
    public class Plugin : CodeEditor2Plugin.IPlugin
    {
        public static string StaticID = "IcarusVerilog";
        public string Id { get { return StaticID; } }


        public static Avalonia.Media.Color ThemeColor = Avalonia.Media.Color.FromArgb(255, 150, 50, 50);
        public bool Register()
        {
            if (!CodeEditor2.Global.Plugins.ContainsKey("Verilog")) return false;

            // register project property creator
            CodeEditor2.Data.Project.Created += projectCreated;

            return true;
        }

        private void projectCreated(CodeEditor2.Data.Project project,CodeEditor2.Data.Project.Setup? setup)
        {
//            project.ProjectProperties.Add(Id, new ProjectProperty(project));
        }

        /// <summary>
        /// Id of the "IcarusVerilog" grouping row in the shared node menu.
        /// </summary>
        public const string MenuGroupId = "icarusVerilog";

        /// <summary>Id of the "Run Simulation" row inside that group.</summary>
        public const string MenuRunSimulationId = "icarusRunSimulation";

        public bool Initialize()
        {
            // The menu row is a catalog entry now, not a ContextMenu subscription:
            // the editor and VS Code then share one declaration, and the row can
            // carry the condition under which it makes sense (see Available).
            pluginVerilog.CoreBridge.HierarchyMenuCatalog.Register(new pluginVerilog.CoreBridge.HierarchyMenuCatalog.Entry
            {
                Id = MenuGroupId,
                Label = "IcarusVerilog",
                Detail = "Icarus Verilog simulation",
                Targets = pluginVerilog.CoreBridge.HierarchyMenuCatalog.AppliesTo.File,
                // The simulation tab and its dialogs exist only in the editor.
                Hosts = pluginVerilog.CoreBridge.HierarchyMenuCatalog.Hosts.Desktop,
                Group = 30,
                IconPath = "CodeEditor2/Assets/Icons/play.svg",
                IconColor = PackColor(ThemeColor),
                Children =
                {
                    new pluginVerilog.CoreBridge.HierarchyMenuCatalog.Entry
                    {
                        Id = MenuRunSimulationId,
                        Label = "Run Simulation",
                        Detail = "compile and run the hierarchy with Icarus Verilog",
                        Targets = pluginVerilog.CoreBridge.HierarchyMenuCatalog.AppliesTo.File,
                        Hosts = pluginVerilog.CoreBridge.HierarchyMenuCatalog.Hosts.Desktop,
                        Group = 10,
                        IconPath = "CodeEditor2/Assets/Icons/play.svg",
                        IconColor = PackColor(ThemeColor),
                        Action = pluginVerilog.CoreBridge.HierarchyMenuCatalog.ActionKind.DesktopHandler,
                        // Only offered when this plugin is actually loaded: the
                        // row cannot work otherwise, and the catalog is static
                        // data that cannot tell on its own.
                        Available = () => CodeEditor2.Global.Plugins.ContainsKey(StaticID),
                        Handler = RunSimulation,
                    },
                },
            });
            return true;
        }

        /// <summary>Packs an Avalonia colour into the catalog's 0xAARRGGBB form.</summary>
        private static uint PackColor(Avalonia.Media.Color color)
        {
            return ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
        }

        /// <summary>
        /// Runs the simulation for the node the menu was opened on. The context
        /// carries that node, so this does not depend on the current selection
        /// (which may have moved while the menu was open).
        /// </summary>
        private static void RunSimulation(pluginVerilog.CoreBridge.NavigateNodeMenuContext context)
        {
            pluginVerilog.Data.VerilogFile? vFile = context.File as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return;
            _ = RunSimulationAsync(vFile);
        }

        private static async System.Threading.Tasks.Task RunSimulationAsync(pluginVerilog.Data.VerilogFile vFile)
        {

            Simulation.IcarusVerilogSimulation icarusVerilogSimulation = new Simulation.IcarusVerilogSimulation();
            icarusVerilogSimulation.File = vFile;


            // heavy SimulationSetup creation runs off the UI thread
            Views.SimulationTab? tab = await Views.SimulationTab.CreateAsync(icarusVerilogSimulation);
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
