using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GradProject
{
    public class LabManager
    {
        private readonly string _scriptPath;
        private readonly string _terminateScriptPath;
        private readonly ILogger<LabManager> _logger;
        
        public LabManager(IConfiguration configuration, ILogger<LabManager> logger)
        {
            _scriptPath = configuration["LabSettings:ScriptPath"] ?? "/app/scripts/setup-lab3rd.sh";
            _terminateScriptPath = configuration["LabSettings:TerminateScriptPath"] ?? "/app/scripts/Terminate_lab2nd.sh";
            _logger = logger;
            
            _logger.LogInformation("Resolved script path: {ScriptPath}", _scriptPath);
            _logger.LogInformation("Resolved terminate script path: {TerminateScriptPath}", _terminateScriptPath);
            
            if (!File.Exists(_scriptPath))
            {
                _logger.LogError("Script file does not exist at {ScriptPath}", _scriptPath);
                throw new FileNotFoundException($"Script file not found at {_scriptPath}");
            }
            
            if (!File.Exists(_terminateScriptPath))
            {
                _logger.LogError("Terminate script file does not exist at {TerminateScriptPath}", _terminateScriptPath);
                throw new FileNotFoundException($"Terminate script file not found at {_terminateScriptPath}");
            }
            
            try
            {
                var scriptContent = File.ReadAllText(_scriptPath);
                _logger.LogInformation("Script content at {ScriptPath}:\n{ScriptContent}", _scriptPath, scriptContent);
                
                var terminateScriptContent = File.ReadAllText(_terminateScriptPath);
                _logger.LogInformation("Terminate script content at {TerminateScriptPath}:\n{TerminateScriptContent}", _terminateScriptPath, terminateScriptContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read script content");
            }
        }
        
      	public async Task<string> CreateLabAsync(string username, string imageName)
        {
            _logger.LogInformation("Creating lab for username: {Username} with image: {ImageName}", username, imageName);

            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogError("Username is null or empty");
                throw new ArgumentException("Username cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(imageName))
            {
                _logger.LogError("Image name is null or empty");
                throw new ArgumentException("Image name cannot be null or empty");
            }

            _logger.LogInformation("Cleaning up existing container for {Username}", username);
            var cleanupProcess = new ProcessStartInfo
            {
                FileName = "docker",
                ArgumentList = { "stop", $"{username}_lab_container" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var cleanup = new Process { StartInfo = cleanupProcess })
            {
                cleanup.Start();
                await cleanup.WaitForExitAsync();
            }

            cleanupProcess = new ProcessStartInfo
            {
                FileName = "docker",
                ArgumentList = { "rm", $"{username}_lab_container" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var cleanup = new Process { StartInfo = cleanupProcess })
            {
                cleanup.Start();
                await cleanup.WaitForExitAsync();
            }

            _logger.LogInformation("Executing script {ScriptPath} with username {Username} and image {ImageName}", _scriptPath, username, imageName);
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                ArgumentList = { _scriptPath, username, imageName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, args) => { if (args.Data != null) output.AppendLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) error.AppendLine(args.Data); };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                var scriptOutput = output.ToString().Trim();
                var scriptError = error.ToString().Trim();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Script failed with exit code {ExitCode}: {Error}", process.ExitCode, scriptError);
                    throw new Exception($"Script failed: {scriptError}");
                }

                _logger.LogInformation("Script succeeded with output: {Output}", scriptOutput);

                var labUrlLine = scriptOutput.Split('\n').LastOrDefault(line => line.StartsWith("Lab URL:"));
                if (string.IsNullOrEmpty(labUrlLine))
                {
                    _logger.LogError("Lab URL not found in script output: {Output}", scriptOutput);
                    throw new Exception("Lab URL not found in script output");
                }
                var labUrl = labUrlLine.Replace("Lab URL: ", "").Trim();

                return labUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute script for username {Username} with image {ImageName}", username, imageName);
                throw;
            }
        }
        public async Task ExtendLabTimeAsync(string username)
        {
            _logger.LogInformation("ExtendLabTimeAsync called for username: {Username}. No action taken (database disabled).", username);
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogError("Username is null or empty");
                throw new ArgumentException("Username cannot be null or empty");
            }
            
            await Task.CompletedTask;
        }
        
        public async Task TerminateLabAsync(string labUrl)
        {
            _logger.LogInformation("Terminating lab for URL: {LabUrl}", labUrl);
            
            if (string.IsNullOrWhiteSpace(labUrl))
            {
                _logger.LogError("Lab URL is null or empty");
                throw new ArgumentException("Lab URL cannot be null or empty");
            }
            
            _logger.LogInformation("Executing terminate script {TerminateScriptPath} with URL {LabUrl}", _terminateScriptPath, labUrl);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"{_terminateScriptPath} {labUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();
            
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) output.AppendLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) error.AppendLine(args.Data); };
            
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                
                var scriptOutput = output.ToString().Trim();
                var scriptError = error.ToString().Trim();
                
                if (process.ExitCode != 0)
                {
                    _logger.LogError("Terminate script failed with exit code {ExitCode}: {Error}", process.ExitCode, scriptError);
                    throw new Exception($"Terminate script failed: {scriptError}");
                }
                
                _logger.LogInformation("Terminate script succeeded with output: {Output}", scriptOutput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute terminate script for URL {LabUrl}", labUrl);
                throw;
            }
        }
    }
}
