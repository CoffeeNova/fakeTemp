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
        public static void CreateNewReportAsync<T>(string repotPath, string reportFileName, string templatePath, string templateFileName, List<T> reportData) where T : ITermometer
        {
            Task.Factory.StartNew(() => CreateNewReport<T>(repotPath, reportFileName, templatePath, templateFileName, reportData));
        }

        public static void CreateNewReport<T>(string repotPath, string reportFileName, string templatePath, string templateFileName, List<T> reportData) where T : ITermometer
        {
            FileInfo templateFileInfo;
            try
            {
                templateFileInfo = new FileInfo(templatePath.PathFormatter() + templateFileName);
                if (!templateFileInfo.Exists)
                    throw new InvalidOperationException("Template file doesn't exists!");
                var newFileInfo = new FileInfo(repotPath.PathFormatter() + reportFileName);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can't get template file.", ex);
            }
            var reportFileInfo = new FileInfo(repotPath.PathFormatter() + reportFileName);
            GenerateReportHeader<T>(reportFileInfo, templateFileInfo, reportData);
            reportFileInfo.Refresh();

            WriteReport<T>(repotPath, reportFileName, reportData);
        }

        public static Task WriteReportAsync<T>(string repotPath, string reportFileName, List<T> reportData) where T : ITermometer
        {
            return Task.Factory.StartNew(() => WriteReport<T>(repotPath, reportFileName, reportData));
        }

        public static void WriteReport<T>(string repotPath, string reportFileName, List<T> reportData) where T : ITermometer
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

                    int lastColumn = dataWorkSheet.Dimension.End.Column;
                    int lastRow = dataWorkSheet.Dimension.End.Row;
                    int sensorsCount = typeof(T) == typeof(TermometerAgrolog) ? TermometerAgrolog.Sensors : TermometerGrainbar.Sensors;

                    foreach (var first in reportData)
                    {
                        lastColumn++;
                        DateTime date = first.MeasurementDate;
                        dataWorkSheet.Cells[WS2Positions.DATE_ROW, lastColumn].Value = date;
                        for (int i = WS2Positions.CAPTION_DATA_START_ROW; i <= lastRow; i++)
                        {
                            string silo = dataWorkSheet.Cells[i, WS2Positions.SILO_COL].Text; ;
                            string cable = dataWorkSheet.Cells[i, WS2Positions.CABLE_COL].Text;
                            string sensor = dataWorkSheet.Cells[i, WS2Positions.SENSOR_COL].Text;

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
                            }
                        }
                        reportData.RemoveAll(t => t.MeasurementDate == date);
                    }

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
            string secondColumnCaption = programType == typeof(TermometerAgrolog) ? "Т-подвеска" : "Ш.М.П.";
            using (var pck = new ExcelPackage(reportFileInfo, templateFileInfo))
            {
                //Generate first worksheet texts.
                ExcelWorksheet reportWorksheet = pck.Workbook.Worksheets[1];
                var currentDate = DateTime.Now;
                reportWorksheet.Cells[WS1Positions.HEADER_COL, WS1Positions.HEADER_ROW].Value = string.Format("Отчет сформирован по данным термометрии {0}.", thermometryName);
                reportWorksheet.Cells[WS1Positions.HEADER_COL, WS1Positions.HEADER_ROW + 1].Value = string.Format("Время последней модификации отчета: {0}.", currentDate.ToString("hh:mm:ss dd.MM.yyyy"));
                reportWorksheet.Cells[WS1Positions.HEADER_COL, WS1Positions.HEADER_ROW + 2].Value = string.Format("{0}.   {1}.", Constants.APPLICATION_NAME, Constants.COMPANY_NAME);
                reportWorksheet.Cells[WS1Positions.SILO_COL, WS1Positions.TABLE_CAPTIONS_ROW].Value = "Силос";
                reportWorksheet.Cells[WS1Positions.CABLE_COL, WS1Positions.TABLE_CAPTIONS_ROW].Value = secondColumnCaption;
                reportWorksheet.Cells[WS1Positions.SENSOR_COL, WS1Positions.TABLE_CAPTIONS_ROW].Value = "Датчик";
                reportWorksheet.Cells[WS1Positions.DATE_COL, WS1Positions.DATE_ROW].Value = "Значения по времени";

                //Generate second worksheet (data worksheet) silo, cable, sensor names.
                ExcelWorksheet dataWorkSheet = pck.Workbook.Worksheets[2];
                DateTime earliestDateInReport = reportData.OrderBy(r => r.MeasurementDate).First().MeasurementDate;
                List<T> oneTimeData = reportData.FindAll(r => r.MeasurementDate == earliestDateInReport);
                GenerateCaptionsDataSheet<T>(ref dataWorkSheet, WS2Positions.CAPTION_DATA_START_ROW, WS2Positions.SILO_COL, WS2Positions.CABLE_COL, WS2Positions.SENSOR_COL, oneTimeData);

                //Generate third worksheet (config)
                ExcelWorksheet configWorkSheet = pck.Workbook.Worksheets[3];

                int tableHeight = (int)configWorkSheet.Cells[Config.TableSize.Height.Row, Config.TableSize.Height.Col].Value;
                int maxValueVertScrollBar = oneTimeData.Count - tableHeight > 0 ? oneTimeData.Count - tableHeight : 0;
                configWorkSheet.Cells[Config.VertScrollBar.MaxValue.Row, Config.VertScrollBar.MaxValue.Col].Value = maxValueVertScrollBar;
                pck.Save();
            }
        }

        private static void GenerateCaptionsDataSheet<T>(ref ExcelWorksheet dataWorkSheet, int startRow, int startSiloCol, int startCableCol, int startSensorCol, List<T> reportData) where T : ITermometer
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

        private struct WS1Positions
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

        private struct WS2Positions
        {
            public const int SILO_COL = 1;
            public const int CABLE_COL = 2;
            public const int SENSOR_COL = 3;
            public const int CAPTION_DATA_START_ROW = 2;
            public const int DATE_ROW = 1;
        }

        private struct Config
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

        private const string SPECIAL_NULL_VALUE_SYMBOL = "#";
    }

}
