using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Application.Features.Products.DTO;
public class CreateProductAttributeTemplateRequest
{
    public int ProductId { get; set; }

    public List<ProductAttributeTemplateRequirementRequest> Requirements { get; set; } = [];
}

public class ProductAttributeTemplateRequirementRequest
{
    public int AttributeDefinitionId { get; set; }

    public bool IsRequired { get; set; }
}