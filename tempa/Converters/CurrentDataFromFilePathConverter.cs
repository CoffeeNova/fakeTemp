using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CoffeeJelly.tempadll.Extensions;

namespace CoffeeJelly.tempa.Converters
{
    class CurrentDataFromFilePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (value as string);
            Debug.Assert(!string.IsNullOrEmpty(path), "value should be not empty string.");
            Debug.Assert(System.IO.Path.HasExtension(path),
                    "value should be a path format fith extension.");

            string dataFileName;
            path.PathRemoveLastPart(out dataFileName);
            //if (dataFileName == Constants.AGROLOG_DATA_FILE
            //    || dataFileName == Constants.GRAINBAR_DATA_FILE)
            //    return Constants.ARCHIVE_DATA_CURRENT_INSCRIPTION;

            try
            {
                var dateRange = dataFileName.Split(' ').First();
                var initDate = dateRange.Split('-').First();
                return DateTime.Parse(initDate).Year.ToString();
            }
            catch
            {
                return Constants.ARCHIVE_DATA_CURRENT_INSCRIPTION;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
