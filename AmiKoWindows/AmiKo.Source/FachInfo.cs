﻿/*
Copyright (c) 2016 Max Lungarella <cybrmx@gmail.com>

This file is part of AmiKoWindows.

AmiKoDesk is free software: you can redistribute it and/or modify
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

using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace AmiKoWindows
{
    class FachInfo : INotifyPropertyChanged
    {
        #region Private Fields
        string _appFolder;
        string _cssStr;
        #endregion

        #region Constructors
        public FachInfo()
        {
            _appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Load important files
            string cssFilePath = _appFolder + Constants.CSS_SHEET;
            if (File.Exists(cssFilePath))
            {
                _cssStr = "<style>" + File.ReadAllText(cssFilePath) + "</style>";
            }
        }
        #endregion

        #region Event Handlers
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Dependency Properties
        // Source object used for data binding, this is a property
        private string _htmlText;
        public string HtmlText
        {
            get { return _htmlText; }
            set
            {
                if (value != _htmlText)
                {
                    _htmlText = value;
                    OnPropertyChanged("HtmlText");
                }
            }
        }
        #endregion

        #region Public Functions
        public void ShowHtml(string htmlStr)
        {
            string headStr = "<head>" 
                + "<meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>" 
                // + "<link rel='stylesheet' type='text/css' href='http://fonts.googleapis.com/css?family=Roboto&subset=latin,latin-ext'>"
                + _cssStr
                + "</head>"; 
            HtmlText = headStr + htmlStr;
        }

        public void LoadHtmlFromFile(string fileName)
        {
            string filePath = _appFolder + @"\htmls\" + fileName;
            if (File.Exists(filePath))
            {
                string html = File.ReadAllText(filePath);
                HtmlText = html;
            }
        }
        #endregion
    }
}
