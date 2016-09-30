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
using tempa.Exceptions;

namespace tempa
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Settings();
            ManualInitializing();

            _log.Info(string.Format("{0} is started successfully.", Constants.APPLICATION_NAME));
        }

        private void Settings()
        {
            #region checked menu items
            #endregion

            Internal.CheckRegistrySettings(ref _agrologReportsPath, Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.AGROLOG_REPORTS_FOLDER_PATH);
            Internal.CheckRegistrySettings(ref _grainbarReportsPath, Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.GRAINBAR_REPORTS_FOLDER_PATH);
        }

        private void ManualInitializing()
        {
            LogTextBlock.Inlines.Clear();
            LogMaker.newMessage += LogMaker_newMessage;
            WatchersInit();
        }

        private void WatchersInit()
        {
            try
            {
                _agrologFolderWatcher = new FileSystemWatcher(_agrologReportsPath);
                WatcherSettings<TermometerAgrolog>(_agrologFolderWatcher, Constants.AGROLOG_FILE_EXTENSION, _agrologFolderWatcherState);
            }
            catch (ArgumentException ex)
            {
                LogMaker.Log(string.Format("Каталог Agrolog отчетов {0} не существует.", _agrologReportsPath), true);
                ExceptionHandler.Handle(ex, false);
            }
            try
            {
                _grainbarFolderWatcher = new FileSystemWatcher(_grainbarReportsPath);
                WatcherSettings<TermometerGrainbar>(_grainbarFolderWatcher, Constants.GRAINBAR_FILE_EXTENSION, _grainbarFolderWatcherState);
            }
            catch (ArgumentException ex)
            {
                LogMaker.Log(string.Format("Каталог Грейнбар отчетов {0} не существует.", _grainbarReportsPath), true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private void WatcherSettings<T>(FileSystemWatcher watcher, string fileExtension, bool enable) where T : ITermometer
        {
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*." + fileExtension;
            watcher.Created += (sender, e) => FileSystemWatcher_OnCreated<T>(sender, e);

            if (enable)
                watcher.EnableRaisingEvents = true;
        }

        private async void FileSystemWatcher_OnCreated<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            string programName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
            LogMaker.Log(string.Format("Обнаружен новый файл {0} отчета {1}. Начинаем процесс парсинга.", programName, e.Name), false);

            List<T> initReportList = await ReadNewReport<T>(sender, e, programName);
            if (initReportList == null)
                return;

            string binaryName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_ACTIVE_DATA_FILE : Constants.GRAINBAR_ACTIVE_DATA_FILE;
            LogMaker.Log(string.Format("Данные приняты. Сохраняем их в файле {0}.", binaryName), false);

            List<T> allPreviousReports = await ReadDataFile<T>(binaryName);
            if (allPreviousReports == null)
                return;
            throw new NotImplementedException("Доделать: сравнение списков данных по дате, если даты нет, то добавить и сохранить (не забыть про многопоточность)");

        }

        private async Task<List<T>> ReadNewReport<T>(object sender, FileSystemEventArgs e, string programName) where T : ITermometer
        {
            List<T> reportList;
            try
            {
                string path = e.FullPath.Replace(e.Name, string.Empty);
                reportList = await DataWorker.ReadReportAsync<T>(path, e.Name);
                if (reportList == null || reportList.Count == 0)
                    throw new InvalidOperationException(string.Format("Parsing file {0} operation returns empty result.", e.Name));
                return reportList;
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Парсинг данных файла {0} завершился неудачно. См. Error.log", programName, e.Name), true);
                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }

        private async Task<List<T>> ReadDataFile<T>(string dataFileName) where T : ITermometer
        {
            List<T> dataFile;

            Monitor.Enter(TypeLock<T>.SyncLock);
            try
            {
                dataFile = await DataWorker.ReadBinaryAsync<T>(Constants.APPLICATION_DIRECTORY, dataFileName);
                return dataFile;
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Процесс извлечения данных из файла {0} завершился неудачно. См. Error.log", dataFileName), true);
                ExceptionHandler.Handle(ex, false);
            }
            finally
            {
                Monitor.Exit(TypeLock<T>.SyncLock);
            }
            return null;
        }

        void LogMaker_newMessage(string message, bool isError)
        {
            Run run = new Run(message + Environment.NewLine);
            if (isError)
                run.Foreground = Brushes.Red;
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

        private void FileBrowsButt_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileBrowsTreeOnForm == false)
            {
                FileBrowsTreeView.Items.Clear();
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    TreeViewItem item = new TreeViewItem();
                    BrushConverter bc = new BrushConverter();
                    item.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    item.Tag = drive;
                    item.Header = drive.ToString();
                    item.Items.Add("*");
                    FileBrowsTreeView.Items.Add(item);
                }
                _isFileBrowsTreeOnForm = true;
                FileBrowsTreeView.Tag = (sender as Button).Tag;
                FileBrowsTreeView.Focus();
            }
        }

        private void FileBrowsTreeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            BrushConverter bc = new BrushConverter();
            item.Foreground = (Brush)bc.ConvertFrom("#FFBFB7B7");
            item.Items.Clear();
            DirectoryInfo dir;
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else dir = (DirectoryInfo)item.Tag;
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    newItem.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    item.Items.Add(newItem);
                }
            }
            catch
            { }
        }

        private void FileBrowsTreeView_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FileBrowsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int i = 0;
            String path = "";
            Stack<TreeViewItem> pathstack = Internal.GetNodes(e.NewValue as UIElement);

            foreach (TreeViewItem item in pathstack)
            {
                if (i > 0)
                    path += item.Header + "\\";
                else
                    path += item.Header;
                i++;
            }
            var tag = (ReportType)(sender as TreeView).Tag;
            if (tag == ReportType.Agrolog)
                _agrologReportsPath = path;
            else
                _grainbarReportsPath = path;
        }

        private void FileBrowsOkButt_Click(object sender, RoutedEventArgs e)
        {
            if (FileBrowsTreeView.SelectedValuePath != null)
            {
                try //сохраним в реестре последний выбранный путь
                {
                    AgrologFilesPathTextBox.Text = Internal.SaveRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, _agrologReportsPath);
                    GrainbarFilesPathTextBox.Text = Internal.SaveRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, _grainbarReportsPath);
                }
                catch (InvalidOperationException ex)
                {
                    LogMaker.Log(string.Format("Невозможно сохранить настройки в реестр. См. Error.log"), true);
                    ExceptionHandler.Handle(ex, false);
                }

            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button) == AgrologButton)
                WriteReport<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER, Constants.AGROLOG_ACTIVE_DATA_FILE, Constants.APPLICATION_REPORT_FOLDER_PATH, Constants.AGROLOG_REPORT_FILE_NAME);
            else if ((sender as Button) == GrainbarButton)
                WriteReport<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER, Constants.GRAINBAR_ACTIVE_DATA_FILE, Constants.APPLICATION_REPORT_FOLDER_PATH, Constants.GRAINBAR_REPORT_FILE_NAME);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MinimizeButt_Click(object sender, RoutedEventArgs e)
        {

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

        private async void WriteReport<T>(string dataFolderPath, string dataFileName, string reportFolderPath, string reportFileName) where T : ITermometer
        {
            try
            {
                LogMaker.Log(string.Format("Чтение данных из файла {0}", dataFileName), false);
                List<T> agrologData = await DataWorker.ReadBinaryAsync<T>(dataFolderPath, dataFileName);
                LogMaker.Log(string.Format("Формирование отчета {0}", dataFileName), false);
                await DataWorker.WriteReportAsync<T>(reportFolderPath, reportFileName, agrologData);
                LogMaker.Log(string.Format("Отчет {0} сформирован успешно.", reportFileName), false);
            }
            catch (WriteReportException ex)
            {
                if (ex.InnerException.GetType() == typeof(ReportFileException))
                {
                    LogMaker.Log(string.Format("Файл отчета не существует. Необходимо создать новый.", reportFileName), true);
                    throw new NotImplementedException("Тут планируется создание отображения анимированных кнопок 'Создать' и 'Отмена' и реализацию создание нового файла отчета. ");
                }
                else
                    LogMaker.Log(string.Format("Не получилось сформировать отчет {0}, cм. Error.log.", reportFileName), true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        ReportType AgrologReportType { get { return ReportType.Agrolog; } }
        ReportType GrainbarReportType { get { return ReportType.Grainbar; } }

        bool _isFileBrowsTreeOnForm = false;                 //на форме ли окно выбора файлов
        bool _agrologFolderWatcherState = false;
        bool _grainbarFolderWatcherState = false;
        string _agrologReportsPath;
        string _grainbarReportsPath;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        FileSystemWatcher _agrologFolderWatcher;
        FileSystemWatcher _grainbarFolderWatcher;
        //List<TermometerAgrolog> agrologData;
        //List<TermometerGrainbar> grainbarData;
        //Timer _errorLabelTimer;

        private static class TypeLock<T>
        {
            public static readonly object SyncLock = new object();
        }


    }
}
