using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CopyFilesWPF.Model
{
    public class FileCopier
    {
        private readonly Grid _gridPanel;
        private readonly FilePath _filePath;

        public delegate void ProgressChangeDelegate(double progress, ref bool cancel, Grid gridPanel);
        public delegate void CompleteDelegate(Grid gridPanel);
        public event ProgressChangeDelegate OnProgressChanged;
        public event CompleteDelegate OnComplete;

        public bool CancelFlag = false;
        public ManualResetEvent PauseFlag = new(true);

        public FileCopier(
            FilePath filePath,
            ProgressChangeDelegate onProgressChange,
            CompleteDelegate onComplete,
            Grid gridPanel)
        {
            OnProgressChanged += onProgressChange;
            OnComplete += onComplete;
            _filePath = filePath;
            _gridPanel = gridPanel;
        }
        public async Task<byte[]> CreateBufferAsync()
        {
            return await Task.Run(() => new byte[1024 * 1024]);
        }
        public async Task CopyFileAsync()
        {
            byte[] buffer = await CreateBufferAsync();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            {
                try
                {
                    using (var source = new FileStream(_filePath.PathFrom, FileMode.Open, FileAccess.Read))
                    {
                        var fileLength = source.Length;
                        using (var destination = new FileStream(_filePath.PathTo, FileMode.CreateNew, FileAccess.Write))
                        {
                            long totalBytes = 0;
                            int currentBlockSize = 0;

                            while ((currentBlockSize = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                token.ThrowIfCancellationRequested();

                                totalBytes += currentBlockSize;
                                double percentage = totalBytes * 100.0 / fileLength;

                                await destination.WriteAsync(buffer, 0, currentBlockSize);
                                OnProgressChanged(percentage, ref CancelFlag, _gridPanel);

                                if (CancelFlag)
                                {
                                    File.Delete(_filePath.PathTo);
                                    break;
                                }
                                await Task.Delay(100);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Operation was canceled.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException error)
                {
                    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    OnComplete(_gridPanel);
                }
            }
        }
    }
}
