using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using NLog;
using System.Threading;
using System.ComponentModel;
using CoffeeJelly.tempa.Exceptions;
using CoffeeJelly.tempa.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CoffeeJelly.tempa.Controls;
using CoffeeJelly.tempa.FileBrowser.ViewModel;
using CoffeeJelly.tempa.ViewModel;

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
            _agrologFileBrowserViewModel.Path = Internal.CheckRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.AGROLOG_REPORTS_FOLDER_PATH);
            _grainbarFileBrowserViewModel.Path = Internal.CheckRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.GRAINBAR_REPORTS_FOLDER_PATH);
            IsAgrologDataCollect = Internal.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsGrainbarDataCollect = Internal.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsAutostart = Internal.CheckRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsDataSubstitution = Internal.CheckRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, false);
        }

        private void ManualInitializing()
        {
            LogMaker.newMessage += LogMaker_newMessage;
            AgrologFileBrowsButt.Tag = ProgramType.Agrolog;
            GrainbarFileBrowsButt.Tag = ProgramType.Grainbar;
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

            _agrologFileBrowserViewModel = new FileBrowserViewModel(FileBrowserType.Agrolog);
            _grainbarFileBrowserViewModel = new FileBrowserViewModel(FileBrowserType.Grainbar);
            AgrologFilesPathTextBox.DataContext = _agrologFileBrowserViewModel;
            GrainbarFilesPathTextBox.DataContext = _grainbarFileBrowserViewModel;

            _agrologFileBrowserViewModel.PropertyChanged += FileBrowserViewModel_PropertyChanged;
            _grainbarFileBrowserViewModel.PropertyChanged += FileBrowserViewModel_PropertyChanged;
            FileBrowsTreeView.Items.Clear();

        }

        //delete
        //private void FillTreeViewWithRootDrives(ref TreeView treeview)
        //{
        //    treeview.Items.Clear();
        //    foreach (DriveInfo drive in DriveInfo.GetDrives())
        //    {
        //        var item = new TreeViewItem();
        //        var bc = new BrushConverter();
        //        item.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
        //        item.Tag = drive;
        //        item.Header = drive.ToString();
        //        item.Items.Add("*");
        //        treeview.Items.Add(item);
        //    }
        //}

        //private Task FileBrowsTreeViewDirExpandAsync(string path, ItemCollection itemCollection)
        //{
        //    return Task.Factory.StartNew(() => FileBrowsTreeViewDirExpand(path, itemCollection));
        //}

        //delete
        //private void FileBrowsTreeViewDirExpand(string path, ItemCollection itemCollection)
        //{
        //    foreach (TreeViewItem item in itemCollection)
        //    {
        //        DirectoryInfo dir = GetDirectoryInfo(item, true);

        //        var splittedPath = path.Split('\\').ToList();
        //        splittedPath.RemoveAll(string.IsNullOrEmpty);

        //        foreach (string dirName in splittedPath)
        //        {
        //            if (dir.Name.PathFormatter() != dirName.PathFormatter()) continue;
        //            Dispatcher.Invoke(new Action(() =>
        //            {
        //                item.IsExpanded = false;
        //                item.IsExpanded = true;
        //                item.IsSelected = true;
        //            }));
        //            FileBrowsTreeViewDirExpand(path.ReplaceFirst(dirName.PathFormatter(), string.Empty), item.Items);
        //            break;
        //        }
        //    }
        //}

        //delete
        //private void FillTreeViewItemWithDirectories(ref TreeViewItem treeViewItem)
        //{
        //    treeViewItem.Items.Clear();
        //    DirectoryInfo dir = GetDirectoryInfo(treeViewItem, false);
        //    try
        //    {
        //        foreach (DirectoryInfo subDir in dir.GetDirectories())
        //        {
        //            var newItem = new TreeViewItem
        //            {
        //                Tag = subDir,
        //                Header = subDir.ToString()
        //            };
        //            newItem.Items.Add("*");
        //            treeViewItem.Items.Add(newItem);
        //        }
        //    }
        //    catch
        //    {
        //        // ignored
        //    }
        //}

        //delete
        //private DirectoryInfo GetDirectoryInfo(TreeViewItem item, bool anotherThread)
        //{
        //    DirectoryInfo dir;
        //    object tag = anotherThread ? Dispatcher.Invoke(new Func<object>(() => item.Tag)) : item.Tag;

        //    var info = tag as DriveInfo;
        //    if (info != null)
        //    {
        //        DriveInfo drive = info;
        //        dir = drive.RootDirectory;
        //    }
        //    else dir = (DirectoryInfo)tag;

        //    return dir;
        //}

        private DirectoryInfo CreateNewFolder(string folderName)
        {
            //if (string.IsNullOrEmpty(folderName))
            //    return null;
            //var selectedItem = (TreeViewItem)(FileBrowsTreeView.SelectedItem);
            //DirectoryInfo dir = GetDirectoryInfo(selectedItem, false);

            //try
            //{
            //    string path = dir.FullName.PathFormatter() + folderName;
            //    if (!Directory.Exists(path))
            //    {
            //        DirectoryInfo newDir = Directory.CreateDirectory(path);
            //        LogMaker.Log($"Создана новая папка: \"{newDir.FullName}\".", false);
            //        return newDir;
            //    }
            //    return new DirectoryInfo(path);
            //}
            //catch (ArgumentException ex)
            //{
            //    LogMaker.Log("Имя папки содежит недопустимые символы, или содержит только пробелы. См. Error.log", true);
            //    ExceptionHandler.Handle(ex, false);
            //}
            return null;
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
                Internal.CopyResource(Constants.EXCEL_TEMPLATE_REPORT_NAME, tempExcelTemplateName);
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
                        $"Не достаточно данных для построения графика \"{Internal.GetProgramName<T>()}\", cм. Error.log.", true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    tcs.SetResult(null);
                    //tcs.SetException(ex);
                }
                catch (Exception ex)
                {
                    LogMaker.InvokedLog(
                        $"Не получилось построить график \"{Internal.GetProgramName<T>()}\", cм. Error.log.", true, this.Dispatcher);
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


        private void SaveReportsPathToRegistry(ProgramType programType)
        {
            string regKey = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_PATH_REGKEY : Constants.GRAINBAR_REPORTS_PATH_REGKEY;
            string savedValue = programType == ProgramType.Agrolog ? _agrologFileBrowserViewModel.Path : _grainbarFileBrowserViewModel.Path;
            try //сохраним в реестре последний выбранный путь
            {
                Internal.SaveRegistrySettings(regKey, Constants.SETTINGS_LOCATION, savedValue);
            }
            catch (InvalidOperationException ex)
            {
                LogMaker.Log("Невозможно сохранить настройки в реестр. См. Error.log", true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private void StartDataCollect(ProgramType programType)
        {
            if (programType == ProgramType.Agrolog)
            {
                if (AgrologDataChb.IsChecked != null && AgrologDataChb.IsChecked.Value)
                {
                    AgrologDataChb.IsChecked = false;
                    AgrologDataChb.IsChecked = true;
                }
            }
            else if (programType == ProgramType.Grainbar)
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
                app.Dispatcher.Invoke(new Action(() => DataAccessChanged<T>(false)));
                LogMaker.InvokedLog($"Построение графика \"{Internal.GetProgramName<T>()}\"", false, this.Dispatcher);

                var data = reportData.Select(t => t as Termometer).ToList();

                var func = new Func<MainPlotWindow>(() =>
                {
                    var p = new MainPlotWindow(data);
                    p.Show();
                    p.Closed += (sender, e) => ClosePlotCallback(Internal.GetProgramName<T>(), ref p);
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

        }

        private void ClosePlotCallback(string plotName, ref MainPlotWindow plotWindow)
        {
            LogMaker.InvokedLog($"Закрытие окна графика \"{plotName}\"", false, this.Dispatcher);
            plotWindow.Dispatcher.InvokeShutdown();
            plotWindow = null;
        }

        //private void FileBrowsTreeView_Expanded(object sender, RoutedEventArgs e)
        //{
        //    var item = (TreeViewItem)e.OriginalSource;
        //    FillTreeViewItemWithDirectories(ref item);
        //    ScrollViewer scroller = (ScrollViewer)Internal.FindVisualChildElement(this.FileBrowsTreeView, typeof(ScrollViewer));
        //    scroller.ScrollToBottom();
        //    item.BringIntoView();
        //}

        //private void FileBrowsTreeView_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    IsFileBrowsTreeOnForm = false;
        //}

        //private void FileBrowsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    String path = "";
        //    Stack<TreeViewItem> pathstack = Internal.GetNodes(e.NewValue as UIElement);
        //    if (pathstack.Count == 0)
        //        return;

        //    int i = 0;
        //    foreach (TreeViewItem item in pathstack)
        //    {
        //        if (i > 0)
        //            path += item.Header.ToString().PathFormatter();
        //        else
        //            path += item.Header.ToString();
        //        i++;
        //    }
        //    var treeView = sender as TreeView;
        //    if (treeView != null)
        //    {
        //        var tag = (ProgramType)treeView.Tag;
        //        if (tag == ProgramType.Agrolog)
        //            AgrologReportsPath = path;
        //        else
        //            GrainbarReportsPath = path;
        //    }
        //}

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

        private void FileBrowsButt_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = FileBrowsGrid.DataContext as FileBrowserViewModel;
            if (dataContext != null && dataContext.Active) return;

            var button = sender as Button;
            if (button != null)
            {
                if ((ProgramType)button.Tag == ProgramType.Agrolog)
                {

                    //var items = (FileBrowsTreeView.ItemsSource as ItemCollection);
                    //items?.Clear();
                    FileBrowsGrid.DataContext = _agrologFileBrowserViewModel;
                    _agrologFileBrowserViewModel.Active = true;
                }

                else if ((ProgramType)button.Tag == ProgramType.Grainbar)
                {
                    FileBrowsTreeView.Items.Clear();
                    FileBrowsGrid.DataContext = _grainbarFileBrowserViewModel;
                    _grainbarFileBrowserViewModel.Active = true;
                }

                //{
                //    Path = AgrologReportsPath,
                //    Type = ProgramType.Grainbar
                //};

                //FileBrowsTreeView.Tag = button.Tag;
                //FillTreeViewWithRootDrives(ref FileBrowsTreeView);
                //if (FileBrowsTreeView.Items.Count == 0)
                //    return;

                //if ((ProgramType)button.Tag == ProgramType.Agrolog)
                //    await FileBrowsTreeViewDirExpandAsync(AgrologReportsPath, FileBrowsTreeView.Items);
                //else
                //    await FileBrowsTreeViewDirExpandAsync(GrainbarReportsPath, FileBrowsTreeView.Items);
            }

            //var command =(FileBrowsTreeView.DataContext as FileBrowserViewModel).ActivateCommand;
            //command.Execute(null);
        }


        private void FileBrowsOkButt_Click(object sender, RoutedEventArgs e)
        {
            //var programType = (ProgramType)FileBrowsTreeView.Tag;
            //SaveReportsPathToRegistry(programType);
            //StartDataCollect(programType);
            //IsFileBrowsTreeOnForm = false;
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
                    Constants.APPLICATION_DATA_FOLDER_PATH,
                    Constants.AGROLOG_DATA_FILE,
                    Constants.EXCEL_REPORT_FOLDER_PATH,
                    Constants.AGROLOG_EXCEL_REPORT_FILE_NAME);
                await task.AwaitCriticalTask();
                AgrologReportPermission = true;
            }
            else if (Equals(button, GrainbarButton))
            {
                GrainbarReportPermission = false;
                var task = WriteReport<TermometerGrainbar>(
                    Constants.APPLICATION_DATA_FOLDER_PATH,
                    Constants.GRAINBAR_DATA_FILE,
                    Constants.EXCEL_REPORT_FOLDER_PATH,
                    Constants.GRAINBAR_EXCEL_REPORT_FILE_NAME);
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
                    Internal.SaveRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, IsAgrologDataCollect);
                    Internal.SaveRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, IsGrainbarDataCollect);
                    Internal.SaveRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, IsAutostart);
                    Internal.SaveRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, IsDataSubstitution);
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

        private void FileBrowsPlusButt_Click(object sender, RoutedEventArgs e)
        {
            NewFolderBox.Text = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
            NewFolderBox.Focus();
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
            var programType = textBox.Name == "AgrologFilesPathTextBox" ? ProgramType.Agrolog : ProgramType.Grainbar;
            //var propertyName = textBox.Name == "AgrologFilesPathTextBox" ? "AgrologReportsPath" : "GrainbarReportsPath";

            if (programType == ProgramType.Agrolog)
            {
                if (textBox.Text == _agrologFileBrowserViewModel.Path)
                    return;
                _agrologFileBrowserViewModel.Path = textBox.Text;
            }
            else
            {
                if (textBox.Text == _grainbarFileBrowserViewModel.Path)
                    return;
                _grainbarFileBrowserViewModel.Path = textBox.Text;
            }


            SaveReportsPathToRegistry(programType);
            StartDataCollect(programType);
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Constants.EXCEL_REPORT_FOLDER_PATH.PathFormatter();
            filePath += (sender as Button)?.Name == "AShowExcelBut" ? Constants.AGROLOG_EXCEL_REPORT_FILE_NAME : Constants.GRAINBAR_EXCEL_REPORT_FILE_NAME;
            string programName = (sender as Button)?.Name == "AShowExcelBut" ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            try
            {
                Process.Start(filePath);
                LogMaker.Log($"Отчет {programName} запущен.", false);
            }
            catch (FileNotFoundException ex)
            {
                LogMaker.Log($"Файл отчета {programName} не существует.", true);
                ExceptionHandler.Handle(ex, false);
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Ошибка открытия файла отчета {programName}.", true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private async void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || AgrologPlotButton == null) return;
            button.IsEnabled = false;

            if (Equals(button, AgrologPlotButton))
                await OpenPlotAsync<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.AGROLOG_DATA_FILE);
            else if (Equals(button, GrainbarPlotButton))
                await OpenPlotAsync<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.GRAINBAR_DATA_FILE);
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

        private void FileBrowserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fileBrowserViewModel = sender as FileBrowserViewModel;
            if (fileBrowserViewModel == null) return;

            if (e.PropertyName == "Active")
            {
                IsFileBrowsTreeOnForm = _agrologFileBrowserViewModel.Active || _grainbarFileBrowserViewModel.Active;
                if (fileBrowserViewModel.Active) return;

                if (fileBrowserViewModel.Type == FileBrowserType.Agrolog)
                    StartDataCollect(ProgramType.Agrolog);
                else if (fileBrowserViewModel.Type == FileBrowserType.Grainbar)
                    StartDataCollect(ProgramType.Grainbar);
            }
        }

        private void LogCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(propertyName: nameof(LogCollection));
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

        public static readonly RoutedEvent FileBrowserShowEvent = EventManager.RegisterRoutedEvent(
        nameof(FileBrowserShow), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UIwindow));

        public event RoutedEventHandler FileBrowserShow
        {
            add { AddHandler(FileBrowserShowEvent, value); }
            remove { RemoveHandler(FileBrowserShowEvent, value); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region fields

        //на форме ли окно выбора файлов
        public bool IsSettingsGridOnForm;
        public bool IsCreateReportWindowShow;
        public bool IsAboutOnForm;

        private bool _isFileBrowsTreeOnForm;
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

        private FileBrowserViewModel _agrologFileBrowserViewModel;
        private FileBrowserViewModel _grainbarFileBrowserViewModel;
        private FileBrowserViewModel _archiveAgrologDataContext;
        private FileBrowserViewModel _archiveGrainbarDataContext;



        #endregion

        #region properties

        public bool IsFileBrowsTreeOnForm
        {
            get { return _isFileBrowsTreeOnForm; }

            set
            {
                if (_isFileBrowsTreeOnForm != value)
                {
                    _isFileBrowsTreeOnForm = value;
                    if (value)
                    {
                        RaiseEvent(new RoutedEventArgs(UIwindow.FileBrowserShowEvent, this));
                        FileBrowsTreeView.Focus();
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

        private void FileBrowserShowCompleted(object sender, EventArgs e)
        {

            var viewModel = FileBrowsGrid.DataContext as FileBrowserViewModel;
            if (viewModel == null || !viewModel.Active) return;
            if (viewModel.ContinueExploreCommand.CanExecute(null))
                viewModel.ContinueExploreCommand.Execute(null);
        }
    }
}
