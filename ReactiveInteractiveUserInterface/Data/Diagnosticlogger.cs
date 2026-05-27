//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  [https://github.com/mpostol/TP/discussions/182](https://github.com/mpostol/TP/discussions/182)
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : IDisposable
    {
        #region Singleton

        private static readonly Lazy<DiagnosticLogger> _instance =
            new Lazy<DiagnosticLogger>(() => new DiagnosticLogger());

        public static DiagnosticLogger Instance => _instance.Value;

        #endregion Singleton

        #region private fields

        private readonly BlockingCollection<string> _buffer;
        private readonly Task _writerTask;
        private readonly string _logFilePath;
        private bool _disposed = false;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private const int MaxBufferSize = 1000;

        #endregion private fields

        #region ctor

        private DiagnosticLogger()
        {
            string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
             "TP_BallSimulator", "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"diagnostic_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            _buffer = new BlockingCollection<string>(MaxBufferSize);

            _writerTask = Task.Run(WriterLoop);
        }
        #endregion ctor

        #region public API

        public void Log(string ballId, double x, double y, double vx, double vy, double timestamp)
        {
            if (_disposed) return;

            var entry = new DiagnosticEntry
            {
                Timestamp = timestamp,
                BallId = ballId,
                X = Math.Round(x, 4),
                Y = Math.Round(y, 4),
                Vx = Math.Round(vx, 4),
                Vy = Math.Round(vy, 4)
            };

            string line = $"[{entry.Timestamp:F3}] Ball={entry.BallId} X={entry.X:F4} Y={entry.Y:F4} Vx={entry.Vx:F4} Vy={entry.Vy:F4}";

            _buffer.TryAdd(line);
        }

        #endregion public API

        #region private

        private async Task WriterLoop()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(_logFilePath, append: false, Encoding.ASCII);
                await writer.WriteLineAsync($"# Diagnostic log started at {DateTime.Now:O}");
                await writer.WriteLineAsync("# Format: [timestamp_ms] Ball=ID X=x Y=y Vx=vx Vy=vy");
                await writer.FlushAsync();

                foreach (string line in _buffer.GetConsumingEnumerable(_cts.Token))
                {
                    await writer.WriteLineAsync(line);
                    if (_buffer.Count == 0)
                        await writer.FlushAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"DiagnosticLogger error: {ex.Message}");
            }
        }

        #endregion private

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _buffer.CompleteAdding();
            _cts.CancelAfter(TimeSpan.FromSeconds(2));
            try { _writerTask.Wait(3000); } catch { }
            _buffer.Dispose();
            _cts.Dispose();
        }

        #endregion IDisposable

        #region nested types

        private struct DiagnosticEntry
        {
            public double Timestamp { get; set; }
            public string BallId { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Vx { get; set; }
            public double Vy { get; set; }
        }

        #endregion nested types
    }
}