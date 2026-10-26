using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface IViewRepository
{
    void CreateView(string dbName, ViewModel view);
    void DeleteView(string dbName, string viewName);
    ViewModel GetView(string dbName, string viewName);
    List<ViewModel> GetAllViews(string dbName);
    void SetViewDescription(string dbName, string viewName, string description);
}