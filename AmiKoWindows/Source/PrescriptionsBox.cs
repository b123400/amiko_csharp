/*
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AmiKoWindows
{
    // Prescription Manager Object which knows active (opened) prescription.
    //
    // ```
    // # properties for active prescription
    // Contact Patient
    // Account Operotar
    // List<Medication> Medications
    // ```
    class PrescriptionsBox
    {
        const string AMIKO_FILE_PLACE_DATE_FORMAT = "dd.MM.yyyy (HH:mm:ss)";
        const string FILE_NAME_SUFFIX_DATE_FORMAT = "yyyy-MM-dd'T'HHmmss";

        private static readonly Regex AMIKO_FILE_EXTENSION_RGX = new Regex(@"\.amk\z", RegexOptions.Compiled);

        string _dataDir;

        #region Public Fields
        public string PlaceDate { get; set; }
        public string Hash { get; set; }

        public Contact Patient { get; set; }
        public Account Operator { get; set; }

        public bool IsActivePrescriptionPersisted // TODO
        {
            get { return ((PlaceDate != null && !PlaceDate.Equals(string.Empty)) && Medications.Count > 0); }
        }

        private HashSet<Medication> _Medications = new HashSet<Medication>();
        public List<Medication> Medications
        {
            get { return new List<Medication>(_Medications); }
        }

        #region Prescription File Manager
        private HashSet<TitleItem> _Files = new HashSet<TitleItem>();
        public List<TitleItem> Files
        {
            get { return new List<TitleItem>(_Files); }
        }
        #endregion
        #endregion

        #region Dependency Properties
        private ItemsObservableCollection _medicationListItems = new ItemsObservableCollection();
        public ItemsObservableCollection MedicationListItems
        {
            get { return _medicationListItems; }
            private set
            {
                if (value != _medicationListItems)
                    _medicationListItems = value;
            }
        }

        #region Prescription File Manager
        private TitlesObservableCollection _fileNames = new TitlesObservableCollection();
        public TitlesObservableCollection FileNames
        {
            get { return _fileNames; }
            private set
            {
                if (value != _fileNames)
                    _fileNames = value;
            }
        }
        #endregion
        #endregion

        public PrescriptionsBox()
        {
            _dataDir = Utilities.AppRoamingDataFolder();
            EnforceDir(_dataDir);
        }

        public void UpdateMedicationList()
        {
            MedicationListItems.Clear();
            MedicationListItems.AddRange(Medications);
        }

        public void UpdateFileNames()
        {
            FileNames.Clear();
            FileNames.AddRange(Files); // string[]
        }

        public void Renew()
        {
            _Medications.Clear();
            UpdateMedicationList();

            this.Hash = Utilities.GenerateUUID();
            this.PlaceDate = "";
        }

        public async Task Save()
        {
            this.PlaceDate = GeneratePlaceDate();
            if (Hash == null)
                this.Hash = Utilities.GenerateUUID(); // new

            await Task.Run(() =>
            {
                var outputFile = String.Format("RZ_{0}.amk", Utilities.GetLocalTimeAsString(FILE_NAME_SUFFIX_DATE_FORMAT));
                string outputPath = Path.Combine(_dataDir, Patient.Uid, outputFile);
                Log.WriteLine("outputPath: {0}", outputPath);
                try
                {
                    if (File.Exists(outputPath))
                        File.Delete(outputPath);

                    string json = SerializeCurrentData();
                    using (var output = File.Create(outputPath))
                    {
                        byte[] bytes = new UTF8Encoding(false).GetBytes(json);
                        output.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (IOException ex)
                {
                    Log.WriteLine(ex.Message);
                }
            });
        }

        public void AddMedication(Medication medication)
        {
            _Medications.Add(medication);
            UpdateMedicationList();
        }

        public void RemoveMedication(Medication medication)
        {
            _Medications.Remove(medication);
            UpdateMedicationList();
        }

        public void LoadFiles()
        {
            if (Patient == null)
                return;

            _Files.Clear();

            string userDir = Path.Combine(_dataDir, Patient.Uid);
            // Log.WriteLine("userDir: {0}", userDir);
            if (EnforceDir(userDir))
            {
                string[] files = Directory.GetFiles(userDir).OrderByDescending(f => f).ToArray();
                foreach (var path in files)
                {
                    //Log.WriteLine("filepath: {0}", path);
                    var filename = Path.GetFileName(path);
                    if (!AMIKO_FILE_EXTENSION_RGX.IsMatch(filename))
                        continue;
                    var item = new TitleItem() {
                        Id = filename, Title = AMIKO_FILE_EXTENSION_RGX.Replace(filename, "")
                    };
                    _Files.Add(item);
                }
            }
            UpdateFileNames();
        }

        public void DeleteFile(string hash)
        {

        }

        // Returns json as string
        private string SerializeCurrentData()
        {
            var serializer = new JavaScriptSerializer();
            var presenter = new PrescriptionJSONPresenter(Hash, PlaceDate);
            presenter.@operator = new AccountJSONPresenter(Operator);
            presenter.@patient = new ContactJSONPresenter(Patient);

            return serializer.Serialize(presenter);
        }

        private string GeneratePlaceDate()
        {
            if (Operator != null)
                return Utilities.ConcatWith(", ", Operator.City, Utilities.GetLocalTimeAsString(AMIKO_FILE_PLACE_DATE_FORMAT));
            return "";
        }

        private static bool EnforceDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Directory.Exists(dir);
        }
    }
}
