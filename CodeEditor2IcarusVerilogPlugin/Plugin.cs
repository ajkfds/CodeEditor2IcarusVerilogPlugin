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

        private void projectCreated(CodeEditor2.Data.Project project)
        {
//            project.ProjectProperties.Add(Id, new ProjectProperty(project));
        }

        public bool Initialize()
        {
            ContextMenu contextMenu = CodeEditor2.Controller.NavigatePanel.GetContextMenu();
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
            // register project property form tab
            //            CodeEditor.Tools.ProjectPropertyForm.FormCreated += Tools.ProjectPropertyTab.ProjectPropertyFromCreated;

            return true;
        }

        private void MenuItem_RunSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Views.SimulationTab? tab = Views.SimulationTab.Create();
            if (tab == null) return;
            CodeEditor2.Controller.Tabs.AddItem(tab);
            tab.Run();
        }
    }
}
