using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tempa.Extensions;

namespace tempa
{
    internal static class Constants
    {
        internal const string APPLICATION_NAME = "Tempafake";
        internal const string COMPANY_NAME = "ОАО БЕЛСОЛОД";
        internal const string SETTINGS_LOCATION = @"SOFTWARE\" + APPLICATION_NAME;
        internal const string AGROLOG_FOLDER_NAME = "Agrolog";
        internal const string GRAINBAR_FOLDER_NAME = "Grainbar";
        internal const string APPLICATION_DATA_FOLDER = "Data";
        internal const string APPLICATION_REPORTS_FOLDER = "Reports";
        internal const string AGROLOG_ACTIVE_DATA_FILE = "Agrolog.dat";
        internal const string GRAINBAR_ACTIVE_DATA_FILE = "Grainbar.dat";
        internal const string AGROLOG_REPORT_FILE_NAME = "Agrolog report.xls";
        internal const string GRAINBAR_REPORT_FILE_NAME = "Grainbar report.xls";
        internal const string AGROLOG_REPORTS_PATH_REGKEY = "agrologReports";
        internal const string GRAINBAR_REPORTS_PATH_REGKEY = "grainbarReports";
        internal const string IS_AGROLOG_DATA_COLLECT_REGKEY = "isAgrologDataCollect";
        internal const string IS_GRAINBAR_DATA_COLLECT_REGKEY = "isGrainbarDataCollect";
        internal const string IS_AUTOSTART_REGKEY = "isAutoStart";
        internal const string IS_DATA_SUBSTITUTION_REGKEY = "isDataSubstitution";
        internal const string AGROLOG_FILE_EXTENSION = "csv";
        internal const string GRAINBAR_FILE_EXTENSION = "txt";
        internal const string AGROLOG_PROGRAM_NAME = "Agrolog";
        internal const string GRAINBAR_PROGRAM_NAME = "Грейнбар";

        internal static readonly string APPLICATION_DIRECTORY = System.IO.Directory.GetCurrentDirectory();
        internal static readonly string AGROLOG_REPORTS_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + AGROLOG_FOLDER_NAME;
        internal static readonly string GRAINBAR_REPORTS_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + GRAINBAR_FOLDER_NAME;
        internal static readonly string APPLICATION_DATA_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + APPLICATION_DATA_FOLDER;
        internal static readonly string APPLICATION_REPORT_FOLDER_PATH = APPLICATION_DIRECTORY.PathFormatter() + APPLICATION_REPORTS_FOLDER;
    }
}
