using System.Collections.Generic;
using System.Threading.Tasks;
using business_layer.Dtos;

namespace business_layer.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<DocumentSearchResultDto>> SearchDocumentsAsync(string searchTerm);
    }
}