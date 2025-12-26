namespace IngApp.Application.Features.Suppliers.DTO
{
    public class CreateSupplierTypeRequest
    {
        public string Name { get; set; } = null!;

        /// <summary>
        /// توضیح نوع تأمین‌کننده (اختیاری)
        /// </summary>
        public string? Description { get; set; }
    }
}
