using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using tempa.Extensions;
using tempa.Exceptions;

namespace tempa
{
    public static partial class DataWorker
    {
        public static void CreateNewExcelReportAsync<T>(string repotPath, string reportFileName, string templatePath, string templateFileName, List<T> reportData) where T : ITermometer
        {
            Task.Factory.StartNew(() => CreateNewExcelReport<T>(repotPath, reportFileName, templatePath, templateFileName, reportData));
        }

        public static void CreateNewExcelReport<T>(string repotPath, string reportFileName, string templatePath, string templateFileName, List<T> reportData) where T : ITermometer
        {
            FileInfo templateFileInfo;
            try
            {
                templateFileInfo = new FileInfo(templatePath.PathFormatter() + templateFileName);
                if (!templateFileInfo.Exists)
                    throw new InvalidOperationException("Template file doesn't exists!");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can't get template file.", ex);
            }
            var reportFileInfo = new FileInfo(repotPath.PathFormatter() + reportFileName);
            GenerateReportHeader<T>(reportFileInfo, templateFileInfo, reportData);
            reportFileInfo.Refresh();

            WriteExcelReport<T>(repotPath, reportFileName, reportData);
        }

        public static Task WriteExcelReportAsync<T>(string repotPath, string reportFileName, List<T> reportData) where T : ITermometer
        {
            return Task.Factory.StartNew(() => WriteExcelReport<T>(repotPath, reportFileName, reportData));
        }

        public static void WriteExcelReport<T>(string repotPath, string reportFileName, List<T> reportData) where T : ITermometer
        {
            try
            {
                var reportFileInfo = new FileInfo(repotPath.PathFormatter() + reportFileName);
                if (!reportFileInfo.Exists)
                    throw new ReportFileException("Report file doesn't exists!");

                using (var pck = new ExcelPackage(reportFileInfo))
                {
                    ExcelWorksheet reportWorksheet = pck.Workbook.Worksheets[1];
                    ExcelWorksheet dataWorkSheet = pck.Workbook.Worksheets[2];
                    ExcelWorksheet configWorkSheet = pck.Workbook.Worksheets[3];

                    //find last empty column
                    int lastColumn = dataWorkSheet.Dimension.End.Column;
                    while(dataWorkSheet.Cells[DataSheet.DATE_ROW, lastColumn].Text.EqualsAny(new string[]{string.Empty, SPECIAL_BLANK_VALUE_SYMBOL}))
                        lastColumn--;

                    int lastRow = dataWorkSheet.Dimension.End.Row;
                    int sensorsCount = typeof(T) == typeof(TermometerAgrolog) ? TermometerAgrolog.Sensors : TermometerGrainbar.Sensors;

                    //recieve the last record from an excel report and remove earlier from reportData
                    DateTime lastDate = dataWorkSheet.Cells[DataSheet.DATE_ROW, lastColumn].GetValue<DateTime>();
                    reportData.RemoveAll(t => t.MeasurementDate <= lastDate);

                    for (int rep = 0; rep < reportData.Count; rep ++ )
                    {
                        lastColumn++;
                        DateTime date = reportData.First().MeasurementDate;
                        dataWorkSheet.Cells[DataSheet.DATE_ROW, lastColumn].Value = date;
                        for (int i = DataSheet.CAPTION_DATA_START_ROW; i <= lastRow; )
                        {
                            string silo = dataWorkSheet.Cells[i, DataSheet.SILO_COL].Text; ;
                            string cable = dataWorkSheet.Cells[i, DataSheet.CABLE_COL].Text;
                            string sensor = dataWorkSheet.Cells[i, DataSheet.SENSOR_COL].Text;

                            var termometer = (from item in reportData
                                              where item.MeasurementDate == date
                                              where item.Silo == silo
                                              where item.Cable == cable
                                              select item).FirstOrDefault();

                            for (int j = 0; j < sensorsCount; j++)
                            {
                                string writeValue;
                                if (termometer == null || termometer.Sensor[j] == null)
                                    writeValue = SPECIAL_NULL_VALUE_SYMBOL;
                                else
                                    writeValue = termometer.Sensor[j].ToString();
                                dataWorkSheet.Cells[i, lastColumn].Value = writeValue;
                                i++;
                            }
                        }
                        reportData.RemoveAll(t => t.MeasurementDate == date);
                    }

                    int tableWidth = configWorkSheet.Cells[ConfigSheet.TableSize.Width.Row, ConfigSheet.TableSize.Width.Col].GetValue<int>();
                    int maxValueHorScrollBar = lastColumn - DataSheet.DATE_COL - tableWidth > 0 ? lastColumn - DataSheet.DATE_COL - tableWidth : 0;
                    configWorkSheet.Cells[ConfigSheet.HorScrollBar.MaxValue.Row, ConfigSheet.HorScrollBar.MaxValue.Col].Value = maxValueHorScrollBar;
                    pck.Save();
                }
            }
            catch (Exception ex)
            {
                throw new WriteReportException("Can't write report.", ex);
            }
        }

