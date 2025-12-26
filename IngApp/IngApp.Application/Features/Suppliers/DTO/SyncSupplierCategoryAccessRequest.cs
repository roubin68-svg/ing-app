using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Application.Features.Suppliers.DTO;

public class SyncSupplierCategoryAccessRequest
{
    public List<int> ProductCategoryIds { get; set; } = [];
}
