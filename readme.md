# DataGridView + Firebase

In your code you are adding a `degerlerClass` item to `values` using this code:

	data = {"sarj": str(10), "gyro": str(5) }
	db.child("values").set(data)

Then it appears that you are making a make a call to retrieve the items contained in `values` using:

	FirebaseResponse response = client.Get(@"values");

What I'm would expect from reading your code is that the response body will contain a `List` of `degerlerClass` records not a `Dictionary` of them and that `response.Body.ToString()` would evaluate to this:

     "[
          {
            "gyroData": "10",
            "sarjData"": "5"
          }
      ]"

*Your post doesn't show the string that we're trying to deserialize, but I'm confident that this answer will require only small adjustments even if the response body is different then what I'm simulating in my mock data. But please you do need to show what `string responseBody = response.Body.ToString()` evaluates to in ordet to be 100% certain.*

The intent of the `DataGridView` appears to be to display a list of `degerlerClass` records and that is should look like this after the response is received:

![screenshot]()


This suggests that what you want is a `DataSource` bound to the `DataGridView` that is set once in the beginning to `BindingList<degerlerClass>`. 

***
**Setting up the `DataGridView` in `MainForm` to display a list of `degerlerClass`**

    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        IFirebaseClient client = new MockClient();
        private readonly BindingList<degerlerClass> DataSource = new BindingList<degerlerClass>();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.DataSource = DataSource;
            // Generate and format columns
            DataSource.Add(new degerlerClass());
            foreach(DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            DataSource.Clear();
        }
        private void dataView_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = client.Get(@"values");
            var responseBody = response.Body.ToString();
            var data = JsonConvert.DeserializeObject<List<degerlerClass>>(responseBody);
            PopulateDataView(data);
        }
        private void PopulateDataView(List<degerlerClass> data)
        {
            DataSource.Clear();
            foreach (var record in data)
            {
                DataSource.Add(record);
            }
        }
    }

***
**Mock Firebase Response to test the `JsonConvert`**

    #region M O C K
    interface IFirebaseClient
    {
        FirebaseResponse Get(string arg);
    }
    class MockClient : IFirebaseClient
    {
        public FirebaseResponse Get(string v) => new FirebaseResponse();
    }

    internal class FirebaseResponse
    {
        public object Body =>
 @"[
      {
        ""gyroData"": ""10"",
        ""sarjData"": ""5""
      }
  ]";
    }
    #endregion M O C K
