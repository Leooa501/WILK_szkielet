using DocumentFormat.OpenXml.Drawing;
using WILK.Services;

namespace WILK.Views
{
    public partial class CreateAdditionalMaterialView : CreateReservationList
    {
        public event EventHandler<EventArgs>? CreateAdditionalMaterialRequested;

        public CreateAdditionalMaterialView(IEnterpriseDatabase enterpriseDatabase, IFileProcessingService fileProcessingService)
            : base(enterpriseDatabase, fileProcessingService)
        {
            base.Text = "Dodaj materiał dodatkowy";
            HideRadio();
        }

        protected override async void OnButtonSaveListClick(object? sender, EventArgs e)
        {
            if (base._excelData.Count == 0)
            {
                MessageBox.Show("Brak danych do zapisania. Proszę załadować plik z materiałami dodatkowymi.", "Błąd");
                return;
            }

            await base._enterpriseDatabase.AddListOfAdditionalMaterialsAsync(base._excelName, GetQuantity()).ContinueWith(async task =>
            {
                var result = task.Result;
                if (!result.IsSuccess)
                {
                     MessageBox.Show($"Lista materiałów dodatkowych została utworzona z ID: {result.Data}", "Sukces");
                    this.Close();
                }

                List<(int componentId, int quantity)> materials = new();
                foreach(var row in base._excelData)
                {
                    if(!int.TryParse(row.Kol2, out var r_id) || !int.TryParse(row.Kol3, out var qty))
                    {
                        MessageBox.Show($"Niepoprawne dane w liście: {row.Kol1}, {row.Kol2}, {row.Kol3}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var compIdResult = await base._enterpriseDatabase.GetComponentIdByRIdAsync(r_id);
                    if(!compIdResult.IsSuccess || compIdResult.Data == -1)
                    {
                        MessageBox.Show($"Brak elementu w bazie danych id: {row.Kol2}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    materials.Add((compIdResult.Data, qty));
                }

                var addResult = await base._enterpriseDatabase.AddAdditionalMaterialReservation(materials, result.Data);
                if (!addResult.IsSuccess)
                {
                    MessageBox.Show($"Błąd podczas dodawania rezerwacji materiałów dodatkowych: {addResult.ErrorMessage}", "Błąd zapisu listy Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }else
                {
                    MessageBox.Show($"Lista materiałów dodatkowych została utworzona z ID: {result.Data}", "Sukces");
                    this.Close();
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

    }
}