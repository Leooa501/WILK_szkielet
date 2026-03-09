using WILK.Models;
using WILK.Services.Repositories;

namespace WILK.Views
{
    public interface IMainView
    {
        event EventHandler Load;

        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
    }


    public class CsvImportEventArgs : EventArgs
    {
        public string FileName { get; }
        public CsvImportEventArgs(string fileName) { FileName = fileName; }
    }
    
    public class ExcelListSavedEventArgs : EventArgs
    {
        public int Start { get; }
        public ExcelListSavedEventArgs(int start) { Start = start; }
    }

    public class AddReservationEventArgs : EventArgs
    {
        public int RId { get; }
        public int Quantity { get; }
        public AddReservationEventArgs(int rId, int qty) { RId = rId; Quantity = qty; }
    }

    public class ExcelListEventArgs : EventArgs
    {
        public string FileName { get; }
        public ExcelListEventArgs(string fileName) { FileName = fileName; }
    }

    public class ExportBrakiEventArgs : EventArgs
    {
        public string FilePath { get; }
        public bool onlySMD { get; }
        public ExportBrakiEventArgs(string filePath, bool onlySMD) { FilePath = filePath; this.onlySMD = onlySMD; }
    }

    public class DeleteReservationEventArgs : EventArgs
    {
        public int Id { get; }
        public bool IsList { get; }
        public DeleteReservationEventArgs(int id, bool isList) { Id = id; IsList = isList; }
    }

    public class UpdateListDoneEventArgs : EventArgs
    {
        public int ReservationId { get; }
        public int AddValue { get; }
        public IReservationRepository.Side Side { get; set; }
        public UpdateListDoneEventArgs(int reservationId, int addValue, IReservationRepository.Side side) { ReservationId = reservationId; AddValue = addValue; Side = side; }
    }

    public class ExcelAltsListEventArgs : EventArgs
    {
        public string FileName { get; }
        public ExcelAltsListEventArgs(string fileName) { FileName = fileName; }
    }
    
    public class ReverseLastUpdateEventArgs : EventArgs
    {
        public int ReservationId { get; }
        public IReservationRepository.Side Side { get; set; }
        public ReverseLastUpdateEventArgs(int reservationId, IReservationRepository.Side side) { ReservationId = reservationId; Side = side; }
    }

    public class AlternativeDeleteEventArgs : EventArgs
    {
        public int AlternativeId { get; }
        public AlternativeDeleteEventArgs(int alternativeId) { AlternativeId = alternativeId; }
    }

    public class WarmUpDeleteEventArgs : EventArgs
    {
        public int WarmUpId { get; }
        public WarmUpDeleteEventArgs(int warmUpId) { WarmUpId = warmUpId; }
    }

    public class LoadContainersEventArgs : EventArgs
    {
        public string FilePath { get; }
        public LoadContainersEventArgs(string filePath) { FilePath = filePath; }
    }

    public class GenerateListEventArgs : EventArgs
    {
        public int id { get; }
        public string filePath { get; }
        public bool oneSided { get; set; }
        public int? quantity { get; set; }
        public GenerateListEventArgs(string filePath, int id, bool oneSided, int? quantity = null) { this.filePath = filePath; this.id = id; this.oneSided = oneSided; this.quantity = quantity; }
    }

    public class GenerateListTHTEventArgs : EventArgs
    {
        public int id { get; }
        public string filePath { get; }
        public string? quantity { get; }
        public GenerateListTHTEventArgs(string filePath, int id, string? quantity = null) { this.filePath = filePath; this.id = id; this.quantity = quantity; }
    }

    public class UpdateListDoneTHTEventArgs : EventArgs
    {
        public int ReservationId { get; }
        public int AddValue { get; }
        public UpdateListDoneTHTEventArgs(int reservationId, int addValue) { ReservationId = reservationId; AddValue = addValue; }
    }

    public class ExcessiveUsageEventArgs : EventArgs
    {
        public int RId { get; }
        public int Quantity { get; }
        public string Reason { get; }
        public string? ReelId { get; }

        public ExcessiveUsageEventArgs(int rId, int quantity, string reason, string? reelId = null)
        {
            RId = rId;
            Quantity = quantity;
            Reason = reason;
            ReelId = reelId;
        }
    }

    public class ExcessiveDeleteEventArgs : EventArgs
    {
        public int Id { get; }

        public ExcessiveDeleteEventArgs(int id)
        {
            Id = id;
        }
    }

    public class ListsSelectedEventArgs : EventArgs
    {
        public int listId { get; }
        public string? listName { get; }

        public ListsSelectedEventArgs(int listId, string? listName = null)
        {
            this.listId = listId;
            this.listName = listName;
        }
    }

    public class AddAlternativeEventArgs : EventArgs
    {
        public int reservationId { get; }
        public int originalRId { get; }
        public int substituteRId { get; }
        public int quantity { get; }

        public AddAlternativeEventArgs(int reservationId, int originalRId, int substituteRId, int quantity)
        {
            this.reservationId = reservationId;
            this.originalRId = originalRId;
            this.substituteRId = substituteRId;
            this.quantity = quantity;
        }
    }

    public class EditAlternativeEventArgs : EventArgs
    {
        public int reservationId { get; }
        public int newQuantity { get; }

        public EditAlternativeEventArgs(int reservationId, int newQuantity)
        {
            this.reservationId = reservationId;
            this.newQuantity = newQuantity;
        }
    }

    public class RemoveAlternativeEventArgs : EventArgs
    {
        public int alternativeId { get; }
        public int reservationId { get; }

        public RemoveAlternativeEventArgs(int alternativeId, int reservationId)
        {
            this.alternativeId = alternativeId;
            this.reservationId = reservationId;
        }
    }

    public class DailyUsageEventArgs : EventArgs
    {
        public DateTime SelectedDate { get; }
        public DailyUsageEventArgs(DateTime selectedDate)
        {
            SelectedDate = selectedDate;
        }
    }

    public class ErrorsEventArgs : EventArgs
    {
        public string Type { get; }
        public string Cause { get; }
        public string ReelId { get; }
        public int CorrectAmount { get; }
        public int CorrectOrder { get; }
        public string CorrectBox { get; }
        public string Description { get; }
        public string Author { get; }

        public ErrorsEventArgs(string type, string cause, string reelId, int correctAmount, int correctOrder, string correctBox, string description, string author)
        {
            Type = type;
            Cause = cause;
            ReelId = reelId;
            CorrectAmount = correctAmount;
            CorrectOrder = correctOrder;
            CorrectBox = correctBox;
            Description = description;
            Author = author;
        }
    }

    public class UpdateTransferredStatusEventArgs : EventArgs
    {
        public int ListId { get; }
        public bool Transferred { get; }

        public UpdateTransferredStatusEventArgs(int listId, bool transferred)
        {
            ListId = listId;
            Transferred = transferred;
        }
    }

    public class GenerateListPart : EventArgs
    {
        public int Quantity { get; }
        public GenerateListPart(int quantity)        
        {
            Quantity = quantity;       
        }
    }
    public class ReverseLastUpdateTHTEventArgs : EventArgs
    {
        public int ReservationId { get; }
        public ReverseLastUpdateTHTEventArgs(int reservationId) { ReservationId = reservationId; }
    }

    public class MultipleFilesEventArgs : EventArgs
    {
        public IReadOnlyList<FileEntry> Files { get; }
        public MultipleFilesEventArgs(IReadOnlyList<FileEntry> files) { Files = files; }
    }
}