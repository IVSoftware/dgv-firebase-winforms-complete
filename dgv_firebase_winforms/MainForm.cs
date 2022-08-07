using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dgv_firebase_winforms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // Credentials are maintained outside the public repo.
            var fileInfo = new FileInfo(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                @"..\..\..\..\..\credentials\credential.json"));

            var GOOGLE_APPLICATION_CREDENTIALS = fileInfo.FullName;
            if(fileInfo.Exists)
            {
                Environment.SetEnvironmentVariable(
                    "GOOGLE_APPLICATION_CREDENTIALS",
                    GOOGLE_APPLICATION_CREDENTIALS
                );
            }
            else
            {
                var message = $"Please place a valid credentials file here: {GOOGLE_APPLICATION_CREDENTIALS}";
                Debug.Assert(false, message);
            }
            try
            {
                var credential = 
                    JsonConvert
                    .DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"))
                    );
                var projectID = credential["project_id"];
                _db = FirestoreDb.Create(projectId: credential["project_id"]);
            }
            catch( Exception ex)
            {
                MessageBox.Show($"Database creation failed with error: {ex.Message}");
                return;
            }
        }
        private readonly FirestoreDb _db;
        private readonly BindingList<ValuesClass> DataSource = new BindingList<ValuesClass>();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView1.DataSource = DataSource;
            // Generate and format columns
            DataSource.Add(new ValuesClass());
            foreach(DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            DataSource.Clear();
            // When User adds a row, it will be added to the database.
            dataGridView1.CellEndEdit += addOrSetDocument;
            dataGridView1.UserDeletingRow += onUserDeletingRow;
        }

        private async void addOrSetDocument(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < DataSource.Count)
            {
                var doc = DataSource[e.RowIndex];
                if (doc.Id == null)
                {
                    var result = await _db.Collection("values").AddAsync(doc);
                    doc.Id = result.Id;
                }
                else
                {
                    await _db.Collection("values").Document(doc.Id).SetAsync(doc);
                }
            }
        }
        private async void onUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Index < DataSource.Count)
            {
                var doc = DataSource[e.Row.Index];
                if (doc.Id != null)
                {
                    await _db.Collection("values").Document(doc.Id).DeleteAsync();
                }
            }
        }
        // https://firebase.google.com/docs/database/admin/save-data
        // https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/userguide

        private async void dataView_Click(object sender, EventArgs e)
        {
            try{
                var snapshot = await
                    _db.Collection(@"values")
                    .GetSnapshotAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
                var recordset = snapshot.Select(_ => _.ConvertTo<ValuesClass>());
                DataSource.Clear();
                foreach (var record in recordset)
                {
                    DataSource.Add(record);
                }
            }
            catch (Exception ex){
                Debug.Assert(false, ex.Message);
            }
        }
    }

    // https://stackoverflow.com/a/68223746/5438626
    // NOTE: Original post seems to use `dergerlerClass` and `valuesClass` interchangeably.
    [FirestoreData]
    public class ValuesClass
    {
        [FirestoreDocumentId, Browsable(false)]
        public string Id { get; set; }

        [FirestoreProperty]
        public string gyra { get; set; }

        [FirestoreProperty]
        public string sarj { get; set; }
    }
}
