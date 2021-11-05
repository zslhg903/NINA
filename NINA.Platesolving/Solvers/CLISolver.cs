#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Image.Interfaces;
using NINA.Core.Utility;
using NINA.Core.Utility.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NINA.Image.FileFormat;
using NINA.Core.Model;
using NINA.Core.Locale;
using System.Collections.Generic;

namespace NINA.PlateSolving.Solvers {

    internal abstract class CLISolver : BaseSolver {
        protected string executableLocation;

        public CLISolver(string executableLocation) {
            this.executableLocation = executableLocation;
        }

        protected abstract string GetLocalizedPlateSolverName();

        protected abstract string GetArguments(
            string imageFilePath,
            string outputFilePath,
            PlateSolveParameter parameter,
            PlateSolveImageProperties imageProperties);

        protected abstract PlateSolveResult ReadResult(
            string outputFilePath,
            PlateSolveParameter parameter,
            PlateSolveImageProperties imageProperties);

        protected override async Task<PlateSolveResult> SolveAsyncImpl(
            IImageData source,
            PlateSolveParameter parameter,
            PlateSolveImageProperties imageProperties,
            IProgress<ApplicationStatus> progress,
            CancellationToken cancelToken) {
            var result = new PlateSolveResult() { Success = false };
            string imagePath = null, outputPath = null;
            try {
                //Copy Image to local app data
                imagePath = await PrepareAndSaveImage(source, cancelToken);

                progress?.Report(new ApplicationStatus() { Status = Loc.Instance["LblSolving"] });

                outputPath = GetOutputPath(imagePath);

                await StartCLI(imagePath, outputPath, parameter, imageProperties, progress, cancelToken);

                //Extract solution coordinates
                result = ReadResult(outputPath, parameter, imageProperties);
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });
                if (imagePath != null && File.Exists(imagePath)) {
                    File.Delete(imagePath);
                }
                if (outputPath != null && File.Exists(outputPath)) {
                    File.Delete(outputPath);
                }

                foreach (var file in GetSideCarFilePaths(imagePath)) {
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                }
            }
            return result;
        }

        protected async Task<string> PrepareAndSaveImage(IImageData source, CancellationToken cancelToken) {
            FileSaveInfo fileSaveInfo = new FileSaveInfo {
                FilePath = WORKING_DIRECTORY,
                FilePattern = Path.GetRandomFileName(),
                FileType = FileTypeEnum.FITS
            };

            return await source.SaveToDisk(fileSaveInfo, cancelToken, forceFileType: true);
        }

        protected abstract string GetOutputPath(string imageFilePath);

        /// <summary>
        /// Some solvers create more files than the result output path.
        /// Return a list of paths to those sidecar files be deleted.
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        protected virtual List<string> GetSideCarFilePaths(string imageFilePath) {
            return new List<string>();
        }

        protected async Task StartCLI(string imageFilePath, string outputFilePath, PlateSolveParameter parameter, PlateSolveImageProperties imageProperties, IProgress<ApplicationStatus> progress, CancellationToken ct) {
            if (executableLocation != "cmd.exe" && !File.Exists(executableLocation)) {
                throw new FileNotFoundException("Executable not found", executableLocation);
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = executableLocation;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = GetArguments(imageFilePath, outputFilePath, parameter, imageProperties);
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) => {
                progress?.Report(new ApplicationStatus() { Status = e.Data });
                Logger.Debug(e.Data);
            };

            process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) => {
                progress?.Report(new ApplicationStatus() { Status = e.Data });
                Logger.Error(e.Data);
            };
            Logger.Debug($"Starting process '{executableLocation}' with args '{startInfo.Arguments}'");
            process.Start();
            await process.WaitForExitAsync(ct);
        }
    }
}