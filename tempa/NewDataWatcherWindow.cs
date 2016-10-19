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

namespace CoffeeJelly.tempa
{
    public class NewDataWatcherWindow : Window, INotifyPropertyChanged
    {
        public NewDataWatcherWindow()
        {
            CheckDataFiles();
            Settings();
        }

        private void Settings()
        {
            bool checkAgrolog = Internal.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            bool checkGrainbar = Internal.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            if (checkAgrolog)
            {
                WatcherInitChange("IsAgrologDataCollect", true);
                var checkAgrologDirTask = CheckDirectoryForNewDataAsync<TermometerAgrolog>(GetReportsPathFromReg(ProgramType.Agrolog), Constants.AGROLOG_FILE_EXTENSION);
                checkAgrologDirTask.CriticalTask();
            }
            if (checkGrainbar)
            {
                WatcherInitChange("IsGrainbarDataCollect", true);
                var checkGrainBarDirTask = CheckDirectoryForNewDataAsync<TermometerGrainbar>(GetReportsPathFromReg(ProgramType.Grainbar), Constants.GRAINBAR_FILE_EXTENSION);
                checkGrainBarDirTask.CriticalTask();
            }
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
                    MessageBox.Show("Критическая ошибка. Приложение закроется через 5 сек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.ServiceNotification);
                }));
                Thread.Sleep(3000);
                ExceptionHandler.Handle(ex, true);
            }
        }

        private void CreateNewDataFile<T>(string dataFilePath, string dataFileName) where T : ITermometer
        {
            LogMaker.Log($"Файл данных {dataFilePath.PathFormatter() + dataFileName} не обнаружен. Создаю новый.", false);
            var patternReportText = typeof(T) == typeof(TermometerAgrolog) ? Properties.Resources.AgrologPatternReport : Properties.Resources.GrainbarPatternReport;
            var patternReportList = DataWorker.ReadPatternReport<T>(patternReportText);
            var task = WriteDataFile<T>(patternReportList, dataFilePath, dataFileName, false);
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
            watcher.Created += (sender, e) => FileSystemWatcher_OnCreated<T>(sender, e);
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

        private string DataFileName<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
        }

        private async Task<List<T>> ReadDataFile<T>(string dataFileName, bool readAsync) where T : ITermometer
        {
            try
            {
                List<T> dataFile;
                if (readAsync)
                    dataFile = await DataWorker.ReadBinaryAsync<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                else
                    dataFile = DataWorker.ReadBinary<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                return dataFile;
            }
            catch (IOException ex)
            {

            }
            catch (Exception ex)
            {
                LogMaker.Log($"Процесс извлечения данных из файла \"{dataFileName}\" завершился неудачно. См. Error.log", true);
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
                LogMaker.Log($"Процесс записи данных в файл \"{dataFileName}\" завершился неудачно. См. Error.log", true);
                ExceptionHandler.Handle(ex, false);
                return false;
            }
        }

        private Task CheckDirectoryForNewDataAsync<T>(string path, string fileExtension) where T : ITermometer
        {
            return Task.Factory.StartNew(() => CheckDirectoryForNewData<T>(path, fileExtension));
        }

        private void CheckDirectoryForNewData<T>(string path, string fileExtension) where T : ITermometer
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                LogMaker.Log($"Каталог \"{path}\" не существует, или к нему нет доступа.", true);
                return;
            }

            FileInfo[] filesInfo = dirInfo.GetFiles("*." + fileExtension);
            string programName = Internal.GetProgramName<T>();
            string dataFileName = DataFileName<T>();

            LockNow<T>();


            LogMaker.Log($"Начинаю проверку каталога \"{path}\" на наличие новых данных {programName}...", false);
            var checkList = new List<bool>();
            List<T> dataList = ReadDataFile<T>(dataFileName, false).Result;
            if (dataList == null)
                return;

            foreach (var fileInfo in filesInfo)
            {
                if (ExitToken.IsCancellationRequested)
                    return;
                checkList.Add(NewDataVerification<T>(fileInfo.DirectoryName, fileInfo.Name, dataList, false).Result);
            }

            if (checkList.Any(b => b == true))
            {
                LogMaker.Log($"Данные приняты. Сохраняю их в файле \"{dataFileName}\".", false);
                var writeResult = WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, false).Result;
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

            UnLockNow<T>();
        }

        private async Task<bool> NewDataVerification<T>(string path, string fileName, List<T> dataList, bool isAsync) where T : ITermometer
        {
            string programName = Internal.GetProgramName<T>();
            LogMaker.Log($"Обнаружен новый файл \"{programName}\" отчета {fileName}. Начинаем процесс парсинга...", false);
            List<T> initReportList;
            initReportList = await ReadNewReport<T>(path, fileName, programName, isAsync);

            if (initReportList == null)
                return false;

            bool operationValue = false;
            DateTime initReportDate = initReportList.First().MeasurementDate;
            bool initReportAlreadySaved = dataList.Any(t => t.MeasurementDate == initReportDate);
            if (!initReportAlreadySaved)
            {
                dataList.AddRange(initReportList);
                LogMaker.Log($"Данные из файла отчета \"{fileName}\" новые. Запишу в базу данных", false);
                operationValue = true;
            }
            else
            {
                string dataFileName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
                LogMaker.Log(
                    $"Данные из файла отчета \"{fileName}\" уже существуют в базе данных файла \"{dataFileName}\".", false);
                operationValue = false;
            }
            return operationValue;
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

        private void DeleteFile(string path, string fileName)
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

        public void WatcherInitChange(string propertyName, bool state)
        {
            if (propertyName == "IsAgrologDataCollect")
            {
                if (state && _agrologDataWatcher == null)
                {
                    bool result = WatcherInit<TermometerAgrolog>(ref _agrologDataWatcher, GetReportsPathFromReg(ProgramType.Agrolog), Constants.AGROLOG_FILE_EXTENSION);
                    if (!result)
                        return;
                }
                else if (!state && _agrologDataWatcher != null)
                    DisposeWatcher<TermometerAgrolog>(ref _agrologDataWatcher);

            }
            else if (propertyName == "IsGrainbarDataCollect")
            {
                if (state && _grainbarDataWatcher == null)
                {
                    bool result = WatcherInit<TermometerGrainbar>(ref _grainbarDataWatcher, GetReportsPathFromReg(ProgramType.Grainbar), Constants.GRAINBAR_FILE_EXTENSION);
                    if (!result)
                        return;
                }
                else if (!state && _grainbarDataWatcher != null)
                    DisposeWatcher<TermometerGrainbar>(ref _grainbarDataWatcher);
            }
        }

        private string GetReportsPathFromReg(ProgramType programType)
        {
            string regKeyPath = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_PATH_REGKEY : Constants.GRAINBAR_REPORTS_PATH_REGKEY;
            string regKeyValue = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_FOLDER_PATH : Constants.AGROLOG_REPORTS_FOLDER_PATH;

            return Internal.CheckRegistrySettings(regKeyPath, Constants.SETTINGS_LOCATION, regKeyValue);
        }

        private void LockNow<T>()
        {
            var qWaitOne = DataHandlingLock<T>.SyncLock.WaitOne();
            if (typeof(T) == typeof(TermometerAgrolog))
                AgrologQueue = qWaitOne;
            else if (typeof(T) == typeof(TermometerGrainbar))
                GrainbarQueue = qWaitOne;
        }

        private void UnLockNow<T>()
        {
            var qRelease = DataHandlingLock<T>.SyncLock.Release();
            if (qRelease > 0)
                return;

            if (typeof(T) == typeof(TermometerAgrolog))
                AgrologQueue = false;
            else if (typeof(T) == typeof(TermometerGrainbar))
                GrainbarQueue = false;
        }

        private void FileSystemWatcher_OnCreated<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            if (ExitToken.IsCancellationRequested)
            {
                this.Dispatcher.Invoke(new Action(() => (sender as FileSystemWatcher).EnableRaisingEvents = false));
                return;
            }

            var fileProcessingTask = FileProcessingAsync<T>(sender, e);

            fileProcessingTask.CriticalTask();
        }

        private Task FileProcessingAsync<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            return Task.Factory.StartNew(() => FileProcessing<T>(sender, e));
        }

        private void FileProcessing<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            string path = e.FullPath.Replace(e.Name, string.Empty);
            string dataFileName = DataFileName<T>();
            var disp = Dispatcher;
            bool dataIsNew = false;

             LockNow<T>();

            List<T> dataList = ReadDataFile<T>(dataFileName, false).Result;
            if (dataList == null)
                return;

            dataIsNew = NewDataVerification<T>(path, e.Name, dataList, false).Result;

            var writeResult = true;
            if (dataIsNew)
            {
                LogMaker.Log($"Данные приняты. Сохраняем их в файле \"{dataFileName}\".", false);
                writeResult = WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, false).Result;
            }
            if (writeResult)
                DeleteFile(path, e.Name);

            UnLockNow<T>();
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

        private FileSystemWatcher _agrologDataWatcher;
        private FileSystemWatcher _grainbarDataWatcher;
        private bool _agrologQueue;
        private bool _grainbarQueue = false;

        public bool AgrologQueue
        {
            get { return _agrologQueue; }
            set
            {
                _agrologQueue = value;
                NotifyPropertyChanged();
            }
        }

        public bool GrainbarQueue
        {
            get { return _grainbarQueue; }
            set
            {
                _grainbarQueue = value;
                NotifyPropertyChanged();
            }
        }

        private static class DataHandlingLock<T>
        {
            public static Semaphore SyncLock = new Semaphore(1, 1);
        }

    }

}
