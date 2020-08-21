using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NubeSync.Client.SQLiteStoreEFCore
{
    public partial class NubeSQLiteDataStoreEFCore
    {
        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await NubeSettings.FindAsync(key).ConfigureAwait(false);

            if (setting != null)
            {
                return setting.Value;
            }

            return null;
        }

        public async Task<bool> SetSettingAsync(string key, string value)
        {
            var setting = await NubeSettings.FindAsync(key).ConfigureAwait(false);

            if (setting == null)
            {
                setting = new NubeSetting() { Id = key };
                await NubeSettings.AddAsync(setting).ConfigureAwait(false);
            }
            else
            {
                NubeSettings.Attach(setting);
                Entry(setting).State = EntityState.Modified;
            }

            setting.Value = value;

            return await SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}