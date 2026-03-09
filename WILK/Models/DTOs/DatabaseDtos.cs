namespace WILK.Models.DTOs
{
    // Simplified DTOs for repository layer operations
    
    public record ComponentDto(int Id, string Name, int RId, string Type, int Quantity);
    
    public record ReservationDto(int Id, string ComponentName, int Quantity, bool IsList);
    
    public record ReservationListDto(int Id, string Name, string Progress, int LastDone, DateTime CreatedAt);
    
    public record AlternativeDto(int Id, int OriginalRId, string OriginalName, int SubstituteRId, string SubstituteName);
    
    public record WarmUpDto(int Id, int RId, string ComponentName);
    
    public record BrakiDto(int ComponentId, string ComponentName, int ShortageQuantity, int RId);
    
    public record CreateReservationListRequest(
        string Name,
        int Start,
        bool IsTHT,
        IEnumerable<CreateReservationItemRequest> Items);
    
    public record CreateReservationItemRequest(string ComponentName, string ComponentId, string Quantity);
    
    public record UpdateReservationProgressRequest(int ReservationId, int NewDone);
    
    public record ImportInventoryRequest(IEnumerable<ComponentUpdateDto> Components);
    
    public record ComponentUpdateDto(int RId, string Name, string Type, int Quantity);
    
    public record ImportAlternativesRequest(IEnumerable<AlternativeImportDto> Alternatives);
    
    public record AlternativeImportDto(string OriginalRId, string SubstituteRId);
    
    public record ImportContainersRequest(IEnumerable<string> JsonData);
    
    public record AddPnPDataRequest(
        IEnumerable<PnPDataDto> Data,
        long ListId,
        string FileName,
        string Type);
    
    public record PnPDataDto(int RId, int Quantity);

    public class DatabaseResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }

        public static DatabaseResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        
        public static DatabaseResult<T> Failure(string errorMessage, Exception? exception = null) => 
            new() { IsSuccess = false, ErrorMessage = errorMessage, Exception = exception };
    }

    public class DatabaseResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }

        public static DatabaseResult Success() => new() { IsSuccess = true };
        
        public static DatabaseResult Failure(string errorMessage, Exception? exception = null) => 
            new() { IsSuccess = false, ErrorMessage = errorMessage, Exception = exception };
    }
}