using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Trims process working sets to let Windows reclaim memory that isn't
/// actively in use. This does not free memory Windows couldn't already
/// reclaim on its own when something else needs it, on modern Windows the
/// real-world benefit is often smaller than "RAM cleaner" tools imply. The
/// UI shows real before/after numbers rather than a promised gain, see
/// docs/ROADMAP.md.
/// </summary>
public sealed class RamOptimizer : IRamOptimizer
{
    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public Task<RamOptimizationResult> OptimizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Optimize(cancellationToken), CancellationToken.None);
    }

    private static RamOptimizationResult Optimize(CancellationToken cancellationToken)
    {
        var before = GetAvailableMemoryBytes();

        var trimmed = 0;
        var skipped = 0;

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (EmptyWorkingSet(process.Handle))
                {
                    trimmed++;
                }
                else
                {
                    skipped++;
                }
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or UnauthorizedAccessException)
            {
                // Expected for other users' processes and protected system
                // processes, this app does not run elevated.
                skipped++;
            }
            finally
            {
                process.Dispose();
            }
        }

        var after = GetAvailableMemoryBytes();

        return new RamOptimizationResult(trimmed, skipped, before, after);
    }

    private static long GetAvailableMemoryBytes()
    {
        var status = new MEMORYSTATUSEX();
        status.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();

        return GlobalMemoryStatusEx(ref status) ? (long)status.ullAvailPhys : 0;
    }
}