        private static void GenerateReportHeader<T>(FileInfo reportFileInfo, FileInfo templateFileInfo, List<T> reportData) where T : ITermometer
        {
            var programType = typeof(T);
            string thermometryName = programType == typeof(TermometerAgrolog) ? "Agrolog" : "ГРЕЙНБАР";
            string secondColumnCaption = programType == typeof(TermometerAgrolog) ? "Линия" : "Ш.М.П.";
            using (var pck = new ExcelPackage(reportFileInfo, templateFileInfo))
            {
                //Generate first worksheet texts.
                ExcelWorksheet reportWorksheet = pck.Workbook.Worksheets[1];
                ExcelWorksheet dataWorkSheet = pck.Workbook.Worksheets[2];
                ExcelWorksheet configWorkSheet = pck.Workbook.Worksheets[3];

                var currentDate = DateTime.Now;
                reportWorksheet.Cells[ ReportSheet.HEADER_ROW, ReportSheet.HEADER_COL].Value = string.Format("Отчет сформирован по данным термометрии {0}.", thermometryName);
                reportWorksheet.Cells[ReportSheet.HEADER_ROW + 1, ReportSheet.HEADER_COL].Value = string.Format("Время последней модификации отчета: {0}.", currentDate.ToString("hh:mm:ss dd.MM.yyyy"));
                reportWorksheet.Cells[ReportSheet.HEADER_ROW + 2, ReportSheet.HEADER_COL].Value = string.Format("{0}.   {1}.", Constants.APPLICATION_NAME, Constants.COMPANY_NAME);
                reportWorksheet.Cells[ReportSheet.TABLE_CAPTIONS_ROW, ReportSheet.SILO_COL].Value = "Силос";
                reportWorksheet.Cells[ReportSheet.TABLE_CAPTIONS_ROW, ReportSheet.CABLE_COL].Value = secondColumnCaption;
                reportWorksheet.Cells[ReportSheet.TABLE_CAPTIONS_ROW, ReportSheet.SENSOR_COL].Value = "Датчик";
                reportWorksheet.Cells[ReportSheet.DATE_COL, ReportSheet.DATE_ROW].Value = "Значения по времени";

                //Generate second worksheet (data worksheet) silo, cable, sensor names.
                DateTime earliestDateInReport = reportData.OrderBy(r => r.MeasurementDate).First().MeasurementDate;
                List<T> oneTimeData = reportData.FindAll(r => r.MeasurementDate == earliestDateInReport);
                GenerateCaptionsDataSheet<T>(dataWorkSheet, DataSheet.CAPTION_DATA_START_ROW, DataSheet.SILO_COL, DataSheet.CABLE_COL, DataSheet.SENSOR_COL, oneTimeData);
                int tableWidth = configWorkSheet.Cells[ConfigSheet.TableSize.Width.Row, ConfigSheet.TableSize.Width.Col].GetValue<int>();
                InitialFillDataSheetSpetialSymbols(dataWorkSheet, tableWidth);

                //Generate third worksheet (config)

                int tableHeight = configWorkSheet.Cells[ConfigSheet.TableSize.Height.Row, ConfigSheet.TableSize.Height.Col].GetValue<int>();

                int sensorsCount = oneTimeData.First().Sensor.Count();
                int maxValueVertScrollBar = oneTimeData.Count * sensorsCount - tableHeight > 0 ? oneTimeData.Count * sensorsCount - tableHeight : 0;
                configWorkSheet.Cells[ConfigSheet.VertScrollBar.MaxValue.Row, ConfigSheet.VertScrollBar.MaxValue.Col].Value = maxValueVertScrollBar;
                pck.Save();
            }
        }

        private static void GenerateCaptionsDataSheet<T>(ExcelWorksheet dataWorkSheet, int startRow, int startSiloCol, int startCableCol, int startSensorCol, List<T> reportData) where T : ITermometer
        {
            int i = 0;
            foreach (T termometr in reportData)
            {
                int j = 1;
                foreach (var sensor in termometr.Sensor)
                {
                    dataWorkSheet.Cells[startRow + i, startSiloCol].Value = termometr.Silo;
                    dataWorkSheet.Cells[startRow + i, startCableCol].Value = termometr.Cable;
                    dataWorkSheet.Cells[startRow + i, startSensorCol].Value = j;
                    i++;
                    j++;
                }
            }
        }

        private static void InitialFillDataSheetSpetialSymbols(ExcelWorksheet dataWorkSheet, int width)
        {
            int lastRow = dataWorkSheet.Dimension.End.Row;
            int lastCol = dataWorkSheet.Dimension.End.Column;
            for (int col = lastCol + 2; col <= lastCol + width + 1; col++)
                for (int row = DataSheet.DATE_ROW; row <= lastRow; row++)
                    dataWorkSheet.Cells[row, col].Value = SPECIAL_BLANK_VALUE_SYMBOL;
        }

        private struct ReportSheet
        {
            public const int HEADER_ROW = 1;
            public const int HEADER_COL = 1;
            public const int SILO_COL = 2;
            public const int CABLE_COL = 3;
            public const int SENSOR_COL = 4;
            public const int DATE_ROW = 5;
            public const int DATE_COL = 5;
            public const int TABLE_CAPTIONS_ROW = 7;
        }

        private struct DataSheet
        {
            public const int SILO_COL = 1;
            public const int CABLE_COL = 2;
            public const int SENSOR_COL = 3;
            public const int CAPTION_DATA_START_ROW = 2;
            public const int DATE_ROW = 1;
            public const int DATE_COL = 4;
        }

        private struct ConfigSheet
        {
            public struct VertScrollBar
            {
                public struct MaxValue
                {
                    public const int Row = 5;
                    public const int Col = 2;
                }
                public struct CurrentValue
                {
                    public const int Row = 4;
                    public const int Col = 2;
                }
            }

            public struct HorScrollBar
            {
                public struct MaxValue
                {
                    public const int Row = 3;
                    public const int Col = 2;
                }
                public struct CurrentValue
                {
                    public const int Row = 2;
                    public const int Col = 2;
                }
            }
            public struct TableSize
            {
                public struct Height
                {
                    public const int Row = 8;
                    public const int Col = 2;
                }
                public struct Width
                {
                    public const int Row = 9;
                    public const int Col = 2;
                }
            }
        }

        private const string SPECIAL_BLANK_VALUE_SYMBOL = "#";
        private const string SPECIAL_NULL_VALUE_SYMBOL = "-";
    }

}
