using CopyFilesWPF.Model;
using CopyFilesWPF.View;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CopyFilesWPF.Presenter
{
    public class MainWindowPresenter : IMainWindowPresenter
    {
        private readonly IMainWindowView _mainWindowView;
        private readonly MainWindowModel _mainWindowModel;

        public MainWindowPresenter(IMainWindowView mainWindowView)
        {
            _mainWindowView = mainWindowView;
            _mainWindowModel = new MainWindowModel();
        }

        public void ChooseFileFromButtonClick(string path)
        {
            _mainWindowModel.FilePath.PathFrom = path;
        }

        public void ChooseFileToButtonClick(string path)
        {
            _mainWindowModel.FilePath.PathTo = path;
        }

        public void CopyButtonClick()
        {
            _mainWindowModel.FilePath.PathFrom = _mainWindowView.MainWindowView.FromTextBox.Text;
            _mainWindowModel.FilePath.PathTo = _mainWindowView.MainWindowView.ToTextBox.Text;
            _mainWindowView.MainWindowView.FromTextBox.Text = "";
            _mainWindowView.MainWindowView.ToTextBox.Text = "";
            _mainWindowView.MainWindowView.Height = _mainWindowView.MainWindowView.Height + 60;
            MainPanel();
        }
        public void MainPanel()
        {
            var newPanel = new Grid();
            newPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            newPanel.ColumnDefinitions.Add(new ColumnDefinition());
            newPanel.ColumnDefinitions.Add(new ColumnDefinition());
            newPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            newPanel.RowDefinitions.Add(new RowDefinition());

            CreateSourcePathPanel(newPanel);
            CreateDestinationPathPanel(newPanel);
            CreatePauseButtonPanel(newPanel);
            CreateCancelButtonPanel(newPanel);

            _mainWindowView.MainWindowView.MainPanel.Children.Add(newPanel);
            _mainWindowModel.CopyFile(ProgressChanged, ModelOnComplete, newPanel);

        }
        public void CreateSourcePathPanel(Grid newPanel)
        {
            var nameFile = new TextBlock
            {
                Text = Path.GetFileName(_mainWindowModel.FilePath.PathFrom),
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetRow(nameFile, 0);
            Grid.SetColumn(nameFile, 0);
            newPanel.Children.Add(nameFile);
        }
        public void CreateDestinationPathPanel(Grid newPanel)
        {
            var progressBar = new ProgressBar
            {
                Margin = new Thickness(10, 10, 10, 10)
            };
            Grid.SetRow(progressBar, 1);
            newPanel.Children.Add(progressBar);
        }
        public void CreatePauseButtonPanel(Grid newPanel)
        {
            var pauseB = new Button
            {
                Content = "Pause",
                Margin = new Thickness(5),
                Tag = newPanel
            };
            pauseB.Click += PauseClick;
            Grid.SetRow(pauseB, 1);
            Grid.SetColumn(pauseB, 1);
            newPanel.Children.Add(pauseB);
        }
        public void CreateCancelButtonPanel(Grid newPanel)
        {
            var cancelB = new Button
            {
                Content = "Cancel",
                Margin = new Thickness(5),
                Tag = newPanel
            };
            cancelB.Click += CancelClick;
            Grid.SetRow(cancelB, 1);
            Grid.SetColumn(cancelB, 2);
            newPanel.Children.Add(cancelB);
            DockPanel.SetDock(newPanel, Dock.Top);
            newPanel.Height = 60;
        }
        private void CancelClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ((Button)sender).IsEnabled = false;
            if (((Button)sender)!.Content.ToString()!.Equals("Cancel"))
            {
                ((((Button)sender).Tag as Grid)!.Tag as FileCopier)!.CancelFlag = true;
            }
        }
        private void PauseClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ((Button)sender).IsEnabled = false;
            if (((Button)sender)!.Content.ToString()!.Equals("Pause"))
            {
                ((((Button)sender).Tag as Grid)!.Tag as FileCopier)!.PauseFlag.Reset();
            }
            else
            {
                ((((Button)sender).Tag as Grid)!.Tag as FileCopier)!.PauseFlag.Set();
            }
        }

        private void ModelOnComplete(Grid panel)
        {
            _mainWindowView.MainWindowView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    _mainWindowView.MainWindowView.Height = _mainWindowView.MainWindowView.Height - 60;
                    _mainWindowView.MainWindowView.MainPanel.Children.Remove(panel);
                    _mainWindowView.MainWindowView.CopyButton.IsEnabled = true;
                }
            );
        }
        private void ProgressChanged(double persentage, ref bool cancelFlag, Grid panel)
        {
            _mainWindowView.MainWindowView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    UpdateProgressBar(persentage, panel);
                    ButtonsSwitcher(panel);
                }
            );
        }
        private void UpdateProgressBar(double persentage, Grid panel)
        {
            foreach (var el in panel.Children)
            {
                if (el is ProgressBar bar)
                {
                    bar.Value = persentage;
                }
            }
        }
        private void ButtonsSwitcher(Grid panel)
        {
            foreach (var el in panel.Children)
            {
                if (el is Button button)
                {
                    if (button.Content.ToString() == "Resume" && !button.IsEnabled)
                    {
                        button.Content = "Pause";
                        button.IsEnabled = true;
                    }
                    else if (button.Content.ToString() == "Pause" && !button.IsEnabled)
                    {
                        button.Content = "Resume";
                        button.IsEnabled = true;
                    }
                }
            }
        }
    }
}
