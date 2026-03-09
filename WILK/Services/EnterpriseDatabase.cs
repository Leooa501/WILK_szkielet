using WILK.Models;
using WILK.Services.Repositories;
using System.Data;

using MProductionStorageLib.DB;
using MProductionStorageLib.Model;

namespace WILK.Services
{
    /// <summary>
    /// Unified facade for all database operations, delegates to specialized repositories
    /// </summary>
    public interface IEnterpriseDatabase
    {
        // Component operations
        Task<DatabaseResult<string>> GetComponentNameByRIdAsync(int rId);
        Task<DatabaseResult<int>> GetComponentIdByRIdAsync(int rId);
        Task<DatabaseResult<string>> GetComponentTypeAsync(int componentId);
        Task<DatabaseResult<bool>> UpdateComponentsAsync(string [] IDs = null);

        // Reservation operations
        Task<DatabaseResult<DataTable>> LoadReservationsCompsAsync();
        Task<DatabaseResult<DataTable>> GetReservationsTableAsync();
        Task<DatabaseResult<(int done_top, int done_bot, int start)?>> GetReservationProgressAsync(int reservationId);
        Task<DatabaseResult<bool>> UpdateListReservationAsync(int reservationId, int newDone, IReservationRepository.Side side);
        Task<DatabaseResult<long>> AddListOfComponentsAsync(string name, int start, bool isSingleSided = false, bool isTHT = false, long existingListId = 0);
        Task<DatabaseResult<bool>> AddReservationComponentAsync(int r_id, int quantity);
        Task<DatabaseResult<bool>> DeleteReservationAsync(ReservationItemDto selected);
        Task<DatabaseResult<bool>> AddListOfComponentsFromExcelAsync(List<(string listName, long componentId, int componentQuantity, int listStart)> excelData);
        Task<DatabaseResult<bool>> ReverseLastUpdateAsync(int reservationId, IReservationRepository.Side side);
        Task<DatabaseResult<DataTable>> GetListDataAsync(int listId);
        Task<DatabaseResult<bool>> UpdateReservationListAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId, string side);
        Task<DatabaseResult<List<(long, string)>>> ClearReservationsAsync(long listId);
        Task<DatabaseResult<bool>> UpdateReservationTHTAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId);
        Task<DatabaseResult<DataTable>> GetRealReservationsAsync(long listId);
        Task<DatabaseResult<bool>> EditRealReservationAsync(int id, int quantity);
        Task<DatabaseResult<bool>> RemoveRealAlternativeAsync(int reservationId, int alternativeId);
        Task<DatabaseResult<bool>> AddRealAlternativeComponentAsync(int reservation_id, int originalRId, int substituteRId, int quantity);
        Task<DatabaseResult<DataTable>> GetDailyUsage(DateTime fromDate);
        Task<DatabaseResult<bool>> UpdateLogsAsync(int reservationId, int newDone, IReservationRepository.Side side);
        Task<DatabaseResult<DateTime>> GetLastReportDateAsync();
        Task<DatabaseResult<bool>> MoveRealReservationData(List<(long, string)> componentsToUpdate, long listID);
        Task<DatabaseResult<DataTable>> GetReservationsTHTAsync();
        Task<DatabaseResult<bool>> UpdateReservationTHTAsync(int reservationId, int newQuantity);
        Task<DatabaseResult<(int done, int start)?>> GetReservationTHTProgressAsync(int reservationId);
        Task<DatabaseResult<DataTable>> GetListDataTHTAsync(int listId);
        Task<DatabaseResult<DataTable>> GetCompletedReservationsTableTHTAsync();
        Task<DatabaseResult<DataTable>> GetTraceDataTHT(int list_id);
        Task<DatabaseResult<bool>> CloseReservationTHTAsync(int reservationId);
        Task<DatabaseResult<DataTable>> GetAdditionalMaterialsListAsync();
        Task<DatabaseResult<bool>> UpdateTraceAdditionalMaterialsAsync(int listId, (string reel_id, string box, int quantity) data);
        Task<DatabaseResult<DataTable>> GetTraceDataAdditionalMaterialsAsync(int list_id);
        Task<DatabaseResult<DataTable>> GetDataAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<bool>> UpdateReservationsAdditionalMaterialsAsync(int listId, (long componentId, int quantity) data);
        Task<DatabaseResult<bool>> CompleteAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<int>> AddListOfAdditionalMaterialsAsync(string name, int start);
        Task<DatabaseResult<bool>> AddAdditionalMaterialReservation(List<(int componentId, int quantity)> materials, int listId);
        Task<DatabaseResult<bool>> DeleteAdditionalMaterialsListAsync(int listId);
        Task<DatabaseResult<bool>> UpdateTransferredStatus(int listId, bool transferred);
        Task<DatabaseResult<(bool realizeFlag, string assignedPerson)>> GetMetadata(int listId);
        Task<DatabaseResult<(int done, int start)?>> GetReservationProgressTHTAsync(int reservationId);
        Task<DatabaseResult<DataTable>> LoadReservationTHTAsync(int listId);
        Task<DatabaseResult<DataTable>> GetDraft(int listId);
        Task<DatabaseResult<bool>> UpdateTraceTHT(List<(string reelId, string box, int used)> data, int listId);
        Task<DatabaseResult<bool>> UpdateListReservationTHTAsync(int reservationId, int newDone);
        Task<DatabaseResult<bool>> SaveDraft(int listId, DataTable draftTable);
        Task<DatabaseResult<bool>> UpdateReservationMetadataAsync(int reservationId, bool? realizeFlag = null, DateTime? maxEndDate = null, string? assignedPerson = null, string? destination = null, string? package = null);
        Task<DatabaseResult<bool>> ReverseLastUpdateTHTAsync(int reservationId);

        // Alternative operations
        Task<DatabaseResult<List<ComponentDto>>> GetAlternativeComponentsAsync(int originalComponentId);
        Task<DatabaseResult<DataTable>> GetAlternativeComponentsTableAsync();
        Task<DatabaseResult<bool>> AddAlternativeComponentAsync(int originalRId, int substituteRId);
        Task<DatabaseResult<bool>> DeleteAlternativeComponentAsync(int alternativeId);
        Task<DatabaseResult<bool>> AddListOfAlternativesAsync(List<(string Kol1, string Kol2)> altList);

        // WarmUp operations
        Task<DatabaseResult<DataTable>> GetWarmUpComponentsTableAsync();
        Task<DatabaseResult<bool>> AddWarmUpComponentAsync(int rId);
        Task<DatabaseResult<bool>> DeleteWarmUpComponentAsync(int warmUpId);
        Task<DatabaseResult<bool>> IsInWarmUpAsync(int rId);

        // Location operations
        Task<DatabaseResult<List<Element>>> GetLocationsAsync(string []componentId);
        Task<DatabaseResult<List<Element>>> GetLocationsTHTAsync(string []componentId);

        // Shortage operations
        Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiAsync(bool onlySMD = true);
        Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiForListAsync(List<ExcelRow> listOfComponents);
        Task<DatabaseResult<bool>> ExportBrakiToExcelAsync(List<(int componentId, string componentName, int brakQuantity, int rId)> braki, string filePath);
        Task<DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>> GetComponentAmountAsync(List<ExcelRow> listOfComponents);

        // Import/Export operations
        Task<DatabaseResult<bool>> SetLastStanMagazynowyImportDateAsync(string fileName);
        Task<DatabaseResult<(string FileName, DateTime Date)?>> GetLastStanMagazynowyImportDateAsync();
        Task<DatabaseResult<bool>> AddPnPDataAsync(List<(int r_id, int quantity)> data, long listId, string fileName, string type);
        Task<DatabaseResult<string>> GetSideFileNameAsync(long listId, string type);
        Task<DatabaseResult<string[]>> GetComponentSideAsync(int rId, long listId);
        Task<DatabaseResult<(string type, int qty)[]>> GetComponentSideAndQtyAsync(int rId, long listId);

        // Excessive Usage operations
        Task<DatabaseResult<bool>> AddExcessiveUsageAsync(int productId, int quantity, string reason, string reelId);
        Task<DatabaseResult<System.Data.DataTable>> GetExcessiveUsage(DateTime since);
        Task<DatabaseResult<bool>> DeleteExcessiveUsageAsync(int id);
        
        // Errors operations
        Task<DatabaseResult<bool>> AddErrorsAsync(string type, string cause, string reelId, int correctAmount, int correctOrder, string correctBox, string description, string author);
        Task<DatabaseResult<System.Data.DataTable>> GetErrors(DateTime since);
    


        //Components operations
        Task<DatabaseResult<DataTable>> SearchComponentsAsync(string? id, string? namePrefix);
        Task<DatabaseResult<bool>> AddComponentAsync(int id, string name, string type);
        Task<DatabaseResult<bool>> UpdateComponentAsync(int id, string name, string type);
    }

    public class EnterpriseDatabase : IEnterpriseDatabase
    {
        private readonly IComponentRepository _componentRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IAlternativeRepository _alternativeRepository;
        private readonly IWarmUpRepository _warmUpRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IShortageRepository _shortageRepository;
        private readonly IImportExportRepository _importExportRepository;
        private readonly IExcessiveUsageRepository _excessiveUsageRepository;
        private readonly IErrorsRepository _errorsRepository;

        public EnterpriseDatabase(
            IComponentRepository componentRepository,
            IReservationRepository reservationRepository,
            IAlternativeRepository alternativeRepository,
            IWarmUpRepository warmUpRepository,
            ILocationRepository locationRepository,
            IShortageRepository shortageRepository,
            IImportExportRepository importExportRepository,
            IExcessiveUsageRepository excessiveUsageRepository,
            IErrorsRepository errorsRepository
            )
        {
            _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
            _alternativeRepository = alternativeRepository ?? throw new ArgumentNullException(nameof(alternativeRepository));
            _warmUpRepository = warmUpRepository ?? throw new ArgumentNullException(nameof(warmUpRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _shortageRepository = shortageRepository ?? throw new ArgumentNullException(nameof(shortageRepository));
            _importExportRepository = importExportRepository ?? throw new ArgumentNullException(nameof(importExportRepository));
            _excessiveUsageRepository = excessiveUsageRepository ?? throw new ArgumentNullException(nameof(excessiveUsageRepository));
            _errorsRepository = errorsRepository ?? throw new ArgumentNullException(nameof(errorsRepository));

        }

        #region Component Operations

        public Task<DatabaseResult<bool>> UpdateComponentsAsync(string [] IDs = null)
            => _componentRepository.UpdateComponentsAsync(IDs);

        public Task<DatabaseResult<string>> GetComponentNameByRIdAsync(int rId)
            => _componentRepository.GetComponentNameByRIdAsync(rId);

        public Task<DatabaseResult<int>> GetComponentIdByRIdAsync(int rId)
            => _componentRepository.GetComponentIdByRIdAsync(rId);

        public Task<DatabaseResult<string>> GetComponentTypeAsync(int componentId)
            => _componentRepository.GetComponentTypeAsync(componentId);

        #endregion

        #region Reservation Operations

        public Task<DatabaseResult<DataTable>> LoadReservationsCompsAsync()
            => _reservationRepository.LoadReservationsCompsAsync();

        public Task<DatabaseResult<DataTable>> GetReservationsTableAsync()
            => _reservationRepository.GetReservationsTableAsync();

        public Task<DatabaseResult<(int done_top, int done_bot, int start)?>> GetReservationProgressAsync(int reservationId)
            => _reservationRepository.GetReservationProgressAsync(reservationId);

        public Task<DatabaseResult<bool>> UpdateListReservationAsync(int reservationId, int newDone, IReservationRepository.Side side)
            => _reservationRepository.UpdateListReservationAsync(reservationId, newDone, side);

        public Task<DatabaseResult<long>> AddListOfComponentsAsync(string name, int start, bool isSingleSided = false, bool isTHT = false, long existingListId = 0)
            => _reservationRepository.AddListOfComponentsAsync(name, start, isSingleSided, isTHT, existingListId);

        public Task<DatabaseResult<bool>> AddReservationComponentAsync(int r_id, int quantity)
            => _reservationRepository.AddReservationComponentAsync(r_id, quantity);

        public Task<DatabaseResult<bool>> DeleteReservationAsync(ReservationItemDto selected)
            => _reservationRepository.DeleteReservationAsync(selected);

        public Task<DatabaseResult<bool>> AddListOfComponentsFromExcelAsync(List<(string listName, long componentId, int componentQuantity, int listStart)> excelData)
            => _reservationRepository.AddListOfComponentsFromExcelAsync(excelData);

        public Task<DatabaseResult<bool>> ReverseLastUpdateAsync(int reservationId, IReservationRepository.Side side)
            => _reservationRepository.ReverseLastUpdateAsync(reservationId, side);

        public Task<DatabaseResult<DataTable>> GetListDataAsync(int listId)
            => _reservationRepository.GetListDataAsync(listId);

        public Task<DatabaseResult<bool>> UpdateReservationListAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId, string side)
            => _reservationRepository.UpdateReservationListAsyncList(dane, listId, side);

        public Task<DatabaseResult<List<(long, string)>>> ClearReservationsAsync(long listId)
            => _reservationRepository.ClearReservationsAsync(listId);

        public Task<DatabaseResult<bool>> UpdateReservationTHTAsyncList(List<(string kolName, string kolId, string kolQuantity)> dane, long listId)
            => _reservationRepository.UpdateReservationTHTAsyncList(dane, listId);

        public Task<DatabaseResult<DataTable>> GetRealReservationsAsync(long listId)
            => _reservationRepository.GetRealReservationAsync(listId);
        public Task<DatabaseResult<bool>> EditRealReservationAsync(int id, int quantity)
            => _reservationRepository.EditRealReservationAsync(id, quantity);

        public Task<DatabaseResult<bool>> RemoveRealAlternativeAsync(int reservationId, int alternativeId)
            => _reservationRepository.RemoveRealAlternativeAsync(reservationId, alternativeId);

        public Task<DatabaseResult<bool>> AddRealAlternativeComponentAsync(int reservation_id, int originalRId, int substituteRId, int quantity)
            => _reservationRepository.AddRealAlternativeComponentAsync(reservation_id, originalRId, substituteRId, quantity);

        public Task<DatabaseResult<DataTable>> GetDailyUsage(DateTime fromDate)
            => _reservationRepository.GetDailyUsage(fromDate);

        public Task<DatabaseResult<bool>> UpdateLogsAsync(int reservationId, int newDone, IReservationRepository.Side side)
            => _reservationRepository.UpdateLogsAsync(reservationId, newDone, side);

        public Task<DatabaseResult<DateTime>> GetLastReportDateAsync()
            => _reservationRepository.GetLastReportDateAsync();

        public Task<DatabaseResult<bool>> MoveRealReservationData(List<(long, string)> componentsToUpdate, long listID)
            => _reservationRepository.MoveRealReservationData(componentsToUpdate, listID);
        public Task<DatabaseResult<DataTable>> GetReservationsTHTAsync()
            => _reservationRepository.GetReservationsTHTAsync();

        public Task<DatabaseResult<bool>> UpdateReservationTHTAsync(int reservationId, int newQuantity)
            => _reservationRepository.UpdateReservationTHTAsync(reservationId, newQuantity);

        public Task<DatabaseResult<(int done, int start)?>> GetReservationTHTProgressAsync(int reservationId)
            => _reservationRepository.GetReservationTHTProgressAsync(reservationId);

        public Task<DatabaseResult<DataTable>> GetListDataTHTAsync(int listId)
            => _reservationRepository.GetListDataTHTAsync(listId);

        public Task<DatabaseResult<DataTable>> GetCompletedReservationsTableTHTAsync()
            => _reservationRepository.GetCompletedReservationsTableTHTAsync();

        public Task<DatabaseResult<DataTable>> GetTraceDataTHT(int list_id)
            => _reservationRepository.GetTraceDataTHT(list_id);
        
        public Task<DatabaseResult<bool>> CloseReservationTHTAsync(int reservationId)
            => _reservationRepository.CloseReservationTHTAsync(reservationId);

        public Task<DatabaseResult<DataTable>> GetAdditionalMaterialsListAsync()
            => _reservationRepository.GetAdditionalMaterialsListAsync();
        
        public Task<DatabaseResult<bool>> UpdateTraceAdditionalMaterialsAsync(int listId, (string reel_id, string box, int quantity) data)
            => _reservationRepository.UpdateTraceAdditionalMaterialsAsync(listId, data);

        public Task<DatabaseResult<DataTable>> GetTraceDataAdditionalMaterialsAsync(int list_id)
            => _reservationRepository.GetTraceDataAdditionalMaterialsAsync(list_id);

        public Task<DatabaseResult<DataTable>> GetDataAdditionalMaterialsListAsync(int listId)
            => _reservationRepository.GetDataAdditionalMaterialsListAsync(listId);

        public Task<DatabaseResult<bool>> UpdateReservationsAdditionalMaterialsAsync(int listId, (long componentId, int quantity) data)
            => _reservationRepository.UpdateReservationsAdditionalMaterialsAsync(listId, data);
        public Task<DatabaseResult<bool>> CompleteAdditionalMaterialsListAsync(int listId)
            => _reservationRepository.CompleteAdditionalMaterialsListAsync(listId);
        
        public Task<DatabaseResult<bool>> UpdateTransferredStatus(int listId, bool transferred)
            => _reservationRepository.UpdateTransferredStatus(listId, transferred);

        public Task<DatabaseResult<int>> AddListOfAdditionalMaterialsAsync(string name, int start)
            => _reservationRepository.AddListOfAdditionalMaterialsAsync(name, start);
        
        public Task<DatabaseResult<bool>> AddAdditionalMaterialReservation(List<(int componentId, int quantity)> materials, int listId)
            => _reservationRepository.AddAdditionalMaterialReservation(materials, listId);

        public Task<DatabaseResult<bool>> DeleteAdditionalMaterialsListAsync(int listId)
            => _reservationRepository.DeleteAdditionalMaterialsListAsync(listId);
        
        public Task<DatabaseResult<(bool realizeFlag, string assignedPerson)>> GetMetadata(int listId)
            => _reservationRepository.GetMetadata(listId);

        public Task<DatabaseResult<(int done, int start)?>> GetReservationProgressTHTAsync(int reservationId)
            => _reservationRepository.GetReservationProgressTHTAsync(reservationId);

        public Task<DatabaseResult<DataTable>> LoadReservationTHTAsync(int listId)
            => _reservationRepository.LoadReservationTHTAsync(listId);

        public Task<DatabaseResult<DataTable>> GetDraft(int listId)
            => _reservationRepository.GetDraft(listId);

        public Task<DatabaseResult<bool>> UpdateTraceTHT(List<(string reelId, string box, int used)> data, int listId)
            => _reservationRepository.UpdateTraceTHT(data, listId);

        public Task<DatabaseResult<bool>> UpdateListReservationTHTAsync(int reservationId, int newDone)
            => _reservationRepository.UpdateListReservationTHTAsync(reservationId, newDone);

        public Task<DatabaseResult<bool>> SaveDraft(int listId, DataTable draftTable)
            => _reservationRepository.SaveDraft(listId, draftTable);

        public Task<DatabaseResult<bool>> UpdateReservationMetadataAsync(int reservationId, bool? realizeFlag = null, DateTime? maxEndDate = null, string? assignedPerson = null, string? destination = null, string? package = null)
            => _reservationRepository.UpdateReservationMetadataAsync(reservationId, realizeFlag, maxEndDate, assignedPerson, destination, package);

        public Task<DatabaseResult<bool>> ReverseLastUpdateTHTAsync(int reservationId)  
            => _reservationRepository.ReverseLastUpdateTHTAsync(reservationId);

        #endregion

        #region Alternative Operations

        public Task<DatabaseResult<List<ComponentDto>>> GetAlternativeComponentsAsync(int originalComponentId)
            => _alternativeRepository.GetAlternativeComponentsAsync(originalComponentId);

        public Task<DatabaseResult<DataTable>> GetAlternativeComponentsTableAsync()
            => _alternativeRepository.GetAlternativeComponentsTableAsync();

        public Task<DatabaseResult<bool>> AddAlternativeComponentAsync(int originalRId, int substituteRId)
            => _alternativeRepository.AddAlternativeComponentAsync(originalRId, substituteRId);

        public Task<DatabaseResult<bool>> DeleteAlternativeComponentAsync(int alternativeId)
            => _alternativeRepository.DeleteAlternativeComponentAsync(alternativeId);

        public Task<DatabaseResult<bool>> AddListOfAlternativesAsync(List<(string Kol1, string Kol2)> altList)
            => _alternativeRepository.AddListOfAlternativesAsync(altList);
    

        #endregion

        #region WarmUp Operations

        public Task<DatabaseResult<DataTable>> GetWarmUpComponentsTableAsync()
            => _warmUpRepository.GetWarmUpComponentsTableAsync();

        public Task<DatabaseResult<bool>> AddWarmUpComponentAsync(int rId)
            => _warmUpRepository.AddWarmUpComponentAsync(rId);

        public Task<DatabaseResult<bool>> DeleteWarmUpComponentAsync(int warmUpId)
            => _warmUpRepository.DeleteWarmUpComponentAsync(warmUpId);

        public Task<DatabaseResult<bool>> IsInWarmUpAsync(int rId)
            => _warmUpRepository.IsInWarmUpAsync(rId);

        #endregion

        #region Location Operations

        public Task<DatabaseResult<List<Element>>> GetLocationsAsync(string []componentId)
            => _locationRepository.GetLocationsAsync(componentId);

            
        public Task<DatabaseResult<List<Element>>> GetLocationsTHTAsync(string []componentId)
            => _locationRepository.GetLocationsTHTAsync(componentId);

        #endregion

        #region Shortage Operations

        public Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiAsync(bool onlySMD = true)
            => _shortageRepository.GetBrakiAsync(onlySMD);

        public Task<DatabaseResult<List<(int componentId, string componentName, int brakQuantity, int rId)>>> GetBrakiForListAsync(List<ExcelRow> listOfComponents)
            => _shortageRepository.GetBrakiForListAsync(listOfComponents);

        public Task<DatabaseResult<bool>> ExportBrakiToExcelAsync(List<(int componentId, string componentName, int brakQuantity, int rId)> braki, string filePath)
            => _shortageRepository.ExportBrakiToExcelAsync(braki, filePath);

        public Task<DatabaseResult<List<(int componentId, string componentName, int Quantity, int rId)>>> GetComponentAmountAsync(List<ExcelRow> listOfComponents)
            => _shortageRepository.GetComponentAmountAsync(listOfComponents);

        #endregion

        #region Import/Export Operations

        public Task<DatabaseResult<bool>> SetLastStanMagazynowyImportDateAsync(string fileName)
            => _importExportRepository.SetLastStanMagazynowyImportDateAsync(fileName);

        public Task<DatabaseResult<(string FileName, DateTime Date)?>> GetLastStanMagazynowyImportDateAsync()
            => _importExportRepository.GetLastStanMagazynowyImportDateAsync();

        public Task<DatabaseResult<bool>> AddPnPDataAsync(List<(int r_id, int quantity)> data, long listId, string fileName, string type)
            => _importExportRepository.AddPnPDataAsync(data, listId, fileName, type);

        public Task<DatabaseResult<string>> GetSideFileNameAsync(long listId, string type)
            => _importExportRepository.GetSideFileNameAsync(listId, type);

        public Task<DatabaseResult<string[]>> GetComponentSideAsync(int rId, long listId)
            => _importExportRepository.GetComponentSideAsync(rId, listId);

        public Task<DatabaseResult<(string type, int qty)[]>> GetComponentSideAndQtyAsync(int rId, long listId)
            => _importExportRepository.GetComponentSideAndQtyAsync(rId, listId);

        #endregion

        #region Excessive Usage Operations
        public Task<DatabaseResult<bool>> AddExcessiveUsageAsync(int productId, int quantity, string reason, string reelId)
            => _excessiveUsageRepository.AddExcessiveUsageAsync(productId, quantity, reason, reelId);
        public Task<DatabaseResult<System.Data.DataTable>> GetExcessiveUsage(DateTime since)
            => _excessiveUsageRepository.GetExcessiveUsageAsync(since);

        public Task<DatabaseResult<bool>> DeleteExcessiveUsageAsync(int id)
            => _excessiveUsageRepository.DeleteExcessiveUsageAsync(id);

        #endregion

        #region Errors Operations
        public Task<DatabaseResult<bool>> AddErrorsAsync(string type, string cause, string reelId, int correctAmount, int correctOrder, string correctBox, string description, string author)
            => _errorsRepository.AddErrorsAsync(type, cause, reelId, correctAmount, correctOrder, correctBox, description, author);
        public Task<DatabaseResult<System.Data.DataTable>> GetErrors(DateTime since)
            => _errorsRepository.GetErrorsAsync(since);

        #endregion

        #region Components Operations
        public Task<DatabaseResult<DataTable>> SearchComponentsAsync(string? id, string? namePrefix)
            => _componentRepository.SearchComponentsAsync(id, namePrefix);

        public Task<DatabaseResult<bool>> AddComponentAsync(int id, string name, string type)
            => _componentRepository.AddComponentAsync(id, name, type);

        public Task<DatabaseResult<bool>> UpdateComponentAsync(int id, string name, string type)
            => _componentRepository.UpdateComponentAsync(id, name, type);
        #endregion
    }
}