﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TestingFramework.Testing;

namespace TestingFramework.AlgoIntegration
{
    public partial class CentroidDecompositionRecoveryAlgorithm : Algorithm
    {
        public List<int> KList = new List<int>(new[] { 3, 2 });
        //public List<int> KList = new List<int>(new[] { AlgoPack.TypicalTruncation });

        private static bool _init = false;
        public CentroidDecompositionRecoveryAlgorithm() : base(ref _init)
        { }
        
        public override string[] EnumerateOutputFiles(int tcase)
        {
            return KList.Select(k => $"{AlgCode}{tcase}_k{k}.txt").ToArray();
        }

        private static string StyleOf(int k) =>
            "linespoints lt 8 lw 3 pt 7 lc rgbcolor \"" + (k == 2 ? "blue" : "cyan") + $"\" pointsize 1.2";

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms()
        {
            return KList.Select(k => new SubAlgorithm($"{AlgCode}_k{k}", String.Empty, StyleOf(k)));
        }

        public override IEnumerable<SubAlgorithm> EnumerateSubAlgorithms(int tcase)
        {
            return KList.Select(k => new SubAlgorithm($"{AlgCode}_k{k}", $"{AlgCode}{tcase}_k{k}", StyleOf(k)));
        }
        
        protected override void PrecisionExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            KList.ForEach(k => RunCd(GetCdProcess(data.N, data.M, data, tcase, k)));
        }

        private Process GetCdProcess(int n, int m, DataDescription data, int len, int k)
        {
            Process cdproc = new Process();

            cdproc.StartInfo.WorkingDirectory = EnvPath;
            cdproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/algoCollection";
            cdproc.StartInfo.CreateNoWindow = true;
            cdproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cdproc.StartInfo.UseShellExecute = false;

            cdproc.StartInfo.Arguments = $"-alg cd -test o -n {n} -m {m} -k {k} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}_k{k}.txt";

            return cdproc;
        }

        private void RunCd(Process cdproc)
        {
            cdproc.Start();
            cdproc.WaitForExit();
            
            if (cdproc.ExitCode != 0)
            {
                string errText =
                    $"[WARNING] CD returned code {cdproc.ExitCode} on exit.{Environment.NewLine}" +
                    $"CLI args: {cdproc.StartInfo.Arguments}";
                
                Console.WriteLine(errText);
                Utils.DelayedWarnings.Enqueue(errText);
            }
        }

        protected override void RuntimeExperiment(ExperimentType et, ExperimentScenario es,
            DataDescription data, int tcase)
        {
            if (et == ExperimentType.Streaming)
            {
                KList.ForEach(k => RunCd(GetStreamingCdProcess(data.N, data.M, data, tcase, k)));
            }
            else
            {
                KList.ForEach(k => RunCd(GetRuntimeCdProcess(data.N, data.M, data, tcase, k)));
            }
        }

        private Process GetRuntimeCdProcess(int n, int m, DataDescription data, int len, int k)
        {
            Process cdproc = new Process();

            cdproc.StartInfo.WorkingDirectory = EnvPath;
            cdproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/algoCollection";
            cdproc.StartInfo.CreateNoWindow = true;
            cdproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cdproc.StartInfo.UseShellExecute = false;

            cdproc.StartInfo.Arguments = $"-alg cd -test rt -n {n} -m {m} -k {k} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}_k{k}.txt";

            return cdproc;
        }
        
        private Process GetStreamingCdProcess(int n, int m, DataDescription data, int len, int k)
        {
            Process cdproc = new Process();

            cdproc.StartInfo.WorkingDirectory = EnvPath;
            cdproc.StartInfo.FileName = EnvPath + "../cmake-build-debug/algoCollection";
            cdproc.StartInfo.CreateNoWindow = true;
            cdproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cdproc.StartInfo.UseShellExecute = false;

            int istep = data.MissingBlocks[0].Item3;
            int max = n;
            n = max - istep;

            cdproc.StartInfo.Arguments = $"-test rt -n {n} -m {m} -k {k} " +
                                         $"-istep {istep} -max {max} " +
                                         $"-in ./{SubFolderDataIn}{data.Code}_m{len}.txt " +
                                         $"-out ./{SubFolderDataOut}{AlgCode}{len}_k{k}.txt";

            return cdproc;
        }
    }
}