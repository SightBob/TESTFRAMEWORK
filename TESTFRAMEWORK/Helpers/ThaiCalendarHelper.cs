using System;

namespace TESTFRAMEWORK.Helpers
{
    public static class ThaiCalendarHelper
    {
        /// <summary>
        /// แปลงปี พ.ศ. เป็น ค.ศ. (ลบ 543) หากปีมากกว่า 2500
        /// </summary>
        public static DateTime? ToGregorian(DateTime? thaiDate)
        {
            if (thaiDate == null)
                return null;

            if (thaiDate.Value.Year > ProjectConstants.ThaiYearThreshold)
                return thaiDate.Value.AddYears(-ProjectConstants.BuddhistOffset);

            return thaiDate;
        }

        /// <summary>
        /// แปลงปี พ.ศ. เป็น ค.ศ. หรือคืนค่า default ถ้าเป็น null
        /// </summary>
        public static DateTime ToGregorianOrDefault(DateTime? thaiDate, DateTime defaultValue)
        {
            return ToGregorian(thaiDate) ?? defaultValue;
        }

        /// <summary>
        /// แปลงปี พ.ศ. เป็น ค.ศ. แล้วจัดรูปแบบเป็น string หรือคืนค่า "-" ถ้าเป็น null
        /// </summary>
        public static string ToGregorianString(DateTime? thaiDate, string format = "dd/MM/yyyy")
        {
            var gregorian = ToGregorian(thaiDate);
            return gregorian.HasValue ? gregorian.Value.ToString(format) : "-";
        }
    }
}
