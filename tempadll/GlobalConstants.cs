using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoffeeJelly.tempadll.Extensions;

namespace CoffeeJelly.tempadll
{
    public static class Constants
    {
        public const string ROOT_APPLICATION_NAMESPACE = "CoffeeJelly.tempa";
        public const string APPLICATION_NAME = "TermoReporter";
        public const string COMPANY_NAME = "ОАО БЕЛСОЛОД";
        public const string APPLICATION_DESCRIPTION = "Программа преобразования данных программ термометрии Agrolog и Грейнбар и построения графиков.";
        public const string SETTINGS_LOCATION = @"SOFTWARE\" + APPLICATION_NAME;
        public const string AGROLOG_FOLDER_NAME = "Agrolog";
        public const string GRAINBAR_FOLDER_NAME = "Grainbar";
        public const string APPLICATION_DATA_FOLDER = "Data";
        public const string APPLICATION_EXCEL_REPORT_FOLDER = "Reports";
        public const string APPLICATION_ARCHIVE_DATA_FOLDER = "Archive data";
        public const string AGROLOG_DATA_FILE = "Agrolog.dat";
        public const string GRAINBAR_DATA_FILE = "Grainbar.dat";
        public const string AGROLOG_EXCEL_REPORT_FILE_NAME = "Agrolog report.xlsm";
        public const string GRAINBAR_EXCEL_REPORT_FILE_NAME = "Grainbar report.xlsm";
        public const string AGROLOG_REPORTS_PATH_REGKEY = "agrologReports";
        public const string GRAINBAR_REPORTS_PATH_REGKEY = "grainbarReports";
        public const string ACTIVE_AGROLOG_DATA_PATH_REGKEY = "activeAgrologData";
        public const string ACTIVE_GRAINBAR_DATA_PATH_REGKEY = "activeGrainbarData";

        public const string IS_AGROLOG_DATA_COLLECT_REGKEY = "isAgrologDataCollect";
        public const string IS_GRAINBAR_DATA_COLLECT_REGKEY = "isGrainbarDataCollect";
        public const string IS_AUTOSTART_REGKEY = "isAutoStart";
        public const string IS_DATA_SUBSTITUTION_REGKEY = "isDataSubstitution";

        public const string AGROLOG_FILE_EXTENSION = "csv";
        public const string GRAINBAR_FILE_EXTENSION = "txt";
        public const string AGROLOG_PROGRAM_NAME = "Agrolog";
        public const string GRAINBAR_PROGRAM_NAME = "Грейнбар";
        public const string NEW_FOLDER_TEXT_BOX_INITIAL_TEXT = "Новая папка";
        public const string AGROLOG_PATTERN_REPORT_NAME = "AgrologPatternReport.csv";
        public const string GRAINBAR_PATTERN_REPORT_NAME = "GrainbarPatternReport.txt";
        public const string AGROLOG_MANIFEST_RESOURSE = "AgrologPatternReport";
        public const string GRAINBAR_MANIFEST_RESOURSE = "GrainbarPatternReport";
        //internal const string EXCEL_TEMPLATE_REPORT_NAME = "tempa.other_files.reportTemplate.xlsm";
        public const string EXCEL_TEMPLATE_REPORT_TEMP_NAME = "reportTemplate.xlsm";
        public const string EXCEL_TEMPLATE_FAKE_REPORT_TEMP_NAME = "fakeReportTemplate.xlsm";

        public const string ARCHIVE_DATA_FILE_NAME_DATE_FORMAT = "dd.MM.yy";
        public const string ARCHIVE_DATA_CURRENT_INSCRIPTION = "текущий";

        public static readonly string APPLICATION_DIRECTORY = System.IO.Directory.GetCurrentDirectory();
        public static readonly string AGROLOG_REPORTS_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + AGROLOG_FOLDER_NAME;
        public static readonly string GRAINBAR_REPORTS_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + GRAINBAR_FOLDER_NAME;
      
        public static readonly string APPLICATION_DATA_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + 
            APPLICATION_DATA_FOLDER;
        //public static readonly string APPLICATION_AGROLOG_DATA_FOLDER_PATH = APPLICATION_DATA_FOLDER_PATH.PathFormatter() +
        //    AGROLOG_FOLDER_NAME;
        //public static readonly string APPLICATION_GRAINBAR_DATA_FOLDER_PATH = APPLICATION_DATA_FOLDER_PATH.PathFormatter() +
        //    GRAINBAR_FOLDER_NAME;
        public static readonly string APPLICATION_ARCHIVE_AGROLOG_DATA_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() +
            APPLICATION_ARCHIVE_DATA_FOLDER.PathFormatter() + AGROLOG_FOLDER_NAME;
        public static readonly string APPLICATION_ARCHIVE_GRAINBAR_DATA_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() +
            APPLICATION_ARCHIVE_DATA_FOLDER.PathFormatter() + GRAINBAR_FOLDER_NAME;

        public static readonly string APPLICATION_AGROLOG_DATA_FILE_PATH = APPLICATION_DATA_FOLDER_PATH.PathFormatter() +
            AGROLOG_DATA_FILE;
        public static readonly string APPLICATION_GRAINBAR_DATA_FILE_PATH = APPLICATION_DATA_FOLDER_PATH.PathFormatter() +
            GRAINBAR_DATA_FILE;

        public static readonly string EXCEL_REPORT_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + 
            APPLICATION_EXCEL_REPORT_FOLDER;
        public static readonly string EXCEL_TEMPLATE_REPORT_NAME = ROOT_APPLICATION_NAMESPACE + 
            ".other_files." + EXCEL_TEMPLATE_REPORT_TEMP_NAME;

    }
}
