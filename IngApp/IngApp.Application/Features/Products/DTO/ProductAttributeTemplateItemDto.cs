using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Application.Features.Products.DTO;

public class ProductAttributeTemplateItemDto
{
    public int AttributeDefinitionId { get; set; }

    public string DisplayName { get; set; } = null!;

    public int DataType { get; set; }

    public bool IsRequired { get; set; }
}