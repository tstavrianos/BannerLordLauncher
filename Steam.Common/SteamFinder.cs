using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text.RegularExpressions;
using Splat;

namespace Steam.Common
{
    using Microsoft.Win32;

    /// <summary>
    /// Steam installation path and Steam games folder finder.
    /// </summary>
    public sealed class SteamFinder : IEnableLogger
    {
        public string SteamPath { get; private set; }
        public string[] Libraries { get; private set; }

        /// <summary>
        /// Tries to find the Steam folder and its libraries on the system.
        /// </summary>
        /// <returns>Returns true if a valid Steam installation folder path was found.</returns>
        public bool FindSteam()
        {
            this.SteamPath = null;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    this.SteamPath = FindWindowsSteamPath();
                    break;
                default:
                    if (IsUnix()) this.SteamPath = FindUnixSteamPath();
                    break;
            }

            return this.SteamPath != null && this.FindLibraries();
        }

        /// <summary>
        /// Retrieves the game folder by reading the game's Steam manifest. The game needs to be marked as installed on Steam.
        /// <para>Returns null if not found.</para>
        /// </summary>
        /// <param name="appId">The game's app id on Steam.</param>
        /// <returns>The path to the game folder.</returns>
        public string FindGameFolder(int appId)
        {
            if (this.Libraries == null)
            {
                this.Log().Error("Steam must be found first.");
                return null;
            }

            foreach (var library in this.Libraries)
            {
                var gameManifestPath = GetManifestFilePath(library, appId);
                if (gameManifestPath == null)
                    continue;

                var gameFolderName = ReadInstallDirFromManifest(gameManifestPath);
                if (gameFolderName == null)
                    continue;

                return Path.Combine(library, "common", gameFolderName);
            }

            return null;
        }

        /// <summary>
        /// Searches for a game directory that has the specified name in known libraries.
        /// </summary>
        /// <param name="gameFolderName">The game's folder name inside the Steam library.</param>
        /// <returns>The game folders path in the libraries.</returns>
        public IEnumerable<string> FindGameFolders(string gameFolderName)
        {
            if (this.Libraries == null)
            {
                this.Log().Error("Steam must be found first.");
                yield break;
            }

            gameFolderName = gameFolderName.ToLowerInvariant();

            foreach (var library in this.Libraries)
            {
                var folder = Directory.EnumerateDirectories(library)
                    .FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant() == gameFolderName);

                if (folder != null)
                    yield return folder;
            }
        }

        private bool FindLibraries()
        {
            var steamLibraries = new List<string>();
            var steamDefaultLibrary = Path.Combine(this.SteamPath, "steamapps");
            if (!Directory.Exists(steamDefaultLibrary))
                return false;

            steamLibraries.Add(steamDefaultLibrary);

            /*
             * Get library folders paths from libraryfolders.vdf
             *
             * Libraries are listed like this:
             *     "id"   "library folder path"
             *
             * Examples:
             *     "1"   "D:\\Games\\SteamLibraryOnD"
             *     "2"   "E:\\Games\\steam_games"
             */
            var regex = new Regex(@"""\d+""\s+""(.+)""");
            var libraryFoldersFilePath = Path.Combine(steamDefaultLibrary, "libraryfolders.vdf");
            if (File.Exists(libraryFoldersFilePath))
            {
                foreach (var line in File.ReadAllLines(libraryFoldersFilePath))
                {
                    var match = regex.Match(line);
                    if (!match.Success)
                        continue;

                    var libPath = match.Groups[1].Value;
                    libPath = libPath.Replace("\\\\", "\\"); // unescape the backslashes
                    libPath = Path.Combine(libPath, "steamapps");
                    if (Directory.Exists(libPath))
                        steamLibraries.Add(libPath);
                }
            }

            this.Libraries = steamLibraries.ToArray();
            return true;
        }

        private static string GetManifestFilePath(string libraryPath, int appId)
        {
            var manifestPath = Path.Combine(libraryPath, $"appmanifest_{appId}.acf");
            return File.Exists(manifestPath) ? manifestPath : null;
        }

        private static string ReadInstallDirFromManifest(string manifestFilePath)
        {
            var regex = new Regex(@"""installdir""\s+""(.+)""");
            foreach (var line in File.ReadAllLines(manifestFilePath))
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                return match.Groups[1].Value;
            }

            return null;
        }

        private static string FindWindowsSteamPath()
        {
            var regPath = Environment.Is64BitOperatingSystem
                 ? @"SOFTWARE\Wow6432Node\Valve\Steam"
                 : @"SOFTWARE\Valve\Steam";
            try
            {
                var subRegKey = Registry.LocalMachine.OpenSubKey(regPath);
                var path = subRegKey?.GetValue("InstallPath").ToString()
                    .Replace('/', '\\'); // not actually required, just for consistency's sake

                return Directory.Exists(path) ? path : null;
            }
            catch
            {
                return null;
            }
        }

        private static string FindUnixSteamPath()
        {
            string path;
            if (Directory.Exists(path = GetDefaultLinuxSteamPath())
                || Directory.Exists(path = GetDefaultMacOsSteamPath()))
            {
                return path;
            }

            return null;
        }

        private static string GetDefaultLinuxSteamPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local/share/Steam/"
            );
        }

        private static string GetDefaultMacOsSteamPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library/Application Support/Steam"
            );
        }

        // https://stackoverflow.com/questions/5116977
        private static bool IsUnix()
        {
            var p = (int)Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        }
    }
}