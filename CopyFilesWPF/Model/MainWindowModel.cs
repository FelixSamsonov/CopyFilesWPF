using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static CopyFilesWPF.Model.FileCopier;

namespace CopyFilesWPF.Model
{
    public class MainWindowModel
    {
        public FilePath FilePath { get; set; }

        public MainWindowModel() {
            FilePath = new FilePath();
        }

        public async Task CopyFile(ProgressChangeDelegate onProgressChanged, CompleteDelegate onComplete, Grid gridPanel)
        {
            var copier = new FileCopier(FilePath, onProgressChanged, onComplete, gridPanel);
            gridPanel.Tag = copier;
            await copier.CopyFileAsync();
        }
    }
}
