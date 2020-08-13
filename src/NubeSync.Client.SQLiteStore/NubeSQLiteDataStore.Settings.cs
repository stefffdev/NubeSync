using System.Threading.Tasks;

namespace NubeSync.Client.SQLiteStore
{
    public partial class NubeSQLiteDataStore
    {
        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await Database.FindAsync<NubeSetting>(key);

            if (setting != null)
            {
                return setting.Value;
            }

            return null;
        }

        public async Task<bool> SetSettingAsync(string key, string value)
        {
            var setting = await Database.FindAsync<NubeSetting>(key);

            if (setting == null)
            {
                setting = new NubeSetting() { Id = key, Value = value };
                return await Database.InsertAsync(setting) > 0;
            }
            else
            {
                setting.Value = value;
                return await Database.UpdateAsync(setting) > 0;
            }
        }
    }
}