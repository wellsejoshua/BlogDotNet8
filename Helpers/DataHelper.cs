using BlogDotNet8.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogDotNet8.Helpers
{
    public static class DataHelper
    {
        //in Heroku this functions to equivalent to running update-database locally
        //will only run from program.cs if there are changes to be made
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //get an instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            //migration: this is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }

    }
}
