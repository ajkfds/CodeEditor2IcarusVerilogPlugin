using CodeEditor2.Shells;
using CodeEditor2.Tests;
using pluginIcarusVerilog.Views;
using pluginVerilog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pluginIcarusVerilog.Simulation
{
    public class IcarusVerilogSimulation : CodeEditor2.Tests.ITest
    {
        public CodeEditor2.Data.File? File { get; set; } = null;
        protected CodeEditor2.Shells.WinCmdShell? shell;
        private const string prompt = "icarusVerilogShell";

        public Action<string, Avalonia.Media.Color?>? LogReceived { get; set; } = null;

        public async Task<string> RunSimulationAsync(CancellationToken cancellationToken)
        {
            pluginVerilog.Data.VerilogFile? vFile = File as pluginVerilog.Data.VerilogFile;
            if (vFile == null) return "failed to launch simulation.";

            pluginVerilog.Data.SimulationSetup? simulationSetup = pluginVerilog.Data.SimulationSetup.Create(vFile);
            if (simulationSetup == null) return "failed to launch simulation.";

            string simName = simulationSetup.TopName;


            string simulationPath = Setup.SimulationPath + "\\" + simName;
            simulationPath = @"c:\temp\" + simName;

            if (!System.IO.Directory.Exists(simulationPath))
            {
                System.IO.Directory.CreateDirectory(simulationPath);
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(simulationPath + "\\command"))
            {
                foreach (string includePath in simulationSetup.IncludePaths)
                {
                    sw.WriteLine("+incdir+" + includePath); // path with space is not accepted
                }
                foreach (CodeEditor2.Data.File file in simulationSetup.Files)
                {
                    sw.WriteLine(simulationSetup.Project.GetAbsolutePath(file.RelativePath));
                }
            }

            shell = new CodeEditor2.Shells.WinCmdShell(new List<string> {
                "prompt "+prompt+"$G$_",
                simulationPath.Substring(0,2),
                "cd "+simulationPath
            });

            shell.Start();
            shell.LineReceived += Shell_LineReceived;

            await Task.Delay(1, cancellationToken);
            while (shell.GetLastLine() != prompt + ">")
            {
                await Task.Delay(10, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return "simulation canceled.";
            }
            await Task.Delay(1, cancellationToken);
            shell.Execute("del " + simName + ".o");
            shell.ClearLogs();
            shell.StartLogging();
//            await Task.Delay(1, cancellationToken);
//            shell.Execute("dir");
            shell.Execute(Setup.BinPath + "iverilog -g2012 -f command -o "+simName+".o");
            await Task.Delay(1, cancellationToken);
            while (shell.GetLastLine() != prompt + ">")
            {
                await Task.Delay(10, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return "simulation canceled.";
            }
//            shell.ClearLogs();
//            shell.StartLogging();
            shell.Execute(Setup.BinPath + "vvp " + simName + ".o");
            while (shell.GetLastLine() != prompt + ">")
            {
                await Task.Delay(10, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return "simulation canceled.";
            }
            List<string> logs = shell.GetLogs();
            StringBuilder sb = new StringBuilder();
            foreach (string log in logs)
            {
                sb.Append(log);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private void Shell_LineReceived(string lineString)
        {
            if (lineString == prompt + ">")
            {
                if(LogReceived  != null) LogReceived(lineString, Avalonia.Media.Colors.Green);
            }
            else
            {
                if (LogReceived != null) LogReceived(lineString, null);
            }
        }

        public Task<TestResult> GetSimulationResultAsync(CancellationToken cancellationToken)
        {
            CodeEditor2.Tests.TestResult result = new TestResult();
            result.Status = TestResult.TestStatus.NoResult;
            return Task.FromResult(result);
        }
    }
}
