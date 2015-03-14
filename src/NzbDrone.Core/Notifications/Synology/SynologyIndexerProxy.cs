using System;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Core.Notifications.Synology
{
    public interface ISynologyIndexerProxy
    {
        bool Test();
        void AddFile(string filepath);
        void DeleteFile(string filepath);
        void AddFolder(string folderpath);
        void DeleteFolder(string folderpath);
        void UpdateFolder(string folderpath);
        void UpdateLibrary();
    }

    public class SynologyIndexerProxy : ISynologyIndexerProxy
    {
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public SynologyIndexerProxy(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public bool Test()
        {
            try
            {
                ExecuteCommand("--help");
                return true;
            }
            catch (Exception ex)
            {
                _logger.DebugException("synoindex not available", ex);
                return false;
            }
        }

        public void AddFile(string filePath)
        {
            ExecuteCommand("-a " + Escape(filePath));
        }

        public void DeleteFile(string filePath)
        {
            ExecuteCommand("-d " + Escape(filePath));
        }

        public void AddFolder(string folderPath)
        {
            ExecuteCommand("-A " + Escape(folderPath));
        }

        public void DeleteFolder(string folderPath)
        {
            ExecuteCommand("-D " + Escape(folderPath));
        }

        public void UpdateFolder(string folderPath)
        {
            ExecuteCommand("-R " + Escape(folderPath));
        }

        public void UpdateLibrary()
        {
            ExecuteCommand("-R video");
        }

        private void ExecuteCommand(string args)
        {
            var output = _processProvider.StartAndCapture("/usr/syno/bin/synoindex", args);

            if (output.Standard.Count != 0)
            {
                throw new SynologyException("synoindex returned an error: {0}", string.Join("\n", output.Standard));
            }

            if (output.Error.Count != 0)
            {
                throw new SynologyException("synoindex returned an error: {0}", string.Join("\n", output.Error));
            }
        }

        private string Escape(string arg)
        {
            return arg.Replace("\\", "\\\\").Replace(" ", "\\ ").Replace("\"", "\\\"");
        }
    }
}
