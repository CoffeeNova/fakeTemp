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
using tempa.Exceptions;
using tempa.Extensions;

namespace tempa
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            ManualInitializing();
            Settings();

            _log.Info(string.Format("{0} is started successfully.", Constants.APPLICATION_NAME));
        }

        private void Settings()
        {
            #region checked menu items
            #endregion

            AgrologReportsPath = Internal.CheckRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.AGROLOG_REPORTS_FOLDER_PATH);
            GrainbarReportsPath = Internal.CheckRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.GRAINBAR_REPORTS_FOLDER_PATH);
            IsAgrologDataCollect = Internal.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsGrainbarDataCollect = Internal.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsAutostart = Internal.CheckRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsDataSubstitution = Internal.CheckRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, false);
        }

        private void ManualInitializing()
        {
            LogTextBlock.Inlines.Clear();
            LogMaker.newMessage += LogMaker_newMessage;
            AgrologFileBrowsButt.Tag = ProgramType.Agrolog;
            GrainbarFileBrowsButt.Tag = ProgramType.Grainbar;
            SettingsShow += MainWindow_onSettingsShow;
            CreateReportShow += MainWindow_CreateReportShow;
            CreateReportHide += MainWindow_CreateReportHide;
            CheckDataFiles();
        }

        private void CheckDataFiles()
        {
            try
            {
                var datFile = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.AGROLOG_DATA_FILE);
                if (!datFile.Exists)
                    CreateNewDataFile<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter(), Constants.AGROLOG_DATA_FILE);
                datFile = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.GRAINBAR_DATA_FILE);
                if (!datFile.Exists)
                    CreateNewDataFile<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter(), Constants.GRAINBAR_DATA_FILE);
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew((() =>
                {
                    MessageBox.Show(string.Format("Критическая ошибка. Приложение закроется через 5 сек."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.ServiceNotification);
                }));
                Thread.Sleep(3000);
                ExceptionHandler.Handle(ex, true);
            }
        }

        private void CreateNewDataFile<T>(string dataFilePath, string dataFileName) where T : ITermometer
        {
            LogMaker.Log(string.Format("Файл данных {0} не обнаружен. Создаю новый.", dataFilePath.PathFormatter() + dataFileName), false);
            string patternReportText = typeof(T) == typeof(TermometerAgrolog) ? Properties.Resources.AgrologPatternReport : Properties.Resources.GrainbarPatternReport;
            List<T> patternReportList = DataWorker.ReadPatternReport<T>(patternReportText);
            var task = WriteDataFile<T>(patternReportList, dataFilePath, dataFileName, false);
            if (!task.Result)
                throw new InvalidOperationException("Can't create .dat file. Operation of application work is impossible.");
        }

        private bool WatcherInit<T>(ref FileSystemWatcher watcher, string reportsPath, string fileExtension) where T : ITermometer
        {
            string programName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;

            try
            {
                watcher = new FileSystemWatcher(reportsPath);
                WatcherSettings<T>(watcher, fileExtension);
                LogMaker.Log(string.Format("Запуск мониторинга данных \"{0}\".", programName), false);
                return true;
            }
            catch (ArgumentException ex)
            {
                LogMaker.Log(string.Format("Каталог \"{0}\" отчетов {1} не существует.", reportsPath, programName), true);
                ExceptionHandler.Handle(ex, false);
                return false;
            }
        }

        private void WatcherSettings<T>(FileSystemWatcher watcher, string fileExtension) where T : ITermometer
        {
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*." + fileExtension;
            watcher.Created += (sender, e) => FileSystemWatcher_OnCreated<T>(sender, e);
            watcher.InternalBufferSize = 81920;
            watcher.EnableRaisingEvents = true;
        }

        private void DisposeWatcher(FileSystemWatcher watcher, ProgramType programType)
        {
            if (watcher == null)
                return;
            string programName = programType == ProgramType.Agrolog ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            LogMaker.Log(string.Format("Данные {0} из каталога \"{1}\" не принимаются.", programName, watcher.Path), false);
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();

        }

        private async Task CheckDirectoryForNewData<T>(string path, string fileExtension) where T : ITermometer
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                LogMaker.InvokedLog(string.Format("Каталог \"{0}\" не существует, или к нему нет доступа.", path), true, this.Dispatcher);
                return;
            }

            FileInfo[] filesInfo = dirInfo.GetFiles("*." + fileExtension);
            string programName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            string dataFileName = DataFileName<T>();
            DataHandlingLock<T>.SyncLock.WaitOne();
            LogMaker.InvokedLog(string.Format("Начинаю проверку каталога \"{0}\" на наличие новых данных {1}...", path, programName), false, this.Dispatcher);
            var checkList = new List<bool>();
            List<T> dataList = await ReadDataFile<T>(dataFileName);
            if (dataList == null)
                return;

            foreach (var fileInfo in filesInfo)
                checkList.Add(await NewDataVerification<T>(fileInfo.DirectoryName, fileInfo.Name, dataList));

            if (checkList.Any(b => b == true))
            {
                LogMaker.InvokedLog(string.Format("Данные приняты. Сохраняю их в файле \"{0}\".", dataFileName), false, this.Dispatcher);
                await WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, true);
            }
            else
                LogMaker.InvokedLog(string.Format("Новых данных {0} не обнаружено.", programName), false, this.Dispatcher);
            DataHandlingLock<T>.SyncLock.Release();
        }

        private async Task<bool> NewDataVerification<T>(string path, string fileName, List<T> dataList) where T : ITermometer
        {
            string programName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            LogMaker.InvokedLog(string.Format("Обнаружен новый файл \"{0}\" отчета {1}. Начинаем процесс парсинга...", programName, fileName), false, this.Dispatcher);

            List<T> initReportList = await ReadNewReport<T>(path, fileName, programName);
            if (initReportList == null)
                return false;

            bool operationValue = false;
            DateTime initReportDate = initReportList.First().MeasurementDate;
            bool initReportAlreadySaved = dataList.Any(t => t.MeasurementDate == initReportDate);
            if (!initReportAlreadySaved)
            {
                dataList.AddRange(initReportList);
                LogMaker.InvokedLog(string.Format("Данные из файла отчета \"{0}\" новые. Запишу в базу данных", fileName), false, this.Dispatcher);
                operationValue = true;
            }
            else
            {
                string dataFileName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
                LogMaker.InvokedLog(string.Format("Данные из файла отчета \"{0}\" уже существуют в базе данных файла \"{1}\".", fileName, dataFileName), false, this.Dispatcher);
                operationValue = false;
            }
            DeleteFile(path, fileName);
            return operationValue;
        }

        private void DeleteFile(string path, string fileName)
        {
            LogMaker.InvokedLog(string.Format("Удаляю файл \"{0}\".", fileName), false, this.Dispatcher);
            try
            {
                new FileInfo(path.PathFormatter() + fileName).Delete();
            }
            catch (Exception ex)
            {
                LogMaker.InvokedLog(string.Format("Ошибка удаления файла \"{0}\".", fileName), true, this.Dispatcher);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private string DataFileName<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
        }

        private async Task<List<T>> ReadNewReport<T>(string path, string fileName, string programName) where T : ITermometer
        {
            List<T> reportList;
            try
            {
                reportList = await DataWorker.ReadReportAsync<T>(path, fileName);
                if (reportList == null || reportList.Count == 0)
                    throw new InvalidOperationException(string.Format("Parsing file \"{0}\" operation returns empty result.", fileName));
                return reportList;
            }
            catch (Exception ex)
            {
                LogMaker.InvokedLog(string.Format("Парсинг данных файла \"{0}\" завершился неудачно. См. Error.log", fileName), true, this.Dispatcher);
                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }

        private async Task<List<T>> ReadDataFile<T>(string dataFileName) where T : ITermometer
        {
            List<T> dataFile;

            try
            {
                dataFile = await DataWorker.ReadBinaryAsync<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                return dataFile;
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Процесс извлечения данных из файла \"{0}\" завершился неудачно. См. Error.log", dataFileName), true);
                ExceptionHandler.Handle(ex, false);
            }

            return null;
        }

        private async Task<bool> WriteDataFile<T>(List<T> dataList, string dataFilePath, string dataFileName, bool writeAsync) where T : ITermometer
        {
            try
            {
                var orderedDataList = dataList.OrderBy(t => t.MeasurementDate).ToList();
                if (writeAsync)
                    await DataWorker.WriteBinaryAsync<T>(dataFilePath, dataFileName, orderedDataList, false);
                else DataWorker.WriteBinary<T>(dataFilePath, dataFileName, orderedDataList, false);
                return true;
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Процесс записи данных в файл \"{0}\" завершился неудачно. См. Error.log", dataFileName), true);
                ExceptionHandler.Handle(ex, false);
                return false;
            }
        }

        void LogMaker_newMessage(string message, bool isError)
        {
            Run run = new Run(message + Environment.NewLine);
            InlineUIContainer inlineUIContainer = new InlineUIContainer();

            if (isError)
                run.Foreground = Brushes.Red;
            else
                run.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EFEFEF")); ;
            LogTextBlock.Inlines.Add(run);
        }

        private void ErrorLabelTimerCallback(Object state)
        {

        }

        private void FilesPathTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void FilesPathTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {

        }

        private async void FileBrowsButt_Click(object sender, RoutedEventArgs e)
        {
            if (IsFileBrowsTreeOnForm == false)
            {
                var button = sender as Button;
                FileBrowsTreeView.Tag = button.Tag;
                FillTreeViewWithRootDrives(ref FileBrowsTreeView);
                if (FileBrowsTreeView.Items.Count == 0)
                    return;

                if ((ProgramType)button.Tag == ProgramType.Agrolog)
                    await FileBrowsTreeViewDirExpandAsync(AgrologReportsPath, FileBrowsTreeView.Items);
                else
                    await FileBrowsTreeViewDirExpandAsync(GrainbarReportsPath, FileBrowsTreeView.Items);

                IsFileBrowsTreeOnForm = true;
                FileBrowsTreeView.Focus();
            }
        }

        private void FillTreeViewWithRootDrives(ref TreeView treeview)
        {
            treeview.Items.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                var item = new TreeViewItem();
                var bc = new BrushConverter();
                item.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                item.Tag = drive;
                item.Header = drive.ToString();
                item.Items.Add("*");
                treeview.Items.Add(item);
            }
        }

        private Task FileBrowsTreeViewDirExpandAsync(string path, ItemCollection itemCollection)
        {
            return Task.Factory.StartNew(() => FileBrowsTreeViewDirExpand(path, itemCollection));
        }


        private void FileBrowsTreeViewDirExpand(string path, ItemCollection itemCollection)
        {
            for (int i = 0; i < itemCollection.Count; i++)
            {
                TreeViewItem item = (TreeViewItem)itemCollection[i];
                DirectoryInfo dir = GetDirectoryInfo(item, true);


                var splittedPath = path.Split('\\').ToList();
                splittedPath.RemoveAll(p => string.IsNullOrEmpty(p));
                var pathCount = splittedPath.Count;
                foreach (string dirName in splittedPath)
                {
                    if (dir.Name.PathFormatter() == dirName.PathFormatter())
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            item.IsExpanded = false;
                            item.IsExpanded = true;
                            item.IsSelected = true;
                        }));
                        FileBrowsTreeViewDirExpand(path.ReplaceFirst(dirName.PathFormatter(), string.Empty), item.Items);
                        break;
                    }
                }
            }
        }

        private void FillTreeViewItemWithDirectories(ref TreeViewItem treeViewItem)
        {
            var bc = new BrushConverter();
            //treeViewItem.Foreground = (Brush)bc.ConvertFrom("#FFBFB7B7");
            treeViewItem.Items.Clear();
            DirectoryInfo dir = GetDirectoryInfo(treeViewItem, false);
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    var newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    //newItem.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    treeViewItem.Items.Add(newItem);
                }
            }
            catch
            { }
            finally { }
        }


        private DirectoryInfo GetDirectoryInfo(TreeViewItem item, bool anotherThread)
        {
            DirectoryInfo dir;
            object tag = anotherThread == true ? Dispatcher.Invoke(new Func<object>(() => item.Tag)) : item.Tag;

            if (tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)tag;
                dir = drive.RootDirectory;
            }
            else dir = (DirectoryInfo)tag;

            return dir;
        }

        private DirectoryInfo CreateNewFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return null;
            var selectedItem = (TreeViewItem)(FileBrowsTreeView.SelectedItem);
            DirectoryInfo dir = GetDirectoryInfo(selectedItem, false);

            try
            {
                string path = dir.FullName.PathFormatter() + folderName;
                if (!Directory.Exists(path))
                {
                    DirectoryInfo newDir = Directory.CreateDirectory(path);
                    LogMaker.Log(string.Format("Создана новая папка: \"{0}\".", newDir.FullName), false);
                    return newDir;
                }
                return new DirectoryInfo(path);
            }
            catch (ArgumentException ex)
            {
                LogMaker.Log(string.Format("Имя папки содежит недопустимые символы, или содержит только пробелы. См. Error.log"), true);
                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }



        private void FileBrowsTreeView_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)e.OriginalSource;
            FillTreeViewItemWithDirectories(ref item);
            ScrollViewer scroller = (ScrollViewer)Internal.FindVisualChildElement(this.FileBrowsTreeView, typeof(ScrollViewer));
            scroller.ScrollToBottom();
            item.BringIntoView();
        }

        private void FileBrowsTreeView_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FileBrowsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int i = 0;
            String path = "";
            Stack<TreeViewItem> pathstack = Internal.GetNodes(e.NewValue as UIElement);
            if (pathstack.Count == 0)
                return;
            foreach (TreeViewItem item in pathstack)
            {
                if (i > 0)
                    path += item.Header.ToString().PathFormatter();
                else
                    path += item.Header.ToString();
                i++;
            }
            var tag = (ProgramType)(sender as TreeView).Tag;
            if (tag == ProgramType.Agrolog)
                AgrologReportsPath = path;
            else
                GrainbarReportsPath = path;
        }


        private async Task WriteReport<T>(string dataFolderPath, string dataFileName, string reportPath, string reportFileName) where T : ITermometer
        {
            List<T> reportData = null;
            try
            {
                LogMaker.Log(string.Format("Чтение данных из файла \"{0}\"", dataFileName), false);
                reportData = await DataWorker.ReadBinaryAsync<T>(dataFolderPath, dataFileName);
                LogMaker.Log(string.Format("Формирование отчета \"{0}\"", reportFileName), false);
                await DataWorker.WriteExcelReportAsync<T>(reportPath, reportFileName, reportData);
                LogMaker.Log(string.Format("Отчет \"{0}\" сформирован успешно.", reportFileName), false);
            }
            catch (WriteReportException ex)
            {
                if (ex.InnerException.GetType() == typeof(ReportFileException))
                {
                    LogMaker.Log(string.Format("Файл отчета не существует. Необходимо создать новый."), true);
                    RaiseEvent(new RoutedEventArgs(MainWindow.CreateReportShowEvent, this));
                    _createReportCancellationToken = new CancellationTokenSource();
                    _createReportResetEvent.Reset();
                    ThreadPool.QueueUserWorkItem((state) => CreateNewReport<T>(reportPath, reportFileName, reportData));
                }
                else
                    LogMaker.Log(string.Format("Не получилось сформировать отчет \"{0}\", cм. Error.log.", reportFileName), true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private void CreateNewReport<T>(string reportPath, string reportFileName, List<T> reportData) where T : ITermometer
        {
            _createReportResetEvent.WaitOne();
            if (_createReportCancellationToken.Token.IsCancellationRequested)
                return;
            LogMaker.InvokedLog(string.Format("Создаю файл отчета \"{0}\".", reportFileName), false, this.Dispatcher);
            string tempExcelTemplateName = Constants.APPLICATION_DIRECTORY.PathFormatter() + Constants.EXCEL_TEMPLATE_REPORT_TEMP_NAME;

            try
            {
                Internal.CopyResource(Constants.EXCEL_TEMPLATE_REPORT_NAME, tempExcelTemplateName);
                var tempExcelTemplate = new FileInfo(tempExcelTemplateName);
                DataWorker.CreateNewExcelReport<T>(reportPath, reportFileName, Constants.APPLICATION_DIRECTORY, Constants.EXCEL_TEMPLATE_REPORT_TEMP_NAME, reportData);
                tempExcelTemplate.Delete();
                LogMaker.InvokedLog(string.Format("Отчет \"{0}\" сформирован успешно.", reportFileName), false, this.Dispatcher);
            }
            catch (Exception ex)
            {
                LogMaker.InvokedLog(string.Format("Ошибка создания отчета \"{0}\".", reportFileName), true, this.Dispatcher);
                ExceptionHandler.Handle(ex, false);
            }

        }

        #region Callbacks

        private void FileBrowsOkButt_Click(object sender, RoutedEventArgs e)
        {
            if (FileBrowsTreeView.SelectedValuePath != null)
            {
                try //сохраним в реестре последний выбранный путь
                {
                    Internal.SaveRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, AgrologReportsPath);
                    Internal.SaveRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, GrainbarReportsPath);
                }
                catch (InvalidOperationException ex)
                {
                    LogMaker.Log(string.Format("Невозможно сохранить настройки в реестр. См. Error.log"), true);
                    ExceptionHandler.Handle(ex, false);
                }

            }
            IsFileBrowsTreeOnForm = false;
        }

        private async void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (button == AgrologButton)
                await WriteReport<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.AGROLOG_DATA_FILE, Constants.EXCEL_REPORT_FOLDER_PATH, Constants.AGROLOG_EXCEL_REPORT_FILE_NAME);
            else if (button == GrainbarButton)
                await WriteReport<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.GRAINBAR_DATA_FILE, Constants.EXCEL_REPORT_FOLDER_PATH, Constants.GRAINBAR_EXCEL_REPORT_FILE_NAME);
            button.IsEnabled = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimizeButt_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void FileBrowsGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void FileBrowsGrid_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Program_Closed(object sender, EventArgs e)
        {

        }

        void MainWindow_onSettingsShow(object sender, RoutedEventArgs e)
        {
            IsSettingsGridOnForm = true;
        }


        void MainWindow_CreateReportShow(object sender, RoutedEventArgs e)
        {
            IsCreateReportWindowShow = true;
        }

        void MainWindow_CreateReportHide(object sender, RoutedEventArgs e)
        {
            IsCreateReportWindowShow = false;
        }


        private void CreateReportYesButton_Click(object sender, RoutedEventArgs e)
        {
            _createReportResetEvent.Set();
            RaiseEvent(new RoutedEventArgs(MainWindow.CreateReportHideEvent, this));
        }

        private void CreateReportNoButton_Click(object sender, RoutedEventArgs e)
        {
            _createReportCancellationToken.Cancel();
            _createReportResetEvent.Set();
            RaiseEvent(new RoutedEventArgs(MainWindow.CreateReportHideEvent, this));
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
                    LogMaker.InvokedLog(string.Format("Невозможно сохранить настройки в реестр. См. Error.log"), true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                }

                LogMaker.InvokedLog(string.Format("Настройки сохранены"), false, this.Dispatcher);
                IsSettingsGridOnForm = false;
            });
        }

        private async void dataChb_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            checkBox.IsEnabled = false;
            if (checkBox.Name == "agrologDataChb")
            {
                IsAgrologDataCollect = WatcherInit<TermometerAgrolog>(ref _agrologFolderWatcher, _agrologReportsPath, Constants.AGROLOG_FILE_EXTENSION);
                if (IsAgrologDataCollect)
                    await CheckDirectoryForNewData<TermometerAgrolog>(_agrologReportsPath, Constants.AGROLOG_FILE_EXTENSION);
            }
            else
            {
                IsGrainbarDataCollect = WatcherInit<TermometerGrainbar>(ref _grainbarFolderWatcher, _grainbarReportsPath, Constants.GRAINBAR_FILE_EXTENSION);
                if (IsGrainbarDataCollect)
                    await CheckDirectoryForNewData<TermometerGrainbar>(_grainbarReportsPath, Constants.GRAINBAR_FILE_EXTENSION);
            }
            checkBox.IsEnabled = true;
        }

        private void dataChb_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).Name == "agrologDataChb")
                DisposeWatcher(_agrologFolderWatcher, ProgramType.Agrolog);
            else DisposeWatcher(_grainbarFolderWatcher, ProgramType.Grainbar);
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
            if (e.Key == Key.Enter)
            {
                var textBox = (sender as TextBox);
                var text = textBox.Text;
                DirectoryInfo newDir = CreateNewFolder(text);
                if (newDir == null)
                {
                    textBox.Text = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
                    textBox.SelectAll();
                }
                await FileBrowsTreeViewDirExpandAsync(newDir.FullName, FileBrowsTreeView.Items);

                FileBrowsTreeView.Focus();
            }
        }

        private void FilesPathTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try //сохраним в реестре последний выбранный путь
            {
                if ((sender as TextBox).Name == "AgrologFilesPathTextBox")
                    Internal.SaveRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, AgrologReportsPath);
                else
                    Internal.SaveRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, GrainbarReportsPath);
            }
            catch (InvalidOperationException ex)
            {
                LogMaker.Log(string.Format("Невозможно сохранить настройки в реестр. См. Error.log"), true);
                ExceptionHandler.Handle(ex, false);
            }
            if ((sender as TextBox).Name == "AgrologFilesPathTextBox")
            {
                if (IsAgrologDataCollect)
                {
                    IsAgrologDataCollect = false;
                    IsAgrologDataCollect = true;
                }
            }
            else
                if (IsGrainbarDataCollect)
                {
                    IsGrainbarDataCollect = false;
                    IsGrainbarDataCollect = true;
                }
        }

        private async void FileSystemWatcher_OnCreated<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            string path = e.FullPath.Replace(e.Name, string.Empty);
            string dataFileName = DataFileName<T>();
            var disp = Dispatcher;
            DataHandlingLock<T>.SyncLock.WaitOne();
            bool dataIsNew = false;
            List<T> dataList = await ReadDataFile<T>(dataFileName);
            if (dataList == null)
                return;

            dataIsNew = await NewDataVerification<T>(path, e.Name, dataList);
            if (dataIsNew)
            {
                LogMaker.InvokedLog(string.Format("Данные приняты. Сохраняем их в файле \"{0}\".", dataFileName), false, this.Dispatcher);
                await WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, true);
            }
            DataHandlingLock<T>.SyncLock.Release();
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Constants.EXCEL_REPORT_FOLDER_PATH.PathFormatter();
            filePath += (sender as Button).Name == "AShowExcelBut" ? Constants.AGROLOG_EXCEL_REPORT_FILE_NAME : Constants.GRAINBAR_EXCEL_REPORT_FILE_NAME;
            string programName = (sender as Button).Name == "AShowExcelBut" ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            try
            {
                Process.Start(filePath);
                LogMaker.Log(string.Format("Отчет {0} запущен.", programName), false);
            }
            catch (FileNotFoundException ex)
            {
                LogMaker.Log(string.Format("Файл отчета {0} не существует.", programName), true);
                ExceptionHandler.Handle(ex, false);
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Ошибка открытия файла отчета {0}.", programName), true);
                ExceptionHandler.Handle(ex, false);
            }
        }
        #endregion
        //------------------------------------------------------------------------------

        #region events
        public static readonly RoutedEvent SettingShowEvent = EventManager.RegisterRoutedEvent(
        "SettingsShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler SettingsShow
        {
            add { AddHandler(SettingShowEvent, value); }
            remove { RemoveHandler(SettingShowEvent, value); }
        }

        public static readonly RoutedEvent CreateReportShowEvent = EventManager.RegisterRoutedEvent(
        "CreateReportShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler CreateReportShow
        {
            add { AddHandler(CreateReportShowEvent, value); }
            remove { RemoveHandler(CreateReportShowEvent, value); }
        }

        public static readonly RoutedEvent CreateReportHideEvent = EventManager.RegisterRoutedEvent(
        "CreateReportHide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler CreateReportHide
        {
            add { AddHandler(CreateReportHideEvent, value); }
            remove { RemoveHandler(CreateReportHideEvent, value); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region fields

        public bool IsFileBrowsTreeOnForm = false;                 //на форме ли окно выбора файлов
        public bool IsSettingsGridOnForm = false;
        public bool IsCreateReportWindowShow = false;

        bool _isAgrologDataCollect = false;
        bool _isGrainbarDataCollect = false;
        bool _isAutostart = true;
        bool _isDataSubstitution = false;
        string _agrologReportsPath;
        string _grainbarReportsPath;
        readonly Logger _log = LogManager.GetCurrentClassLogger();
        FileSystemWatcher _agrologFolderWatcher;
        FileSystemWatcher _grainbarFolderWatcher;
        readonly ManualResetEvent _createReportResetEvent = new ManualResetEvent(false);
        CancellationTokenSource _createReportCancellationToken;
        #endregion

        #region properties

        public string AgrologReportsPath
        {
            get { return _agrologReportsPath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _agrologReportsPath = Constants.AGROLOG_REPORTS_FOLDER_PATH;
                else _agrologReportsPath = value;
                NotifyPropertyChanged();
            }
        }

        public string GrainbarReportsPath
        {
            get { return _grainbarReportsPath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _grainbarReportsPath = Constants.AGROLOG_REPORTS_FOLDER_PATH;
                else _grainbarReportsPath = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsAgrologDataCollect
        {
            get { return _isAgrologDataCollect; }
            set { _isAgrologDataCollect = value; NotifyPropertyChanged(); }
        }

        public bool IsGrainbarDataCollect
        {
            get { return _isGrainbarDataCollect; }
            set { _isGrainbarDataCollect = value; NotifyPropertyChanged(); }
        }

        public bool IsAutostart
        {
            get { return _isAutostart; }
            set { _isAutostart = value; NotifyPropertyChanged(); }
        }

        public bool IsDataSubstitution
        {
            get { return _isDataSubstitution; }
            set { _isDataSubstitution = value; NotifyPropertyChanged(); }
        }

        private static class DataHandlingLock<T>
        {
            //public static readonly object SyncLock = new object();
            public static Semaphore SyncLock = new Semaphore(1, 1);

        }

        #endregion

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 50; i++)
                LogMaker.Log(string.Format("test"), i % 2 == 0);

        }

    }
}
