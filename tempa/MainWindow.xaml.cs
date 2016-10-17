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

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            ManualInitializing();
            Settings();
            _log.Info(string.Format("{0} is started successfully.", Constants.APPLICATION_NAME));
        }

        private void Settings()
        {
            AgrologReportsPath = Internal.CheckRegistrySettings(Constants.AGROLOG_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.AGROLOG_REPORTS_FOLDER_PATH);
            GrainbarReportsPath = Internal.CheckRegistrySettings(Constants.GRAINBAR_REPORTS_PATH_REGKEY, Constants.SETTINGS_LOCATION, Constants.GRAINBAR_REPORTS_FOLDER_PATH);
            
        }

        private void ManualInitializing()
        {
            LogTextBlock.Inlines.Clear();
            LogMaker.newMessage += LogMaker_newMessage;
            AgrologFileBrowsButt.Tag = ProgramType.Agrolog;
            GrainbarFileBrowsButt.Tag = ProgramType.Grainbar;
            SettingsShow += MainWindow_onSettingsShow;
            AboutShow += MainWindow_AboutShow;
            AboutHide += MainWindow_AboutHide;
            CreateReportShow += MainWindow_CreateReportShow;
            CreateReportHide += MainWindow_CreateReportHide;

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

        private Task OpenPlotAsync<T>(string dataFolderPath, string dataFileName) where T : ITermometer
        {
            var tcs = new TaskCompletionSource<object>();
            var newWindowThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    OpenPlotCallBack<T>(dataFolderPath, dataFileName);
                    tcs.SetResult(null);

                }
                catch (PlotDataException ex)
                {
                    LogMaker.InvokedLog(string.Format("Нет данных для построения графика \"{0}\", cм. Error.log.", Internal.GetProgramName<T>()), true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    tcs.SetResult(null);
                    //tcs.SetException(ex);
                }
                catch (Exception ex)
                {
                    LogMaker.InvokedLog(string.Format("Не получилось построить график \"{0}\", cм. Error.log.", Internal.GetProgramName<T>()), true, this.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    tcs.SetResult(null);
                    //tcs.SetException(ex);
                }
            }));

            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
            return tcs.Task;
        }


        private void SaveReportsPathToRegistry(ProgramType programType)
        {
            string regKey = programType == ProgramType.Agrolog ? Constants.AGROLOG_REPORTS_PATH_REGKEY : Constants.GRAINBAR_REPORTS_PATH_REGKEY;
            string savedValue = programType == ProgramType.Agrolog ? AgrologReportsPath : GrainbarReportsPath;
            try //сохраним в реестре последний выбранный путь
            {
                Internal.SaveRegistrySettings(regKey, Constants.SETTINGS_LOCATION, savedValue);
            }
            catch (InvalidOperationException ex)
            {
                LogMaker.Log(string.Format("Невозможно сохранить настройки в реестр. См. Error.log"), true);
                ExceptionHandler.Handle(ex, false);
            }
        }

        private void StartDataCollect(ProgramType programType)
        {
            if (programType == ProgramType.Agrolog)
            {
                if (agrologDataChb.IsChecked.Value)
                {
                    agrologDataChb.IsChecked = false;
                    agrologDataChb.IsChecked = true;
                }
            }
            else if (programType == ProgramType.Grainbar)
                if (grainbarDataChb.IsChecked.Value)
                {
                    grainbarDataChb.IsChecked = false;
                    grainbarDataChb.IsChecked = true;
                }
        }

        //----------------------------------------------------------------------------------------------------------
        #region Callbacks

        private void OpenPlotCallBack<T>(string dataFolderPath, string dataFileName) where T : ITermometer
        {
            List<T> reportData = null;

            try
            {
                LogMaker.InvokedLog(string.Format("Чтение данных из файла \"{0}\"", dataFileName), false, this.Dispatcher);
                reportData = DataWorker.ReadBinary<T>(dataFolderPath, dataFileName);
                LogMaker.InvokedLog(string.Format("Построение графика \"{0}\"", Internal.GetProgramName<T>()), false, this.Dispatcher);

                List<Termometer> data = new List<Termometer>();
                data = reportData.Select(t => t as Termometer).ToList();

                var func = new Func<MainPlotWindow>(() =>
                {
                    var p = new MainPlotWindow(data);
                    p.Show();
                    p.Closed += (sender, e) => ClosePlotCallback(sender, e, Internal.GetProgramName<T>());
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
                    _agrologPlot.Close();
                    _agrologPlot.Dispatcher.InvokeShutdown();
                }
                else if (typeof(T) == typeof(TermometerGrainbar))
                {
                    _grainbarPlot.Close();
                    _grainbarPlot.Dispatcher.InvokeShutdown();
                }
                throw new Exception("Abort thread Exception", ex);
            }

        }

        private void ClosePlotCallback(object sender, EventArgs e, string plotName)
        {
            LogMaker.InvokedLog(string.Format("Закрытие окна графика \"{0}\"", plotName), false, this.Dispatcher);
            (sender as MainPlotWindow).Dispatcher.InvokeShutdown();
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
            IsFileBrowsTreeOnForm = false;
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


        private void FileBrowsOkButt_Click(object sender, RoutedEventArgs e)
        {
            var programType = (ProgramType)FileBrowsTreeView.Tag;
            SaveReportsPathToRegistry(programType);
            StartDataCollect(programType);
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
            this.Hide();
        }

        private void Program_Closed(object sender, EventArgs e)
        {

        }

        void MainWindow_onSettingsShow(object sender, RoutedEventArgs e)
        {
            IsSettingsGridOnForm = true;
        }


        private void MainWindow_AboutShow(object sender, RoutedEventArgs e)
        {
            IsAboutOnForm = true;
        }


        private void MainWindow_AboutHide(object sender, RoutedEventArgs e)
        {
            IsAboutOnForm = false;
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
                    Internal.SaveRegistrySettings(Constants.IS_AGROLOG_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, agrologDataChb.IsChecked.Value);
                    Internal.SaveRegistrySettings(Constants.IS_GRAINBAR_DATA_COLLECT_REGKEY, Constants.SETTINGS_LOCATION, grainbarDataChb.IsChecked.Value);
                    Internal.SaveRegistrySettings(Constants.IS_AUTOSTART_REGKEY, Constants.SETTINGS_LOCATION, autostartChb.IsChecked.Value);
                    Internal.SaveRegistrySettings(Constants.IS_DATA_SUBSTITUTION_REGKEY, Constants.SETTINGS_LOCATION, dataSubstitutionChb.IsChecked.Value);
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

        //private async void dataChb_Checked(object sender, RoutedEventArgs e)
        //{
        //    var checkBox = sender as CheckBox;
        //    checkBox.IsEnabled = false;
           
        //    checkBox.IsEnabled = true;
        //}

        //private void dataChb_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if ((sender as CheckBox).Name == "agrologDataChb")
        //        DisposeWatcher<TermometerAgrolog>(_agrologFolderWatcher);
        //    else DisposeWatcher<TermometerGrainbar>(_grainbarFolderWatcher);
        //}


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
            var textBox = sender as TextBox;
            var programType = textBox.Name == "AgrologFilesPathTextBox" ? ProgramType.Agrolog : ProgramType.Grainbar;
            var propertyName = textBox.Name == "AgrologFilesPathTextBox" ? "AgrologReportsPath" : "GrainbarReportsPath";

            if (textBox.Text == (string)this.GetPropertyValue(propertyName))
                return;
            else
                this.SetPropertyValue(propertyName, textBox.Text);

            SaveReportsPathToRegistry(programType);
            StartDataCollect(programType);
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        private async void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (button == AgrologPlotButton)
                await OpenPlotAsync<TermometerAgrolog>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.AGROLOG_DATA_FILE);
            else if (button == GrainbarPlotButton)
                await OpenPlotAsync<TermometerGrainbar>(Constants.APPLICATION_DATA_FOLDER_PATH, Constants.GRAINBAR_DATA_FILE);
            button.IsEnabled = true;

        }

        private void AboutOKButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(MainWindow.AboutHideEvent, this));
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

        public static readonly RoutedEvent AboutShowEvent = EventManager.RegisterRoutedEvent(
        "AboutShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler AboutShow
        {
            add { AddHandler(AboutShowEvent, value); }
            remove { RemoveHandler(AboutShowEvent, value); }
        }

        public static readonly RoutedEvent AboutHideEvent = EventManager.RegisterRoutedEvent(
        "AboutHide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler AboutHide
        {
            add { AddHandler(AboutHideEvent, value); }
            remove { RemoveHandler(AboutHideEvent, value); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region fields

        public bool IsFileBrowsTreeOnForm = false;                 //на форме ли окно выбора файлов
        public bool IsSettingsGridOnForm = false;
        public bool IsCreateReportWindowShow = false;
        public bool IsAboutOnForm = false;

        string _agrologReportsPath;
        string _grainbarReportsPath;
        readonly Logger _log = LogManager.GetCurrentClassLogger();
        readonly ManualResetEvent _createReportResetEvent = new ManualResetEvent(false);
        CancellationTokenSource _createReportCancellationToken;
        MainPlotWindow _agrologPlot;
        MainPlotWindow _grainbarPlot;
        #endregion

        #region properties

        public string AgrologReportsPath
        {
            get { return _agrologReportsPath; }
            private set
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
            private set
            {
                if (string.IsNullOrEmpty(value))
                    _grainbarReportsPath = Constants.AGROLOG_REPORTS_FOLDER_PATH;
                else _grainbarReportsPath = value;
                NotifyPropertyChanged();
            }
        }

        public string AssemblyVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.FileVersion;
            }
        }

        public string ApplicationName
        {
            get
            {
                return Constants.APPLICATION_NAME;
            }
        }

        public string Description
        {
            get
            {
                return Constants.APPLICATION_DESCRIPTION;
            }
        }
        #endregion

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 50; i++)
                LogMaker.Log(string.Format("test"), i % 2 == 0);

        }

    }
}
