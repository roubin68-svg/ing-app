using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Application.Features.Suppliers.DTO;
public class SupplierCategoryAccessDto
{
    public int ProductCategoryId { get; set; }

    public string ProductCategoryTitle { get; set; } = null!;

    public bool IsActive { get; set; }
}
