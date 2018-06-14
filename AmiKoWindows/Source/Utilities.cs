﻿/*
Copyright (c) ywesee GmbH

This file is part of AmiKo for Windows.

AmiKo for Windows is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace AmiKoWindows
{
    class Utilities
    {
        public static string AppCultureInfoName()
        {
            return System.Globalization.CultureInfo.CurrentUICulture.ToString();
        }

        public static string AppLanguage()
        {
            var culture = AppCultureInfoName();
            if (culture.Equals("de-CH"))
                return "de";
            else if (culture.Equals("fr-CH"))
                return "fr";
            return "de";
        }

        public static string AppName()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != "")
                {
                    return titleAttribute.Title;
                }
            }
            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }

        public static string AppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (version != "")
            {
                return version;
            }
            return "1.0.0";
        }

        public static string AppCompany()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyCompanyAttribute versionAttribute = (AssemblyCompanyAttribute)attributes[0];
                if (versionAttribute.Company != "")
                {
                    return versionAttribute.Company;
                }
            }
            return "Ywesee";
        }

        public static string AppExecutingFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string AppLocalDataFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppCompany(), AppName());
        }

        public static string AppRoamingDataFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppCompany(), AppName());
        }

        public static string SQLiteDBPath()
        {
            return GetDbPath(Constants.AIPS_DB_BASE);
        }

        public static string FrequencyDBPath()
        {
            return GetDbPath(Constants.FREQUENCY_DB_BASE);
        }

        public static string PatientDBPath()
        {
            return GetDbPath(Constants.PATIENT_DB_BASE);
        }

        public static string InteractionsPath()
        {
            string interactionsName = Constants.INTERACTIONS_CSV_BASE + "de.csv";
            if (AppLanguage().Equals("fr"))
                interactionsName = Constants.INTERACTIONS_CSV_BASE + "fr.csv";

            string path = Path.Combine(AppRoamingDataFolder(), interactionsName);
            if (!File.Exists(path))
                path = Path.Combine(AppExecutingFolder(), "Data", interactionsName);
            return path;
        }

        public static string OperatorPictureFilePath()
        {
            string path = Path.Combine(
                AppRoamingDataFolder(), Constants.OPERATOR_PICTURE_FILE);
            return path;
        }

        public static string ReportPath()
        {
            string reportPath = "http://pillbox.oddb.org/amiko_report_de.html";
            if (AppLanguage().Equals("fr"))
                reportPath = "http://pillbox.oddb.org/amiko_report_fr.html";
            return reportPath;
        }

        private static string GetDbPath(string baseName)
        {
            string dbName = baseName + AppLanguage() + ".db";
            string dbPath = Path.Combine(AppRoamingDataFolder(), dbName);
            if (!File.Exists(dbPath))
                dbPath = Path.Combine(AppExecutingFolder(), "Data", dbName);
            return dbPath;
        }

        #region General Functions
        // TitleCase -> snake_case
        public static string ConvertTitleCaseToSnakeCase(string text)
        {
            return string.Concat(text.Select(
                (x, i) => {
                    if (i == 0)
                        return x.ToString().ToLower();
                    else if (char.IsUpper(x))
                        return "_" + x.ToString().ToLower();
                    else
                        return x.ToString();
                }
            ));
        }

        // snake_case -> TitleCase
        public static string ConvertSnakeCaseToTitleCase(string text)
        {
            if (text == null || text.Equals(string.Empty))
                return "";

            string newText = "";
            foreach (string word in text.Split('_'))
            {
                if (!word.Equals(string.Empty))
                    newText += word[0].ToString().ToUpper() + word.Substring(1).ToLower();
            }
            if (newText.Length > 0)
                return newText[0].ToString().ToUpper() + newText.Substring(1);
            return newText;
        }

        public static string GenerateHash(string baseString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(baseString));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hash)
                builder.Append(b.ToString("X2"));
            return builder.ToString();

        }

        public static string GetCurrentTimeInUTC()
        {
            DateTime datetime = DateTime.UtcNow;
            return datetime.ToString("yyyy-MM-dd'T'HH:mm.ss");
        }

        public static string GetLocalTime()
        {
            DateTime datetime = DateTime.Now;
            return datetime.ToString("dd.MM.yyyy (HH:mm:ss)");
        }

        public static DateTime ConvertUTCToLocalTime(string utcString)
        {
            DateTime utcTime = DateTime.Parse(utcString);
            DateTime time = DateTime.SpecifyKind(
                utcTime, DateTimeKind.Utc);
            return time.ToLocalTime();
        }

        public static void ResizeImageFileAsPng(Stream input, Stream output, int width, int height)
        {
            using (var image = Image.FromStream(input))
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(0, 0, width, height));
                bitmap.Save(output, ImageFormat.Png);
            }
        }
        #endregion
    }
}
