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
            PropertyChanged += NewDataWatcherWindow_PropertyChanged;
            Settings();
            CheckDataFiles();
        }

        private void Settings()
        {
            IsAgrologDataCollect = Internal.CheckRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsGrainbarDataCollect = Internal.CheckRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsAutostart = Internal.CheckRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, true);
            IsDataSubstitution = Internal.CheckRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, false);
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
            string programName = Internal.GetProgramName<T>();

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
            watcher.Error += Watcher_Error;
        }

        private void DisposeWatcher<T>(FileSystemWatcher watcher)
        {
            if (watcher == null)
                return;
            string programName = Internal.GetProgramName<T>();
            LogMaker.Log(string.Format("Данные {0} из каталога \"{1}\" не принимаются.", programName, watcher.Path), false);
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();

        }

        private string DataFileName<T>() where T : ITermometer
        {
            return typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
        }

        private async Task<List<T>> ReadDataFile<T>(string dataFileName, bool readAsync) where T : ITermometer
        {
            List<T> dataFile;

            try
            {
                if (readAsync)
                    dataFile = await DataWorker.ReadBinaryAsync<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
                else
                    dataFile = DataWorker.ReadBinary<T>(Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName);
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

        private async Task CheckDirectoryForNewData<T>(string path, string fileExtension) where T : ITermometer
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                LogMaker.Log(string.Format("Каталог \"{0}\" не существует, или к нему нет доступа.", path), true);
                return;
            }

            FileInfo[] filesInfo = dirInfo.GetFiles("*." + fileExtension);
            string programName = Internal.GetProgramName<T>();
            string dataFileName = DataFileName<T>();
            DataHandlingLock<T>.SyncLock.WaitOne();
            LogMaker.Log(string.Format("Начинаю проверку каталога \"{0}\" на наличие новых данных {1}...", path, programName), false);
            var checkList = new List<bool>();
            List<T> dataList = await ReadDataFile<T>(dataFileName, true);
            if (dataList == null)
                return;

            foreach (var fileInfo in filesInfo)
                checkList.Add(await NewDataVerification<T>(fileInfo.DirectoryName, fileInfo.Name, dataList, true));

            if (checkList.Any(b => b == true))
            {
                LogMaker.Log(string.Format("Данные приняты. Сохраняю их в файле \"{0}\".", dataFileName), false);
                await WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, true);
            }
            else
                LogMaker.Log(string.Format("Новых данных {0} не обнаружено.", programName), false);
            DataHandlingLock<T>.SyncLock.Release();
        }

        private async Task<bool> NewDataVerification<T>(string path, string fileName, List<T> dataList, bool isAsync) where T : ITermometer
        {
            string programName = Internal.GetProgramName<T>();
            LogMaker.Log(string.Format("Обнаружен новый файл \"{0}\" отчета {1}. Начинаем процесс парсинга...", programName, fileName), false);
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
                LogMaker.Log(string.Format("Данные из файла отчета \"{0}\" новые. Запишу в базу данных", fileName), false);
                operationValue = true;
            }
            else
            {
                string dataFileName = typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_DATA_FILE : Constants.GRAINBAR_DATA_FILE;
                LogMaker.Log(string.Format("Данные из файла отчета \"{0}\" уже существуют в базе данных файла \"{1}\".", fileName, dataFileName), false);
                operationValue = false;
            }
            DeleteFile(path, fileName);
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
                    throw new InvalidOperationException(string.Format("Parsing file \"{0}\" operation returns empty result.", fileName));
                return reportList;
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Парсинг данных файла \"{0}\" завершился неудачно. См. Error.log", fileName), true);
                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }

        private void DeleteFile(string path, string fileName)
        {
            LogMaker.Log(string.Format("Удаляю файл \"{0}\".", fileName), false);
            try
            {
                new FileInfo(path.PathFormatter() + fileName).Delete();
            }
            catch (Exception ex)
            {
                LogMaker.Log(string.Format("Ошибка удаления файла \"{0}\".", fileName), true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private async void WatcherInitChange(string match)
        {
            if (match == nameof(IsAgrologDataCollect))
            {
                if (IsAgrologDataCollect)
                {
                    bool result = WatcherInit<TermometerAgrolog>(ref _agrologDataWatcher, GetReportsPathFromReg(ProgramType.Agrolog), Constants.AGROLOG_FILE_EXTENSION);
                    if (!result)
                        return;
                    await CheckDirectoryForNewData<TermometerAgrolog>(GetReportsPathFromReg(ProgramType.Agrolog), Constants.AGROLOG_FILE_EXTENSION);
                }
                else
                    DisposeWatcher<TermometerAgrolog>(_agrologDataWatcher);

            }
            else if (match == nameof(IsGrainbarDataCollect))
            {
                if (IsGrainbarDataCollect)
                {
                    bool result = WatcherInit<TermometerGrainbar>(ref _grainbarDataWatcher, GetReportsPathFromReg(ProgramType.Grainbar), Constants.GRAINBAR_FILE_EXTENSION);
                    if (!result)
                        return;
                    await CheckDirectoryForNewData<TermometerGrainbar>(GetReportsPathFromReg(ProgramType.Grainbar), Constants.GRAINBAR_FILE_EXTENSION);
                }
                else
                    DisposeWatcher<TermometerGrainbar>(_grainbarDataWatcher);
            }
        }

        private string GetReportsPathFromReg(ProgramType programType)
        {
            string regKeyPath = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_PATH_REGKEY : Constants.GRAINBAR_REPORTS_PATH_REGKEY;
            string regKeyValue = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_FOLDER_PATH : Constants.AGROLOG_REPORTS_FOLDER_PATH;

            return Internal.CheckRegistrySettings(regKeyPath, Constants.SETTINGS_LOCATION, regKeyValue);
        }

        private async void FileSystemWatcher_OnCreated<T>(object sender, FileSystemEventArgs e) where T : ITermometer
        {
            string path = e.FullPath.Replace(e.Name, string.Empty);
            string dataFileName = DataFileName<T>();
            var disp = Dispatcher;
            bool dataIsNew = false;
            DataHandlingLock<T>.SyncLock.WaitOne();

            List<T> dataList = await ReadDataFile<T>(dataFileName, true);

            if (dataList == null)
                return;

            dataIsNew = await NewDataVerification<T>(path, e.Name, dataList, true);

            if (dataIsNew)
            {
                LogMaker.Log(string.Format("Данные приняты. Сохраняем их в файле \"{0}\".", dataFileName), false);
                await WriteDataFile<T>(dataList, Constants.APPLICATION_DATA_FOLDER_PATH, dataFileName, true);
            }

            DataHandlingLock<T>.SyncLock.Release();
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            LogMaker.Log("Слишком много файлов за один раз. Буфер увеличен в 2 раза. Удалите файлы из каталога и попробуйте еще раз.", true);
            (sender as FileSystemWatcher).InternalBufferSize = (sender as FileSystemWatcher).InternalBufferSize * 2;
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void NewDataWatcherWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.EqualsAny(nameof(IsAgrologDataCollect), nameof(IsGrainbarDataCollect)))
                WatcherInitChange(e.PropertyName);
        }


        public event PropertyChangedEventHandler PropertyChanged;


        FileSystemWatcher _agrologDataWatcher;
        FileSystemWatcher _grainbarDataWatcher;
        bool _isAgrologDataCollect = false;
        bool _isGrainbarDataCollect = false;
        bool _isAutostart = true;
        bool _isDataSubstitution = false;

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

    }
}
