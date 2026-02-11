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

        public bool Initialize()
        {
            pluginVerilog.NavigatePanel.VerilogFileNode.CustomizeNavigateNodeContextMenu += CustomizeNavigateNodeContextMenuHandler;
            return true;
        }

        // Attached to VerilogFileNode context menu
        public static void CustomizeNavigateNodeContextMenuHandler(Avalonia.Controls.ContextMenu contextMenu)
        {
            MenuItem menuItem_IcarusVerilog = CodeEditor2.Global.CreateMenuItem(
                "IcarusVerilog", "menuItem_IcarusVerilog",
                "CodeEditor2/Assets/Icons/play.svg",
                ThemeColor
                );
            contextMenu.Items.Add(menuItem_IcarusVerilog);
            MenuItem menuItem_RunSimulation = CodeEditor2.Global.CreateMenuItem(
                "Run Simulation",
                "menuItem_RunSimulation",
                "CodeEditor2/Assets/Icons/play.svg",
                ThemeColor
                ); ;
            menuItem_IcarusVerilog.Items.Add(menuItem_RunSimulation);
            menuItem_RunSimulation.Click += MenuItem_RunSimulation_Click;
        }

        private static void MenuItem_RunSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CodeEditor2.Data.File? file;
            file = CodeEditor2.Controller.NavigatePanel.GetSelectedFile();

            pluginVerilog.Data.VerilogFile? vFile = file as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return;

            Simulation.IcarusVerilogSimulation icarusVerilogSimulation = new Simulation.IcarusVerilogSimulation();
            icarusVerilogSimulation.TopFile = vFile;


            Views.SimulationTab? tab = Views.SimulationTab.Create(icarusVerilogSimulation);
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
