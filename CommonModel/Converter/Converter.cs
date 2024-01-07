using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using LiveCharts;
using CommonModel.Model;

namespace CommonModel.Converter
{
    public class DateTimeToStringConverter : IValueConverter
    {
        // DateTime을 문자열로 변환하는 메서드
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ChartValues<DateTime> dateTime = (ChartValues<DateTime>)value;
            string format = parameter as string;

            if (!string.IsNullOrEmpty(format))
            {
                return dateTime[0].ToString(format);
            }
            else
            {
                return dateTime.ToString();
            }
        }

        // 문자열을 DateTime으로 변환하는 메서드
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;

            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

    






}
