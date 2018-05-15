using System;
using System.Collections.Generic;
using System.Text;

namespace FilesManager.OficceTasks.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly string[] monthsPTBR = { "janeiro", "fevereiro", "maarço", "abril", "maio", "junho",
                                                        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };

        /// <summary>
        /// This method converts 26 de agosto de 2016 in 26/08/2016
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ParseDate(this string date)
        {
            if (string.IsNullOrEmpty(date)) return string.Empty;

            try
            {
                var parts = date.Split(' ');
                return new DateTime(int.Parse(parts[4]), Array.IndexOf(monthsPTBR, parts[2].ToLower()) + 1, int.Parse(parts[0])).ToString("dd/MM/yyyy");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
