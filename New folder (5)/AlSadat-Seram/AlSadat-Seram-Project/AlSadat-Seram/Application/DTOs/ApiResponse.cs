namespace AlSadatSeram.Services.contract
{
    public class ApiResponse<T> where T : class
    {
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalPages { get; set; }
        public T data { get; set; }
    }
}
