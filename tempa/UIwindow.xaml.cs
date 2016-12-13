using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using CoffeeJelly.tempa.Exceptions;
using CoffeeJelly.tempa.Extensions;
using CoffeeJelly.tempadll.Extensions;
using System.Collections.ObjectModel;
using CoffeeJelly.tempa.Controls;
using CoffeeJelly.tempa.FolderBrowser.ViewModel;
using CoffeeJelly.tempadll;
using CoffeeJelly.tempadll.Exceptions;

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class UIwindow : Window, INotifyPropertyChanged
    {

        public UIwindow()
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            ManualInitializing();
            Settings();

            LogMaker.Log("Графичейский интерфейс запущен!", false);
            LogMaker.Log($"Мониторинг данных Agrolog {(IsAgrologDataCollect ? "активен" : "не активен")}", false);
            LogMaker.Log($"Мониторинг данных Грейнбар {(IsGrainbarDataCollect ? "активен" : "не активен")}", false);
        }

        private void Settings()
        {
            _agrologReportsFolderBrowserViewModel.Path = CoffeeJTools.CheckRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.AGROLOG_REPORTS_FOLDER_PATH);
            _grainbarReportsFolderBrowserViewModel.Path = CoffeeJTools.CheckRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.GRAINBAR_REPORTS_FOLDER_PATH);
            _agrologDataFolderBrowserViewModel.Path = CoffeeJTools.CheckRegistrySettings(Constants.ACTIVE_AGROLOG_DATA_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.APPLICATION_AGROLOG_DATA_FILE_PATH);
            _grainbarDataFolderBrowserViewModel.Path = CoffeeJTools.CheckRegistrySettings(Constants.ACTIVE_GRAINBAR_DATA_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.APPLICATION_GRAINBAR_DATA_FILE_PATH);
            IsAgrologDataCollect = CoffeeJTools.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsGrainbarDataCollect = CoffeeJTools.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsAutostart = CoffeeJTools.CheckRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsDataSubstitution = CoffeeJTools.CheckRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, false);
        }

        private void ManualInitializing()
        {
            LogMaker.newMessage += LogMaker_newMessage;
            AgrologFolderBrowsButt.Tag = ProgramType.Agrolog;
            GrainbarFolderBrowsButt.Tag = ProgramType.Grainbar;
            SettingsShow += UIWindow_onSettingsShow;
            AboutShow += UIWindow_AboutShow;
            AboutHide += UIWindow_AboutHide;
            CreateReportShow += UIWindow_CreateReportShow;
            CreateReportHide += UIWindow_CreateReportHide;
            PropertyChanged += UIWindow_PropertyChanged;
            LogCollection.CollectionChanged += LogCollection_CollectionChanged;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var window = Application.Current.MainWindow as NewDataWatcherWindow;
                if (window == null) return;
                window.PropertyChanged += NewDataWatcherWindow_PropertyChanged;
                AgrologDataHandlingPermission = window.AgrologDataHandlingPermission;
                GrainbarDataHandlingPermission = window.GrainbarDataHandlingPermission;
            }));

            _agrologReportsFolderBrowserViewModel = new FolderBrowserViewModel(FolderBrowserViewModelType.AgrologReports)
            {
                CreateNewFolderInvolve = true
            };
            _grainbarReportsFolderBrowserViewModel = new FolderBrowserViewModel(FolderBrowserViewModelType.GrainbarReports)
            {
                CreateNewFolderInvolve = true
            };
            _agrologDataFolderBrowserViewModel = new FolderBrowserViewModel(FolderBrowserViewModelType.AgrologData)
            {
                CreateNewFolderInvolve = false
            };
            _grainbarDataFolderBrowserViewModel = new FolderBrowserViewModel(FolderBrowserViewModelType.GrainbarData)
            {
                CreateNewFolderInvolve = true
            };

            AgrologFilesPathTextBox.DataContext = _agrologReportsFolderBrowserViewModel;
            GrainbarFilesPathTextBox.DataContext = _grainbarReportsFolderBrowserViewModel;
            AgrologPeriodButton.DataContext = _agrologDataFolderBrowserViewModel;
            GrainbarPeriodButton.DataContext = _grainbarDataFolderBrowserViewModel;

            _agrologReportsFolderBrowserViewModel.PropertyChanged += FolderBrowserViewModel_PropertyChanged;
            _grainbarReportsFolderBrowserViewModel.PropertyChanged += FolderBrowserViewModel_PropertyChanged;
            _agrologDataFolderBrowserViewModel.PropertyChanged += FolderBrowserViewModel_PropertyChanged;
            _grainbarDataFolderBrowserViewModel.PropertyChanged += FolderBrowserViewModel_PropertyChanged;
            FolderBrowsTreeView.Items.Clear();

        }

        private void UIwindow_ChooseDataForArchiveHide(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task WriteReport<T>(string dataFolderPath, string dataFileName, string reportPath,
            string reportFileName) where T : ITermometer
        {
            List<T> reportData = null;
            try
            {
                LogMaker.Log($"Чтение данных из файла \"{dataFileName}\"", false);

                var app = Application.Current;
                app.Dispatcher.Invoke(new Action(() => DataAccessChanged<T>(true)));
                reportData = await DataWorker.ReadBinaryAsync<T>(dataFolderPath, dataFileName);
                app.Dispatcher.Invoke(new Action(() => DataAccessChanged<T>(false)));

                LogMaker.Log($"Формирование отчета \"{reportFileName}\"", false);
                await DataWorker.WriteExcelReportAsync<T>(reportPath, reportFileName, reportData);
                LogMaker.Log($"Отчет \"{reportFileName}\" сформирован успешно.", false);
            }
            catch (WriteReportException ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(ReportFileException))
                {
                    LogMaker.Log("Файл отчета не существует. Необходимо создать новый.", true);
                    RaiseEvent(new RoutedEventArgs(UIwindow.CreateReportShowEvent, this));
                    _createReportCancellationToken = new CancellationTokenSource();
                    _createReportResetEvent.Reset();
                    //ThreadPool.QueueUserWorkItem(async (state) => await CreateNewReport<T>(reportPath, reportFileName, reportData));
                    await CreateNewReportAsync<T>(reportPath, reportFileName, reportData);
                }
                else
                    LogMaker.Log($"Не получилось сформировать отчет \"{reportFileName}\", cм. Error.log.", true);
                ExceptionHandler.Handle(ex, false);
            }
            catch (Exception)
            {
                LogMaker.Log($"Не получилось сформировать отчет \"{reportFileName}\", cм. Error.log.", true);
            }
        }

        private Task CreateNewReportAsync<T>(string reportPath, string reportFileName, List<T> reportData)
            where T : ITermometer
        {
            return Task.Factory.StartNew((() => CreateNewReport<T>(reportPath, reportFileName, reportData)));
        }

        private void CreateNewReport<T>(string reportPath, string reportFileName, List<T> reportData) where T : ITermometer
        {
            _createReportResetEvent.WaitOne();
            if (_createReportCancellationToken.Token.IsCancellationRequested)
                return;
            LogMaker.InvokedLog($"Создаю файл отчета \"{reportFileName}\".", false, this.Dispatcher);
            string tempExcelTemplateName = Constants.APPLICATION_DIRECTORY.PathFormatter() + Constants.EXCEL_TEMPLATE_REPORT_TEMP_NAME;

            try
            {
                CoffeeJTools.CopyResource(Constants.EXCEL_TEMPLATE_REPORT_NAME, tempExcelTemplateName);
                var tempExcelTemplate = new FileInfo(tempExcelTemplateName);
                DataWorker.CreateNewExcelReport<T>(reportPath, reportFileName, Constants.APPLICATION_DIRECTORY, Constants.EXCEL_TEMPLATE_REPORT_TEMP_NAME, reportData);
                tempExcelTemplate.Delete();
                LogMaker.InvokedLog($"Отчет \"{reportFileName}\" сформирован успешно.", false, this.Dispatcher);
            }
            catch (Exception ex)
            {
                LogMaker.InvokedLog($"Ошибка создания отчета \"{reportFileName}\".", true, this.Dispatcher);
                ExceptionHandler.Handle(ex, false);
            }

        }

        private Task OpenPlotAsync<T>(string dataFolderPath, string dataFileName) where T : ITermometer
        {
            var tcs = new TaskCompletionSource<object>();
            var plotThread = new Thread(() =>
            {
                try
                {
                    OpenPlotCallBack<T>(dataFolderPath, dataFileName);
                    tcs.SetResult(null);

                }
                catch (PlotDataException ex)
                {
                    LogMaker.InvokedLog(
                        $"Не достаточно данных для построения графика \"{CoffeeJTools.GetProgramName<T>()}\", cм. Error.log.", true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    tcs.SetResult(null);
                    //tcs.SetException(ex);
                }
                catch (Exception ex)
                {
                    LogMaker.InvokedLog(
                        $"Не получилось построить график \"{CoffeeJTools.GetProgramName<T>()}\", cм. Error.log.", true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    tcs.SetResult(null);
                    //tcs.SetException(ex);
                }
            });

            plotThread.SetApartmentState(ApartmentState.STA);
            plotThread.IsBackground = true;
            plotThread.Name = "Plot Thread";
            plotThread.Start();
            return tcs.Task;
        }


        private void SaveReportsPathToRegistry(FolderBrowserViewModelType viewModelType)
        {
            string regKey;
            FolderBrowserViewModel viewModel;

            switch (viewModelType)
            {
                case FolderBrowserViewModelType.AgrologReports:
                    regKey = Constants.AGROLOG_REPORTS_PATH_REGKEY;
                    viewModel = _agrologReportsFolderBrowserViewModel;
                    break;
                case FolderBrowserViewModelType.GrainbarReports:
                    regKey = Constants.GRAINBAR_REPORTS_PATH_REGKEY;
                    viewModel = _grainbarReportsFolderBrowserViewModel;
                    break;
                case FolderBrowserViewModelType.AgrologData:
                    regKey = Constants.ACTIVE_AGROLOG_DATA_PATH_REGKEY;
                    viewModel = _agrologDataFolderBrowserViewModel;
                    break;
                case FolderBrowserViewModelType.GrainbarData:
                    regKey = Constants.ACTIVE_GRAINBAR_DATA_PATH_REGKEY;
                    viewModel = _grainbarDataFolderBrowserViewModel;
                    break;
                default:
                    return;
            }

            try //сохраним в реестре последний выбранный путь
            {
                CoffeeJTools.SaveRegistrySettings(regKey, Constants.SETTINGS_LOCATION, viewModel.Path);
            }
            catch (InvalidOperationException ex)
            {
                LogMaker.Log("Невозможно сохранить настройки в реестр. См. Error.log", true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private void StartDataCollect(FolderBrowserViewModelType folderBrowserViewModelType)
        {
            if (folderBrowserViewModelType == FolderBrowserViewModelType.AgrologReports)
            {
                if (AgrologDataChb.IsChecked != null && AgrologDataChb.IsChecked.Value)
                {
                    AgrologDataChb.IsChecked = false;
                    AgrologDataChb.IsChecked = true;
                }
            }
            else if (folderBrowserViewModelType == FolderBrowserViewModelType.GrainbarReports)
                if (GrainbarDataChb.IsChecked != null && GrainbarDataChb.IsChecked.Value)
                {
                    GrainbarDataChb.IsChecked = false;
                    GrainbarDataChb.IsChecked = true;
                }
        }

        //----------------------------------------------------------------------------------------------------------
        #region Callbacks

        private void OpenPlotCallBack<T>(string dataFolderPath, string dataFileName) where T : ITermometer
        {
            try
            {
                LogMaker.InvokedLog($"Чтение данных из файла \"{dataFileName}\"", false, this.Dispatcher);

                var app = Application.Current;
                app.Dispatcher.Invoke(new Action(() => DataAccessChanged<T>(true)));

                var reportData = DataWorker.ReadBinary<T>(dataFolderPath, dataFileName);

                LogMaker.InvokedLog($"Построение графика \"{CoffeeJTools.GetProgramName<T>()}\"", false, this.Dispatcher);

                var data = reportData.Select(t => t as Termometer).ToList();

                var func = new Func<MainPlotWindow>(() =>
                {
                    var p = new MainPlotWindow(data);
                    p.Show();
                    p.Closed += (sender, e) => ClosePlotCallback(CoffeeJTools.GetProgramName<T>(), ref p);
                    return p;
                });

                if (typeof(T) == typeof(TermometerAgrolog))
                    _agrologPlot = (_agrologPlot == null || _agrologPlot.Dispatcher.HasShutdownFinished) ? func() : null;
                else if (typeof(T) == typeof(TermometerGrainbar))
                    _grainbarPlot = (_grainbarPlot == null || _grainbarPlot.Dispatcher.HasShutdownFinished) ? func() : null;

                System.Windows.Threading.Dispatcher.Run();

            }
            catch (ThreadAbortException ex)
            {
                if (typeof(T) == typeof(TermometerAgrolog))
                {
                    _agrologPlot?.Close();
                    _agrologPlot?.Dispatcher.InvokeShutdown();
                }
                else if (typeof(T) == typeof(TermometerGrainbar))
                {
                    _grainbarPlot?.Close();
                    _grainbarPlot?.Dispatcher.InvokeShutdown();
                }
                throw new Exception("Abort thread Exception", ex);

            }
            finally
            {
                var app = Application.Current;
                app?.Dispatcher?.Invoke(new Action(() => DataAccessChanged<T>(false)));
            }
        }

        private void ClosePlotCallback(string plotName, ref MainPlotWindow plotWindow)
        {
            LogMaker.InvokedLog($"Закрытие окна графика \"{plotName}\"", false, this.Dispatcher);
            plotWindow.Dispatcher.InvokeShutdown();
            plotWindow = null;
        }

        private void LogMaker_newMessage(string message, DateTime time, bool isError)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_logDate.Day != time.Day)
                {
                    _logDate = time;
                    LogCollection.Add(new LogEntry
                    {
                        DateTime = time,
                        DatePattern = "dd.MM.yyyy",
                    });
                }
                Brush messageColor = isError
                    ? Brushes.Red
                    : (SolidColorBrush)new BrushConverter().ConvertFrom("#EFEFEF");

                _logIndex++;

                LogCollection.Add(new LogEntry
                {
                    DateTime = time,
                    DatePattern = "HH:mm:ss",
                    Index = _logIndex.ToString(),
                    Message = message,
                    MessageColor = messageColor
                });
            }));

        }

        private void FolderBrowsButt_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = FolderBrowsGrid.DataContext as FolderBrowserViewModel;
            if (dataContext != null && dataContext.Active) return;

            var button = sender as Button;
            if (button != null)
            {
                if ((ProgramType)button.Tag == ProgramType.Agrolog)
                {
                    FolderBrowsGrid.DataContext = _agrologReportsFolderBrowserViewModel;
                    _agrologReportsFolderBrowserViewModel.Active = true;
                }
                else if ((ProgramType)button.Tag == ProgramType.Grainbar)
                {
                    FolderBrowsGrid.DataContext = _grainbarReportsFolderBrowserViewModel;
                    _grainbarReportsFolderBrowserViewModel.Active = true;
                }
            }
        }

        private void PeriodButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = FolderBrowsGrid.DataContext as FolderBrowserViewModel;
            if (dataContext != null && dataContext.Active) return;

            var button = sender as Button;
            if (button == AgrologPeriodButton)
            {
                FolderBrowsGrid.DataContext = _agrologDataFolderBrowserViewModel;
                _agrologDataFolderBrowserViewModel.Active = true;
            }
            else if (button == GrainbarPeriodButton)
            {
                FolderBrowsGrid.DataContext = _grainbarDataFolderBrowserViewModel;
                _grainbarDataFolderBrowserViewModel.Active = true;
            }
        }

        private static void DataAccessChanged<T>(bool value) where T : ITermometer
        {
            var app = Application.Current;
            if (!Equals(Thread.CurrentThread, app.Dispatcher.Thread))
                throw new Exception("Wrong thread.");

            var window = app.MainWindow as NewDataWatcherWindow;
            if (window == null) return;

            if (value)
                window.LockDataAccess<T>();
            else
                window.UnLockDataAccess<T>();
        }

        private async void ReportButton_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;
            if (button == null) return;

            if (Equals(button, AgrologButton))
            {
                AgrologReportPermission = false;
                var task = WriteReport<TermometerAgrolog>(
                    System.IO.Path.GetDirectoryName(_agrologDataFolderBrowserViewModel.Path),
                    System.IO.Path.GetFileName(_agrologDataFolderBrowserViewModel.Path),
                    Constants.EXCEL_REPORT_FOLDER_PATH,
                    CoffeeJTools.MakeExcelReportFileNameFromDataFilePath(_agrologDataFolderBrowserViewModel.Path));
                await task.AwaitCriticalTask();
                AgrologReportPermission = true;
            }
            else if (Equals(button, GrainbarButton))
            {
                GrainbarReportPermission = false;
                var task = WriteReport<TermometerGrainbar>(
                    System.IO.Path.GetDirectoryName(_grainbarDataFolderBrowserViewModel.Path),
                    System.IO.Path.GetFileName(_grainbarDataFolderBrowserViewModel.Path),
                    Constants.EXCEL_REPORT_FOLDER_PATH,
                    CoffeeJTools.MakeExcelReportFileNameFromDataFilePath(_grainbarDataFolderBrowserViewModel.Path));
                await task.AwaitCriticalTask();
                GrainbarReportPermission = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimizeButt_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Program_Closed(object sender, EventArgs e)
        {

        }

        void UIWindow_onSettingsShow(object sender, RoutedEventArgs e)
        {
            IsSettingsGridOnForm = true;
        }


        private void UIWindow_AboutShow(object sender, RoutedEventArgs e)
        {
            IsAboutOnForm = true;
        }


        private void UIWindow_AboutHide(object sender, RoutedEventArgs e)
        {
            IsAboutOnForm = false;
        }


        void UIWindow_CreateReportShow(object sender, RoutedEventArgs e)
        {
            IsCreateReportWindowShow = true;
        }

        void UIWindow_CreateReportHide(object sender, RoutedEventArgs e)
        {
            IsCreateReportWindowShow = false;
        }

        private void CreateReportYesButton_Click(object sender, RoutedEventArgs e)
        {
            _createReportResetEvent.Set();
            RaiseEvent(new RoutedEventArgs(CreateReportHideEvent, this));
        }

        private void CreateReportNoButton_Click(object sender, RoutedEventArgs e)
        {
            _createReportCancellationToken.Cancel();
            _createReportResetEvent.Set();
            RaiseEvent(new RoutedEventArgs(CreateReportHideEvent, this));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    CoffeeJTools.SaveRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, IsAgrologDataCollect);
                    CoffeeJTools.SaveRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, IsGrainbarDataCollect);
                    CoffeeJTools.SaveRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, IsAutostart);
                    CoffeeJTools.SaveRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, IsDataSubstitution);
                }
                catch (InvalidOperationException ex)
                {
                    LogMaker.InvokedLog("Невозможно сохранить настройки в реестр. См. Error.log", true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                }

                LogMaker.InvokedLog("Настройки сохранены", false, this.Dispatcher);
                IsSettingsGridOnForm = false;
            });
        }

        private void FakeTemp_Loaded(object sender, RoutedEventArgs e)
        {
            AgrologFilesPathTextBox.CaretIndex = AgrologFilesPathTextBox.Text.Length;
            var rect = AgrologFilesPathTextBox.GetRectFromCharacterIndex(AgrologFilesPathTextBox.CaretIndex);
            AgrologFilesPathTextBox.ScrollToHorizontalOffset(rect.Right);

            GrainbarFilesPathTextBox.CaretIndex = GrainbarFilesPathTextBox.Text.Length;
            var rect2 = GrainbarFilesPathTextBox.GetRectFromCharacterIndex(GrainbarFilesPathTextBox.CaretIndex);
            GrainbarFilesPathTextBox.ScrollToHorizontalOffset(rect2.Right);
        }

        private void FolderBrowsPlusButt_Click(object sender, RoutedEventArgs e)
        {
            //NewFolderBox.Text = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
            //NewFolderBox.Focus();
        }

        private async void NewFolderTb_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    var textBox = (sender as TextBox);
            //    if (textBox != null)
            //    {
            //        var text = textBox.Text;
            //        DirectoryInfo newDir = CreateNewFolder(text);
            //        if (newDir == null)
            //        {
            //            textBox.Text = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
            //            textBox.SelectAll();
            //        }
            //        if (newDir != null) await FileBrowsTreeViewDirExpandAsync(newDir.FullName, FileBrowsTreeView.Items);
            //    }

            //    FileBrowsTreeView.Focus();
            //}
        }

        private void FilesPathTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            var folderBrowserViewModel = textBox.Name == "AgrologFilesPathTextBox"
                ? _agrologReportsFolderBrowserViewModel
                : _grainbarReportsFolderBrowserViewModel;

            if (textBox.Text == folderBrowserViewModel.Path)
                return;
            folderBrowserViewModel.Path = textBox.Text;

            SaveReportsPathToRegistry(folderBrowserViewModel.Type);
            StartDataCollect(folderBrowserViewModel.Type);
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            string reportPath = Constants.EXCEL_REPORT_FOLDER_PATH.PathFormatter();
            string path = (sender as Button)?.Name == "AShowExcelBut"
                ? _agrologDataFolderBrowserViewModel.Path
                : _grainbarDataFolderBrowserViewModel.Path;
            string reportName = CoffeeJTools.MakeExcelReportFileNameFromDataFilePath(path);
            reportPath += reportName;

            try
            {
                Process.Start(reportPath);
                LogMaker.Log($"Отчет {reportName} запущен.", false);
            }
            catch (FileNotFoundException ex)
            {
                LogMaker.Log($"Файл отчета {reportName} не существует.", true);
                ExceptionHandler.Handle(ex, false);
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Ошибка открытия файла отчета {reportName}.", true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private async void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || AgrologPlotButton == null) return;
            button.IsEnabled = false;

            if (Equals(button, AgrologPlotButton))
            {
                string fileName = System.IO.Path.GetFileName(_agrologDataFolderBrowserViewModel.Path);
                string directoryName = System.IO.Path.GetDirectoryName(_agrologDataFolderBrowserViewModel.Path);
                await OpenPlotAsync<TermometerAgrolog>(directoryName, fileName);
            }
            else if (Equals(button, GrainbarPlotButton))
            {
                string fileName = System.IO.Path.GetFileName(_grainbarDataFolderBrowserViewModel.Path);
                string directoryName = System.IO.Path.GetDirectoryName(_grainbarDataFolderBrowserViewModel.Path);
                await OpenPlotAsync<TermometerGrainbar>(directoryName, fileName);
            }
            button.IsEnabled = true;
        }

        private void AboutOKButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(AboutHideEvent, this));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            LogMaker.newMessage -= LogMaker_newMessage;
            base.OnClosing(e);
        }

        private void UIWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.EqualsAny(nameof(IsAgrologDataCollect), nameof(IsGrainbarDataCollect))) return;
            string propertyName = e.PropertyName;
            bool value = (bool)sender.GetPropertyValue(e.PropertyName);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var newDataWatcherWindow = Application.Current.MainWindow as NewDataWatcherWindow;
                if (Equals(propertyName, nameof(IsAgrologDataCollect)))
                    newDataWatcherWindow?.WatcherInitChange<TermometerAgrolog>(value);
                else
                    newDataWatcherWindow?.WatcherInitChange<TermometerGrainbar>(value);
            }));
        }

        private void NewDataWatcherWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AgrologDataHandlingPermission))
                AgrologDataHandlingPermission = (sender as NewDataWatcherWindow).AgrologDataHandlingPermission;
            if (e.PropertyName == nameof(GrainbarDataHandlingPermission))
                GrainbarDataHandlingPermission = (sender as NewDataWatcherWindow).GrainbarDataHandlingPermission;
        }

        private void FolderBrowserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var folderBrowserViewModel = sender as FolderBrowserViewModel;
            if (folderBrowserViewModel == null) return;

            if (e.PropertyName == "Active")
            {
                IsFolderBrowsTreeOnForm = _agrologReportsFolderBrowserViewModel.Active ||
                                          _grainbarReportsFolderBrowserViewModel.Active ||
                                          _agrologDataFolderBrowserViewModel.Active ||
                                          _grainbarDataFolderBrowserViewModel.Active;
                if (folderBrowserViewModel.Active) return;

                SaveReportsPathToRegistry(folderBrowserViewModel.Type);
                StartDataCollect(folderBrowserViewModel.Type);
            }
        }

        private void LogCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(propertyName: nameof(LogCollection));
        }

        private void FolderBrowserShowCompleted(object sender, EventArgs e)
        {
            var viewModel = FolderBrowsGrid.DataContext as FolderBrowserViewModel;
            if (viewModel == null || !viewModel.Active) return;
            if (viewModel.ContinueExploreCommand.CanExecute(null))
                viewModel.ContinueExploreCommand.Execute(null);
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите переместить текущие данные в архив и начать новый период?", "В архив",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
                return;
            RaiseEvent(new RoutedEventArgs(UIwindow.ChooseDataForArchiveHideEvent, this));

            var button = sender as Button;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var newDataWatcherWindow = Application.Current.MainWindow as NewDataWatcherWindow;

                if (button == ArchiveAgrologButton)
                {
                    var dataFileInfo = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.AGROLOG_DATA_FILE);
                    newDataWatcherWindow.ArchieveDataFile<TermometerAgrolog>(dataFileInfo);
                }
                else
                {
                    var dataFileInfo = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.GRAINBAR_DATA_FILE);
                    newDataWatcherWindow.ArchieveDataFile<TermometerGrainbar>(dataFileInfo);
                }
            }));
        }

        #endregion
        //------------------------------------------------------------------------------

        #region events
        public static readonly RoutedEvent SettingShowEvent = EventManager.RegisterRoutedEvent(
        nameof(SettingsShow), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler SettingsShow
        {
            add { AddHandler(SettingShowEvent, value); }
            remove { RemoveHandler(SettingShowEvent, value); }
        }

        public static readonly RoutedEvent CreateReportShowEvent = EventManager.RegisterRoutedEvent(
        nameof(CreateReportShow), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler CreateReportShow
        {
            add { AddHandler(CreateReportShowEvent, value); }
            remove { RemoveHandler(CreateReportShowEvent, value); }
        }

        public static readonly RoutedEvent CreateReportHideEvent = EventManager.RegisterRoutedEvent(
        nameof(CreateReportHide), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler CreateReportHide
        {
            add { AddHandler(CreateReportHideEvent, value); }
            remove { RemoveHandler(CreateReportHideEvent, value); }
        }

        public static readonly RoutedEvent AboutShowEvent = EventManager.RegisterRoutedEvent(
        nameof(AboutShow), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler AboutShow
        {
            add { AddHandler(AboutShowEvent, value); }
            remove { RemoveHandler(AboutShowEvent, value); }
        }

        public static readonly RoutedEvent AboutHideEvent = EventManager.RegisterRoutedEvent(
        nameof(AboutHide), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler AboutHide
        {
            add { AddHandler(AboutHideEvent, value); }
            remove { RemoveHandler(AboutHideEvent, value); }
        }

        public static readonly RoutedEvent FolderBrowserShowEvent = EventManager.RegisterRoutedEvent(
        nameof(FolderBrowserShow), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler FolderBrowserShow
        {
            add { AddHandler(FolderBrowserShowEvent, value); }
            remove { RemoveHandler(FolderBrowserShowEvent, value); }
        }

        public static readonly RoutedEvent ChooseDataForArchiveHideEvent = EventManager.RegisterRoutedEvent(
 nameof(ChooseDataForArchiveHide), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler ChooseDataForArchiveHide
        {
            add { AddHandler(ChooseDataForArchiveHideEvent, value); }
            remove { RemoveHandler(ChooseDataForArchiveHideEvent, value); }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region fields

        //на форме ли окно выбора файлов
        public bool IsSettingsGridOnForm;
        public bool IsCreateReportWindowShow;
        public bool IsAboutOnForm;
        public bool IsChooseDataArchiveOnForm;

        private bool _isFolderBrowsTreeOnForm;
        //private string _agrologReportsPath;
        //private string _grainbarReportsPath;
        private readonly ManualResetEvent _createReportResetEvent = new ManualResetEvent(false);
        private CancellationTokenSource _createReportCancellationToken;
        private MainPlotWindow _agrologPlot;
        private MainPlotWindow _grainbarPlot;

        private bool _isAgrologDataCollect;
        private bool _isGrainbarDataCollect;
        private bool _isAutostart = true;
        private bool _isDataSubstitution;
        private bool _agrologDataHandlingPermission = true;
        private bool _grainbarDataHandlingPermission = true;
        private bool _agrologReportPermission = true;
        private bool _grainbarReportPermission = true;
        private bool _agrologPlotPermission = true;
        private bool _grainbarPlotPermission = true;
        private Visibility _progressBarVisibility = Visibility.Hidden;

        private int _logIndex = 0;
        private DateTime _logDate = DateTime.Today.AddDays(-1);

        private FolderBrowserViewModel _agrologReportsFolderBrowserViewModel;
        private FolderBrowserViewModel _grainbarReportsFolderBrowserViewModel;
        private FolderBrowserViewModel _agrologDataFolderBrowserViewModel;
        private FolderBrowserViewModel _grainbarDataFolderBrowserViewModel;



        #endregion

        #region properties

        public bool IsFolderBrowsTreeOnForm
        {
            get { return _isFolderBrowsTreeOnForm; }

            set
            {
                if (_isFolderBrowsTreeOnForm != value)
                {
                    _isFolderBrowsTreeOnForm = value;
                    if (value)
                    {
                        RaiseEvent(new RoutedEventArgs(UIwindow.FolderBrowserShowEvent, this));
                        FolderBrowsTreeView.Focus();
                    }
                }
            }
        }

        public bool IsAgrologDataCollect
        {
            get { return _isAgrologDataCollect; }
            set
            {
                if (_isAgrologDataCollect == value)
                    return;

                _isAgrologDataCollect = value;
                NotifyPropertyChanged();
            }

        }

        public bool IsGrainbarDataCollect
        {
            get { return _isGrainbarDataCollect; }
            set
            {
                if (_isGrainbarDataCollect == value)
                    return;

                _isGrainbarDataCollect = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsAutostart
        {
            get { return _isAutostart; }
            set
            {
                if (_isAutostart == value)
                    return;

                _isAutostart = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsDataSubstitution
        {
            get { return _isDataSubstitution; }
            set
            {
                if (_isDataSubstitution == value)
                    return;

                _isDataSubstitution = value;
                NotifyPropertyChanged();
            }
        }

        public bool AgrologDataHandlingPermission
        {
            get { return _agrologDataHandlingPermission; }
            set
            {
                if (_agrologDataHandlingPermission == value)
                    return;

                _agrologDataHandlingPermission = value;
                ProgressBarVisibility = value & GrainbarDataHandlingPermission &
                                        AgrologReportPermission & GrainbarReportPermission &
                                        AgrologPlotPermission & GrainbarPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public bool GrainbarDataHandlingPermission
        {
            get { return _grainbarDataHandlingPermission; }
            set
            {
                if (_grainbarDataHandlingPermission == value)
                    return;

                _grainbarDataHandlingPermission = value;
                ProgressBarVisibility = value & AgrologDataHandlingPermission &
                                        AgrologReportPermission & GrainbarReportPermission &
                                        AgrologPlotPermission & GrainbarPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public bool AgrologReportPermission
        {
            get { return _agrologReportPermission; }
            set
            {
                if (_agrologReportPermission == value)
                    return;

                _agrologReportPermission = value;
                ProgressBarVisibility = value & AgrologDataHandlingPermission &
                                        GrainbarDataHandlingPermission & GrainbarReportPermission &
                                        AgrologPlotPermission & GrainbarPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public bool GrainbarReportPermission
        {
            get { return _grainbarReportPermission; }
            set
            {
                if (_grainbarReportPermission == value)
                    return;

                _grainbarReportPermission = value;
                ProgressBarVisibility = value & AgrologDataHandlingPermission &
                                        GrainbarDataHandlingPermission & AgrologReportPermission &
                                        AgrologPlotPermission & GrainbarPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public bool AgrologPlotPermission
        {
            get { return _agrologPlotPermission; }
            set
            {
                if (_agrologPlotPermission == value)
                    return;

                _agrologPlotPermission = value;
                ProgressBarVisibility = value & AgrologDataHandlingPermission &
                                        GrainbarDataHandlingPermission & AgrologReportPermission &
                                        GrainbarReportPermission & GrainbarPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public bool GrainbarPlotPermission
        {
            get { return _grainbarPlotPermission; }
            set
            {
                if (_grainbarPlotPermission == value)
                    return;

                _grainbarPlotPermission = value;
                ProgressBarVisibility = value & AgrologDataHandlingPermission &
                                        GrainbarDataHandlingPermission & AgrologReportPermission &
                                        GrainbarReportPermission & AgrologPlotPermission ? Visibility.Hidden : Visibility.Visible;
                NotifyPropertyChanged();
            }
        }

        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set
            {
                if (_progressBarVisibility == value)
                    return;

                _progressBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<LogEntry> LogCollection { get; set; } = new ObservableCollection<LogEntry>();

        public string AssemblyVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName: assembly.Location);
                return fileVersionInfo.FileVersion;
            }
        }

        public string ApplicationName => Constants.APPLICATION_NAME;

        public string Description => Constants.APPLICATION_DESCRIPTION;

        #endregion

    }
}
