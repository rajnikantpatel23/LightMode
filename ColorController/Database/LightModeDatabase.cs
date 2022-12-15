using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using ColorController.Models;

namespace ColorController.Database
{
    public class LightModeDatabase
    {
        readonly SQLiteAsyncConnection database;

        public LightModeDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<AnimationModel>().Wait();
            database.CreateTableAsync<FavoriteAnimation>().Wait();
            database.CreateTableAsync<MyColor>().Wait();
            database.CreateTableAsync<Controller>().Wait();
            database.CreateTableAsync<LMD>().Wait();
        }

        #region AnimationModel
        public async Task<List<AnimationModel>> GetAnimationsAsync()
        {
            //Get all animations.
            return await database.Table<AnimationModel>().ToListAsync();
        }

        public Task<AnimationModel> GetAnimationAsync(string id)
        {
            // Get a specific animation.
            return database.Table<AnimationModel>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAnimationAsync(AnimationModel animation)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(animation.Id))
            {
                var existingAnimation = await GetAnimationAsync(animation.Id);
                if (existingAnimation == null)
                {
                    result = await database.InsertAsync(animation);
                }
            }

            return result;
        }

        public async Task<int> UpdateAnimationAsync(AnimationModel animation)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(animation.Id))
            {
                result = await database.UpdateAsync(animation);
            }

            return result;
        }
        
        public async Task<int> UpdateAllAnimationAsync(List<AnimationModel> animation)
        {
            int result = 0;
            result = await database.UpdateAllAsync(animation);
            return result;
        }

        public async Task<int> DeleteAnimationAsync(AnimationModel animation)
        {
            // Delete a animation.
            return await database.DeleteAsync(animation);
        }
        #endregion

        #region FavoriteAnimation
        public async Task<List<FavoriteAnimation>> GetFavoriteAnimationsAsync()
        {
            //Get all animations.
            return await database.Table<FavoriteAnimation>().ToListAsync();
        }

        public Task<FavoriteAnimation> GetFavoriteAnimationAsync(string id)
        {
            // Get a specific animation.
            return database.Table<FavoriteAnimation>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }
       
        public async Task<int> SaveFavoriteAnimationAsync(FavoriteAnimation animation)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(animation.Id))
            {
                var existingAnimation = await GetFavoriteAnimationAsync(animation.Id);
                if (existingAnimation == null)
                {
                    result = await database.InsertAsync(animation);
                }
            }

            return result;
        }
         
        public async Task<int> DeleteFavoriteAnimationAsync(FavoriteAnimation animation)
        {
            // Delete a animation.
            return await database.DeleteAsync(animation);
        }

        public async Task<int> DeleteAllFavoriteAnimationAsync()
        {
            return await database.DeleteAllAsync<FavoriteAnimation>();
        }

        public async Task<int> UppdateFavoriteAnimations(List<FavoriteAnimation> favoriteAnimations)
        {
            return await database.UpdateAllAsync(favoriteAnimations);
        }
        #endregion

        #region MyColor
        public async Task<List<MyColor>> GetMyColorsAsync()
        { 
            return await database.Table<MyColor>().ToListAsync();
        }

        public Task<MyColor> GetMyColorAsync(string id)
        { 
            return database.Table<MyColor>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveMyColorAsync(MyColor myColor)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(myColor.Id))
            {
                var existingAnimation = await GetFavoriteAnimationAsync(myColor.Id);
                if (existingAnimation == null)
                {
                    result = await database.InsertAsync(myColor);
                }
            }

            return result;
        }

        public async Task<int> DeleteMyColorAsync(MyColor myColor)
        { 
            return await database.DeleteAsync(myColor);
        }

        public async Task<int> DeleteAllMyColorAsync()
        {
            return await database.DeleteAllAsync<MyColor>();
        } 
        
        public async Task<int> UppdateMyColors(List<MyColor> myColors)
        {
            return await database.UpdateAllAsync(myColors);
        }

        #endregion 
        
        #region Controller
        public async Task<List<Controller>> GetControllers()
        { 
            return await database.Table<Controller>().ToListAsync();
        }

        public Task<Controller> GetDefaultController()
        {
            return database.Table<Controller>()
                            .Where(i => i.IsDefault == true)
                            .FirstOrDefaultAsync();
        }

        public Task<Controller> GetController(string id)
        { 
            return database.Table<Controller>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveController(Controller controller)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(controller.Id))
            {
                var item = await GetController(controller.Id);
                if (item == null)
                {
                    result = await database.InsertAsync(controller);
                }
            }

            return result;
        }

        public async Task<int> DeleteController(Controller controller)
        { 
            return await database.DeleteAsync(controller);
        }

        public async Task<int> DeleteAllController()
        {
            return await database.DeleteAllAsync<Controller>();
        } 
        
        public async Task<int> UpdateAllController(List<Controller> controllers)
        {
            return await database.UpdateAllAsync(controllers);
        }

        public async Task<int> UpdateController(Controller controller)
        {
            int result = 0;
            if (!string.IsNullOrWhiteSpace(controller.Id))
            {
                result = await database.UpdateAsync(controller);
            }

            return result;
        }

        #endregion

        #region LMD
        public async Task<List<LMD>> GetAllLMD()
        {
            return await database.Table<LMD>().ToListAsync();
        }
 
        public async Task<LMD> GetLMDStatus(string controllerId, string animationId)
        {
            return await database.Table<LMD>().FirstOrDefaultAsync(i => i.ControllerId == controllerId && i.AnimationId == animationId);
        }

        public async Task<int> SaveLMD(LMD lmd)
        {
            int result = 0;
            var existingLmd = await GetLMDStatus(lmd.ControllerId , lmd.AnimationId);
            if (existingLmd == null)
            {
                try
                {
                    result = await database.InsertAsync(lmd);
                }
                catch (System.Exception)
                {
                     
                }
            }
            return result;
        }

        #endregion
    }
}
