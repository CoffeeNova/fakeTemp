using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoffeeJelly.tempa.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace CoffeeJelly.tempa
{
    public class NewDataWatcherWindow : Window, INotifyPropertyChanged
    {
        public NewDataWatcherWindow()
        {
            bool checkAgrolog;
            bool checkGrainbar;

            Settings(out checkAgrolog, out checkGrainbar);
            CheckDataFiles();

            NewDataInitVerification<TermometerAgrolog>(checkAgrolog);
            NewDataInitVerification<TermometerGrainbar>(checkGrainbar);
        }

        private void NewDataInitVerification<T>(bool check) where T : ITermometer
        {
            if (!check) return;

            WatcherInitChange<T>(true);
            var task = CheckDirectoryForNewDataAsync<T>(GetNewDataPathFromReg<T>(), DefineDataFileExtension<T>());
            task.AwaitCriticalTask();
        }

        private void Settings(out bool checkAgrolog, out bool checkGrainbar)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Ru");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-Ru");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            checkAgrolog = Internal.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            checkGrainbar = Internal.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);



        }

        private void CheckDataFiles()
        {
            try
            {
                var datFile = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.AGROLOG_DATA_FILE);
                if (!datFile.Exists)
                    CreateNewDataFile<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter(),
                        Constants.AGROLOG_DATA_FILE);
                else
                    NewPeriodVerify<TermometerAgrolog>(datFile);

                datFile = new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() + Constants.GRAINBAR_DATA_FILE);
                if (!datFile.Exists)
                    CreateNewDataFile<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter(), Constants.GRAINBAR_DATA_FILE);
                else
                    NewPeriodVerify<TermometerGrainbar>(datFile);
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew((() =>
                {
                    MessageBox.Show("Критическая ошибка. Приложение закроется через 5 сек.", "Критическая ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.ServiceNotification);
                }));
                Thread.Sleep(5000);
                ExceptionHandler.Handle(ex, true);
            }
        }

        private void NewPeriodVerify<T>(FileInfo fileInfo) where T : ITermometer
        {
            if (fileInfo.CreationTime.Year >= DateTime.Today.Year)
                return;
            LogMaker.Log($"Начало нового периода. Копирую данные файла {fileInfo.Name} в архив.", false);
            ArchieveDataFile<T>(fileInfo);
        }

        private void ArchieveDataFile<T>(FileInfo dataFileInfo) where T : ITermometer
        {
            List<T> dataList = ReadDataFile<T>(dataFileInfo.Name, false).Result;

            var patternDate = dataList.First().MeasurementDate;
            dataList.RemoveAll(t => t.MeasurementDate == patternDate);
            if (dataList.Count == 0)
                return; //data file had only pattern info

            DateTime firstDate = dataList.First().MeasurementDate;
            DateTime lastDate = dataList.Last().MeasurementDate;
            string archievedName = $"{firstDate.Date:dd.MM.yy}-{lastDate.Date:dd.MM.yy} {dataFileInfo.Name}";
            bool transferResult = TransferDataFileToArchieve(dataFileInfo, archievedName);
            if (transferResult)
                CreateNewDataFile<T>(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter(), DefineDataFileName<T>());
        }

        private bool TransferDataFileToArchieve(FileInfo dataFileInfo, string archievedName)
        {
            try
            {
                var archievedFileInfo =
                    dataFileInfo.CopyTo(Constants.APPLICATION_ARCHIEVE_DATA_FOLDER_PATH.PathFormatter() + archievedName);
                return archievedFileInfo.Exists;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(IOException))
                {
                    LogMaker.Log(
                        $"В архиве уже существует файл с таким именем {archievedName}. Приложение будет закрыто.", true);
                    MessageBox.Show(
                        $"В архиве уже существует файл с таким именем {archievedName}. \r\n Приложение будет закрыто.",
                        "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes,
                        MessageBoxOptions.ServiceNotification);
                    ExceptionHandler.Handle(ex, true);
                }
                else
                {
                    LogMaker.Log($"Ошибка сохранения в архив {archievedName}. Приложение будет закрыто.", true);
                    MessageBox.Show($"Ошибка сохранения в архив {archievedName}. \r\n Приложение будет закрыто.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes, MessageBoxOptions.ServiceNotification);
                }
            }
            return false;
        }

        private void CreateNewDataFile<T>(string dataFilePath, string dataFileName) where T : ITermometer
        {
            LogMaker.Log($"Файл данных {dataFilePath.PathFormatter() + dataFileName} не обнаружен. Создаю новый.", false);
            var patternReportText = typeof(T) == typeof(TermometerAgrolog) ? Properties.Resources.AgrologPatternReport : Properties.Resources.GrainbarPatternReport;
            var patternReportList = DataWorker.ReadPatternReport<T>(patternReportText);
            var task = WriteDataFile(patternReportList, dataFilePath, dataFileName, false);
            if (!task.Result)
                throw new InvalidOperationException("Can't create .dat file. Operation of application work is impossible.");
        }

        private bool WatcherInit<T>(ref FileSystemWatcher watcher, string reportsPath, string fileExtension) where T : ITermometer
        {
            string programName = Internal.GetProgramName<T>();

            try
            {
                watcher = new FileSystemWatcher(reportsPath);
                WatcherSettings<T>(watcher, fileExtension);
                LogMaker.Log($"Запуск мониторинга данных \"{programName}\".", false);
                return true;
            }
            catch (ArgumentException ex)
            {
                LogMaker.Log($"Каталог \"{reportsPath}\" отчетов {programName} не существует.", true);
                ExceptionHandler.Handle(ex, false);
                return false;
            }
        }

        private void WatcherSettings<T>(FileSystemWatcher watcher, string fileExtension) where T : ITermometer
        {
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*." + fileExtension;
            watcher.Created += FileSystemWatcher_OnCreated<T>;
            watcher.InternalBufferSize = 81920;
            watcher.EnableRaisingEvents = true;
            watcher.Error += Watcher_Error;
        }

        private void DisposeWatcher<T>(ref FileSystemWatcher watcher)
        {
            if (watcher == null)
                return;
            string programName = Internal.GetProgramName<T>();
            LogMaker.Log($"Данные {programName} из каталога \"{watcher.Path}\" не принимаются.", false);
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }

        private static string DefineDataFileName<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog) ? 
                Constants.AGROLOG_DATA_FILE : 
                Constants.GRAINBAR_DATA_FILE;
        }

        private static string DefineProgramName<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog) ?
                Constants.AGROLOG_PROGRAM_NAME :
                Constants.GRAINBAR_PROGRAM_NAME;
        }

        private string DefineDataFileExtension<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog)
                ? Constants.AGROLOG_FILE_EXTENSION
                : Constants.GRAINBAR_FILE_EXTENSION;
        }


        private async Task<List<T>> ReadDataFile<T>(string dataFileName, bool readAsync) where T : ITermometer
        {
            try
            {
                LockDataAccess<T>();
                List<T> dataFile;
                if (readAsync)
                    dataFile = await DataWorker.ReadBinaryAsync<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                else
                    dataFile = DataWorker.ReadBinary<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                return dataFile;
            }
            catch (Exception ex)
            {
                CriticalErrorLock.WaitOne();
                LogMaker.Log(
                    $"Процесс извлечения данных из файла \"{dataFileName}\" завершился неудачно. См. Error.log", true);
                MessageBox.Show(
                    $"Процесс извлечения данных из файла \"{dataFileName}\" завершился неудачно. См. Error.log. \r\n Критическая ошибка.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes,
                    MessageBoxOptions.ServiceNotification);
                ExceptionHandler.Handle(ex, true);
            }
            finally
            {
                UnLockDataAccess<T>();
            }

            return null;
        }

        private async Task<bool> WriteDataFile<T>(IEnumerable<T> dataList, string dataFilePath, string dataFileName, bool writeAsync) where T : ITermometer
        {
            try
            {
                LockDataAccess<T>();
                var orderedDataList = dataList.OrderBy(t => t.MeasurementDate).ToList();

                if (writeAsync)
                    await DataWorker.WriteBinaryAsync<T>(dataFilePath, dataFileName, orderedDataList);
                else DataWorker.WriteBinary<T>(dataFilePath, dataFileName, orderedDataList);

                if (typeof(T) == typeof(TermometerAgrolog))
                    TotalReportsProcessedA += orderedDataList.GroupBy(t => t.MeasurementDate).Count() - 1;
                else if (typeof(T) == typeof(TermometerGrainbar))
                    TotalReportsProcessedG += orderedDataList.GroupBy(t => t.MeasurementDate).Count() - 1;
                return true;
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Процесс записи данных в файл \"{dataFileName}\" завершился неудачно. См. Error.log", true);
                ExceptionHandler.Handle(ex, false);
                return false;
            }
            finally
            {
                UnLockDataAccess<T>();
            }
        }

        private Task CheckDirectoryForNewDataAsync<T>(string path, string fileExtension) where T : ITermometer
        {
            return Task.Factory.StartNew(() => CheckDirectoryForNewData<T>(path, fileExtension));
        }

        private void CheckDirectoryForNewData<T>(string path, string fileExtension) where T : ITermometer
        {
            DataHandlingLock<T>.SyncLock.WaitOne();
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                LogMaker.Log($"Каталог \"{path}\" не существует, или к нему нет доступа.", true);
                return;
            }

            FileInfo[] filesInfo = dirInfo.GetFiles("*." + fileExtension);
            string programName = Internal.GetProgramName<T>();
            string dataFileName = DefineDataFileName<T>();

            LogMaker.Log($"Начинаю проверку каталога \"{path}\" на наличие новых данных {programName}...", false);
            List<T> dataList = ReadDataFile<T>(dataFileName, false).Result;
            if (dataList == null)
            {
                DataHandlingLock<T>.SyncLock.Release();
                return;
            }

            var checkList = new List<bool>();
            foreach (var fileInfo in filesInfo)
            {
                if (ExitToken.IsCancellationRequested)
                    return;
                checkList.Add(NewDataVerification<T>(fileInfo.DirectoryName, fileInfo.Name, dataList, false).Result != null);
            }

            if (checkList.Any(b => b == true))
            {
                LogMaker.Log($"Данные приняты. Сохраняю их в файле \"{dataFileName}\".", false);
                var writeResult = WriteDataFile(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, false).Result;
                if (!writeResult)
                {
                    LogMaker.Log("Данные не сохранены, обнаруженные данные не будут удалены.", false);
                    return;
                }
            }
            else
                LogMaker.Log($"Новых данных {programName} не обнаружено.", false);

            foreach (var fileInfo in filesInfo)
                DeleteFile(fileInfo.DirectoryName, fileInfo.Name);

            DataHandlingLock<T>.SyncLock.Release();
        }

        private async Task<List<T>> NewDataVerification<T>(string path, string fileName, List<T> dataList, bool isAsync) where T : ITermometer
        {
            string programName = Internal.GetProgramName<T>();
            LogMaker.Log($"Обнаружен новый файл \"{programName}\" отчета {fileName}. Начинаем процесс парсинга...", false);

            List<T> initReportList = null;
            int readTryCount = 10;
            while (initReportList == null && readTryCount > 0)
            {
                initReportList = await ReadNewReport<T>(path, fileName, programName, isAsync);
                if (initReportList == null)
                {
                    Thread.Sleep(500);
                    readTryCount--;
                }
            }
            if (initReportList == null)
                return null;

            DateTime initReportDate = initReportList.First().MeasurementDate;
            bool initReportAlreadySaved = dataList.Any(t => DateTime.Compare(t.MeasurementDate, initReportDate) == 0);
            if (!initReportAlreadySaved)
            {
                dataList.AddRange(initReportList);
                LogMaker.Log($"Данные из файла отчета \"{fileName}\" новые. Запишу в базу данных", false);
                return initReportList;
            }
            else
            {
                string dataFileName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
                LogMaker.Log(
                    $"Данные из файла отчета \"{fileName}\" уже существуют в базе данных файла \"{dataFileName}\".", false);
                return null;
            }
        }

        private async Task<List<T>> ReadNewReport<T>(string path, string fileName, string programName, bool isAsync) where T : ITermometer
        {
            List<T> reportList;
            try
            {
                if (isAsync)
                    reportList = await DataWorker.ReadReportAsync<T>(path, fileName);
                else
                    reportList = DataWorker.ReadReport<T>(path, fileName);

                if (reportList == null || reportList.Count == 0)
                    throw new InvalidOperationException($"Parsing file \"{fileName}\" operation returns empty result.");
                return reportList;
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Парсинг данных файла \"{fileName}\" завершился неудачно. См. Error.log", true);
                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }

        private static void DeleteFile(string path, string fileName)
        {
            LogMaker.Log($"Удаляю файл \"{fileName}\".", false);
            try
            {
                new FileInfo(path.PathFormatter() + fileName).Delete();
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Ошибка удаления файла \"{fileName}\".", true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        public void WatcherInitChange<T>(bool state) where T : ITermometer
        {
            if (state && NewDataWatcher<T>.FileSystemWatcher == null)
                WatcherInit<T>(ref NewDataWatcher<T>.FileSystemWatcher, GetNewDataPathFromReg<T>(), DefineDataFileExtension<T>());
            else if (!state && NewDataWatcher<T>.FileSystemWatcher != null)
                DisposeWatcher<T>(ref NewDataWatcher<T>.FileSystemWatcher);
        }

        private string GetNewDataPathFromReg<T>() where T : ITermometer
        {
            string regKeyPath = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_REPORTS_PATH_REGKEY : Constants.GRAINBAR_REPORTS_PATH_REGKEY;
            string regKeyValue = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_REPORTS_FOLDER_PATH : Constants.GRAINBAR_REPORTS_FOLDER_PATH;

            return Internal.CheckRegistrySettings(regKeyPath, Constants.SETTINGS_LOCATION, regKeyValue);
        }


        public void LockDataAccess<T>() where T : ITermometer
        {
            var qWaitOne = DataFilePermissionLock<T>.SyncLock.WaitOne();
            if (typeof(T) == typeof(TermometerAgrolog))
                AgrologDataHandlingPermission = !qWaitOne;
            else if (typeof(T) == typeof(TermometerGrainbar))
                GrainbarDataHandlingPermission = !qWaitOne;
        }

        public void UnLockDataAccess<T>() where T : ITermometer
        {
            var qRelease = DataFilePermissionLock<T>.SyncLock.Release();
            if (qRelease > 0)
                return;

            DataHandlingPermissionTimer<T>.StartFunction();
        }

        private void FileSystemWatcher_OnCreated<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            if (ExitToken.IsCancellationRequested)
            {
                this.Dispatcher.Invoke(new Action(() => (sender as FileSystemWatcher).EnableRaisingEvents = false));
                return;
            }

            var fileProcessingTask = FileProcessingAsync<T>(sender, e);

             fileProcessingTask.AwaitCriticalTask();
        }

        private Task FileProcessingAsync<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            return Task.Factory.StartNew(() => FileProcessing<T>(sender, e));
        }

        private void FileProcessing<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            DataHandlingLock<T>.SyncLock.WaitOne();
            string path = e.FullPath.Replace(e.Name, string.Empty);
            string dataFileName = DefineDataFileName<T>();
            var disp = Dispatcher;

            if (FileWatcherTimer<T>.State == FileWatcherTimer<T>.TimerState.Stopped)
            {
                var data = ReadDataFile<T>(dataFileName, false).Result;
                if (data == null)
                {
                    DataHandlingLock<T>.SyncLock.Release();
                    return;
                }
                TempRepository<T>.ExistedData.AddRange(data);
            }

            FileWatcherTimer<T>.StartFunction(path, e.Name);

            List<T> newData = NewDataVerification<T>(path, e.Name, TempRepository<T>.ExistedData, false).Result;
            if (newData == null)
                FileWatcherTimer<T>.ProcessedFilesList.Remove(FileWatcherTimer<T>.ProcessedFilesList.Last());
            if (newData != null)
                foreach (var term in newData)
                    TempRepository<T>.NewData.Add(term);

            DataHandlingLock<T>.SyncLock.Release();
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            LogMaker.Log("Слишком много файлов за один раз. Буфер увеличен в 2 раза. Удалите файлы из каталога и попробуйте еще раз.", true);
            var fileSystemWatcher = sender as FileSystemWatcher;
            if (fileSystemWatcher != null)
                fileSystemWatcher.InternalBufferSize = fileSystemWatcher.InternalBufferSize * 2;
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static CancellationTokenSource ExitCancelTokenSource = new CancellationTokenSource();
        public static CancellationToken ExitToken = ExitCancelTokenSource.Token;

        private bool _agrologDataHandlingPermission = true;
        private bool _grainbarDataHandlingPermission = true;
        private int _totalReportsProcessedA = 0;
        private int _totalReportsProcessedG = 0;
        private static readonly object Locker = new object();
        private static readonly Semaphore CriticalErrorLock = new Semaphore(1, 1);

        public bool AgrologDataHandlingPermission
        {
            get { return _agrologDataHandlingPermission; }
            set
            {
                if (_agrologDataHandlingPermission == value)
                    return;
                _agrologDataHandlingPermission = value;
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
                NotifyPropertyChanged();
            }
        }

        public int TotalReportsProcessedA
        {
            get { return _totalReportsProcessedA; }
            set
            {
                if (_totalReportsProcessedA == value)
                    return;
                _totalReportsProcessedA = value;
                NotifyPropertyChanged();
            }
        }

        public int TotalReportsProcessedG
        {
            get { return _totalReportsProcessedG; }
            set
            {
                if (_totalReportsProcessedG == value)
                    return;
                _totalReportsProcessedG = value;
                NotifyPropertyChanged();
            }
        }

        private static class DataFilePermissionLock<T> where T : ITermometer
        {
            public static readonly Semaphore SyncLock = new Semaphore(1, 1);
        }

        private static class DataHandlingLock<T> where T : ITermometer
        {
            public static readonly Semaphore SyncLock = new Semaphore(1, 1);
        }

        private static class TempRepository<T> where T : ITermometer
        {
            public static List<T> NewData { get; } = new List<T>();
            public static List<T> ExistedData { get; set; } = new List<T>();
        }

        private static class NewDataWatcher<T> where T : ITermometer
        {
            public static FileSystemWatcher FileSystemWatcher;
        }

        private static class DataHandlingPermissionTimer<T> where T : ITermometer
        {
            static DataHandlingPermissionTimer()
            {
                Timer.Elapsed += Timer_Elapsed;
            }

            public static void StartFunction()
            {
                State = TimerState.Started;
                Timer.Stop();
                Timer.Start();
            }

            private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                var newDataWatcherWindow = (NewDataWatcherWindow)Application.Current.Dispatcher.Invoke(
                                                                    new Func<NewDataWatcherWindow>(() =>
                                                                   Application.Current.MainWindow as NewDataWatcherWindow));
                if (newDataWatcherWindow == null)
                    return;
                if (typeof(T) == typeof(TermometerAgrolog))
                    newDataWatcherWindow.AgrologDataHandlingPermission = true;
                else if (typeof(T) == typeof(TermometerGrainbar))
                    newDataWatcherWindow.GrainbarDataHandlingPermission = true;

                State = TimerState.Stopped;
            }

            public static TimerState State { get; private set; } = TimerState.Stopped;

            public static Timer Timer { get; } = new System.Timers.Timer()
            {
                Interval = 1000,
                AutoReset = false
            };

            public enum TimerState
            {
                Started,
                Stopped
            }
        }

        private static class FileWatcherTimer<T> where T : ITermometer
        {
            public static void StartFunction(string filePath, string fileName)
            {
                State = TimerState.Started;
                Timer.Stop();
                Timer.Start();
                ProcessedFilesList.Add(new FileBaseInfo(filePath, fileName));
            }

            static FileWatcherTimer()
            {
                Timer.Elapsed += Timer_Elapsed;
            }

            private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                DataHandlingLock<T>.SyncLock.WaitOne();
                var dataFileName = DefineDataFileName<T>();
                var newDataWatcherWindow = (NewDataWatcherWindow)Application.Current.Dispatcher.Invoke(
                                                                    new Func<NewDataWatcherWindow>(() =>
                                                                   Application.Current.MainWindow as NewDataWatcherWindow));
                if (newDataWatcherWindow == null)
                {
                    DataHandlingLock<T>.SyncLock.Release();
                    return;
                }
                var writeResult = true;
                if (TempRepository<T>.NewData.Count > 0)
                {
                    LogMaker.Log($"Новых данных - {TempRepository<T>.NewData.Count}. Данные приняты. Сохраняем их в файле \"{dataFileName}\".", false);
                    writeResult = newDataWatcherWindow.WriteDataFile(TempRepository<T>.ExistedData, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, false).Result;
                }
                else
                    LogMaker.Log($"Новых данных {DefineProgramName<T>()} не обнаружено.", false);

                if (writeResult)
                    foreach (var file in ProcessedFilesList)
                        DeleteFile(file.Path, file.Name);


                State = TimerState.Stopped;
                ProcessedFilesList.Clear();
                TempRepository<T>.NewData.Clear();
                TempRepository<T>.ExistedData.Clear();

                DataHandlingLock<T>.SyncLock.Release();
            }
            /// <summary>
            /// List of processed files by FileSystemWatcher, where key - file path, value - file name.
            /// </summary>
            public static List<FileBaseInfo> ProcessedFilesList { get; } = new List<FileBaseInfo>();

            public static TimerState State { get; private set; } = TimerState.Stopped;

            public static Timer Timer { get; } = new System.Timers.Timer()
            {
                Interval = 3000,
                AutoReset = false
            };

            public enum TimerState
            {
                Started,
                Stopped
            }

            public class FileBaseInfo
            {
                public FileBaseInfo(string path, string name)
                {
                    Path = path;
                    Name = name;
                }
                public string Path { get; private set; }
                public string Name { get; private set; }
            }
        }
    }

}
